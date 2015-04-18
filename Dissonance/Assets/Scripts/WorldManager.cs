using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

// TODO(Julian): Centralize the entity simulation; make all entities register with the world on enable
// and simulate when the world wills it

public class WorldManager : MonoBehaviour {

    [SerializeField]
    int _xDim = 40;
    // Right
    [SerializeField]
    int _zDim = 40;
    // Left
    [SerializeField]
    int _yDim = 40;
    // Up
    [SerializeField]
    float _tileSize = 0.5f;

    public float TileSize {
        get { return _tileSize; }
    }

    public IntVector WorldDims {
        get { return new IntVector(_xDim, _yDim, _zDim); }
    }

    Dictionary<int, Dictionary<int, Dictionary<int, WorldEntity>>> _world =
        new Dictionary<int, Dictionary<int, Dictionary<int,WorldEntity>>>();
    bool[,] _xyWorldShadows;
    bool[,] _zyWorldShadows;

    // Things that only exist in 2d (not 3d shadows)
    HashSet<WorldEntity2D>[,] _xyWorldEntities;
    HashSet<WorldEntity2D>[,] _zyWorldEntities;

    public static WorldManager g;
    void Awake () {
        EnsureGExists();
        InitLevel();
    }

    [SerializeField]
    DPlane _planeXY;
    public DPlane PlaneXY {
        get { return _planeXY; }
    }
    [SerializeField]
    DPlane _planeZY;
    public DPlane PlaneZY {
        get { return _planeZY; }
    }
    DPlane[] _planes;
    public DPlane[] Planes {
        get { return _planes; }
    }

    void OnValidate () {
        UpdatePlanes();
    }

    [SerializeField]
    Transform _shadowContainer;
    public Transform ShadowContainer {
        get { return _shadowContainer; }
    }

    public void EnsureGExists () {
        if (WorldManager.g == null) {
            WorldManager.g = this;
        }
        else
        if (WorldManager.g != this) {
            Destroy(this);
        }
    }

    private List<WorldEntity> _worldEntities = new List<WorldEntity>();
    private List<WorldEntity2D> _worldEntities2D = new List<WorldEntity2D>();

    public void RegisterEntity (WorldEntity e) {
        _worldEntities.Add(e);
        List<IntVector> all = e.AbsoluteLocations(e.Location, e.Rotation);
        Set3DLocationsToEntity(all, e);
        UpdateShadows();
    }

    public void RegisterEntity (WorldEntity2D e) {
        List<IntVector2D> all = e.AbsoluteLocations(e.Location);
        _worldEntities2D.Add(e);
        AddEntityTo2DLocations(all, e.Orientation, e);
    }

    public void DeregisterEntity (WorldEntity e) {
        List<IntVector> all = e.AbsoluteLocations(e.Location, e.Rotation);
        Set3DLocationsToEntity(all, null);
        _worldEntities.Remove(e);
        UpdateShadows();
    }

    public void DeregisterEntity (WorldEntity2D e) {
        List<IntVector2D> all = e.AbsoluteLocations(e.Location);
        AddEntityTo2DLocations(all, e.Orientation, null);
        _worldEntities2D.Remove(e);
    }

    bool _initialized = false;
    void InitLevel () {
        // TODO(JULIAN): Load dimensions
        // _xDim = 40; // Right
        // _zDim = 40; // Left
        // _yDim = 40; // Up

        // TODO(JULIAN): Load _world
        // for (int y = 0; y < _yDim; y++) {
        // 	for (int x = 0; x < _xDim; x++) {
        // 		for (int z = 0; z < _zDim; z++) {
        // 				SetContentsAt(x,y,z, null);
        // 		}
        // 	}
        // }
        // for (int i = 0; i < 100; i++) {
        // 	SetContentsAt((int)(Random.value * _xDim), (int)(Random.value * _yDim), (int)(Random.value * _zDim), new TileContents());
        // }
        _xyWorldEntities = new HashSet<WorldEntity2D>[_xDim, _yDim];
        _zyWorldEntities = new HashSet<WorldEntity2D>[_zDim, _yDim];
        _xyWorldShadows = new bool[_xDim, _yDim];
        _zyWorldShadows = new bool[_zDim, _yDim];
        UpdatePlanes();
        _initialized = true;
        UpdateShadows();
    }

    private void UpdatePlanes () {
        if (_planes == null) {
            _planes = new DPlane[2];
        }
        _planeXY.Init(_xDim, _yDim, _zDim);
        _planeZY.Init(_xDim, _yDim, _zDim);
        _planes[0] = _planeXY;
        _planes[1] = _planeZY;
    }

    // NOTE(Julian): This is the frame bottleneck; optimization potential
    private void UpdateShadows () {

        if (_xyWorldShadows == null)
            _xyWorldShadows = new bool[_xDim, _yDim];
        if (_zyWorldShadows == null)
            _zyWorldShadows = new bool[_zDim, _yDim];

        // Fill _zyWorldShadows map
        for (int y = 0; y < _yDim; y++) {
            for (int z = 0; z < _zDim; z++) {
                _zyWorldShadows[z, y] = true;
                for (int x = 0; x < _xDim; x++) {
                    if (CastsShadowsAt3D(x, y, z)) {
                        _zyWorldShadows[z, y] = false;
                        break;
                    }
                }
            }
        }
        // Fill _xyWorldShadows map
        for (int y = 0; y < _yDim; y++) {
            for (int x = 0; x < _xDim; x++) {
                _xyWorldShadows[x, y] = true;
                for (int z = 0; z < _zDim; z++) {
                    if (CastsShadowsAt3D(x, y, z)) {
                        _xyWorldShadows[x, y] = false;
                        break;
                    }
                }
            }
        }
    }

    private bool CanExistOn (List<IntVector2D> relativeLocations, IntVector2D on, PlaneOrientation orientation) {
        int i;
        switch (orientation) {
            case PlaneOrientation.XY:
                for (i = 0; i < relativeLocations.Count; i++) {
                    if (!PassableAtXY(relativeLocations[i] + on)) { return false; }
                }
                break;
            case PlaneOrientation.ZY:
                for (i = 0; i < relativeLocations.Count; i++) {
                    if (!PassableAtZY(relativeLocations[i] + on)) { return false; }
                }
                break;
        }
        return true;
    }

    public bool CanMoveByDelta (WorldEntity2D e, IntVector2D v) {
        IntVector2D resLoc = e.Location + v;
        List<IntVector2D> occupiedAfter = e.AbsoluteLocations(resLoc);

        // TODO(Julian): Take more than just shadows into account!

        int i;
        switch (e.Orientation) {
            case PlaneOrientation.XY:
                for (i = 0; i < occupiedAfter.Count; i++) {
                    if (!PassableAtXY(occupiedAfter[i])) { return false; }
                }
                break;
            case PlaneOrientation.ZY:
                for (i = 0; i < occupiedAfter.Count; i++) {
                    if (!PassableAtZY(occupiedAfter[i])) { return false; }
                }
                break;
        }

        return true;
    }

    public bool CanJumpByDelta (WorldEntity2D e, IntVector2D v) {
        // First Move Up, then Sideways!

        int maxHoriz = v[0];
        int maxVert = v[1];

        int vertDir = Mathf.RoundToInt(Mathf.Sign(maxVert));
        int horizDir = Mathf.RoundToInt(Mathf.Sign(maxHoriz));

        int vert = 0;
        while (Mathf.Abs(vert) < Mathf.Abs(maxVert)) {
            vert += vertDir;
            if (!CanMoveByDelta(e, new IntVector2D(0, vert))) { return false; }
        }
        int horiz = 0;
        while (Mathf.Abs(horiz) < Mathf.Abs(maxHoriz)) {
            horiz += horizDir;
            if (!CanMoveByDelta(e, new IntVector2D(horiz, vert))) { return false; }
        }
        return true;
    }

    bool PassableAtXY (IntVector2D v) {
        return PassableAtXY(v[0], v[1]);
    }

    bool PassableAtXY (int x, int y) {
        if (x >= _xDim || y >= _yDim || x < 0 || y < 0) { return false; }
        return _xyWorldShadows[x, y];
    }

    bool PassableAtZY (IntVector2D v) {
        return PassableAtZY(v[0], v[1]);
    }

    bool PassableAtZY (int z, int y) {
        if (z >= _zDim || y >= _yDim || z < 0 || y < 0) { return false; }
        return _zyWorldShadows[z, y];
    }

    bool PassableAt2D (int a, int y, PlaneOrientation orientation) {
        switch (orientation) {
            case PlaneOrientation.XY:
                return PassableAtXY(a, y);
            case PlaneOrientation.ZY:
                return PassableAtZY(a, y);
            default:
                return false;
        }
    }

    bool PassableAt2D (IntVector2D v, PlaneOrientation orientation) {
        return PassableAt2D(v[0], v[1], orientation);
    }

    bool CastsShadowsAt3D (int x, int y, int z) {
        if (x < 0 || y < 0 || z < 0 ||
        x >= _xDim || y >= _yDim || z >= _zDim) {
            return false;
        }
        WorldEntity c = ContentsAt(x, y, z);
        if (c != null) {
            return c.CastsShadows;
        }
        return false;
    }

    private void AddEntityTo2DLocations (List<IntVector2D> locations, PlaneOrientation planeOrientation, WorldEntity2D e) {
        for (int i = 0; i < locations.Count; i++) {
            Add2DEntityTo(locations[i], planeOrientation, e);
        }
    }

    private void RemoveEntityFrom2DLocations (List<IntVector2D> locations, PlaneOrientation planeOrientation, WorldEntity2D e) {
        for (int i = 0; i < locations.Count; i++) {
            Remove2DEntityFrom(locations[i], planeOrientation, e);
        }
    }

    private void Set3DLocationsToEntity (List<IntVector> locations, WorldEntity e) {
        for (int i = 0; i < locations.Count; i++) {
            SetContentsAt(locations[i].x, locations[i].y, locations[i].z, e);
        }
    }

    public HashSet<WorldEntity2D> Contents2DAt (IntVector2D v, PlaneOrientation planeOrientation) {
    	var map = Map2DForOrientation(planeOrientation);
    	var contents = map[v[0], v[1]];
    	if (contents == null) {
    		contents = new HashSet<WorldEntity2D>();
    		map[v[0], v[1]] = contents;
    	}
    	return contents;
    }

    public WorldEntity ContentsAt (IntVector v) {
        return ContentsAt(v.x, v.y, v.z);
    }

    public WorldEntity ContentsAt (int x, int y, int z) {
        if (_world.ContainsKey(y)) {
            if (_world[y].ContainsKey(x)) {
                if (_world[y][x].ContainsKey(z)) {
                    return _world[y][x][z];
                }
            }
        }
        return null;
    }

    private HashSet<WorldEntity2D>[,] Map2DForOrientation (PlaneOrientation planeOrientation) {
        switch (planeOrientation) {
            case PlaneOrientation.XY:
            	return _xyWorldEntities;
            case PlaneOrientation.ZY:
				return _zyWorldEntities;
        }
        return null; // Compiler isn't smart enough to know this will never happen
    }

    private void Add2DEntityTo (IntVector2D v, PlaneOrientation planeOrientation, WorldEntity2D t) {
    	HashSet<WorldEntity2D>[,] mapToModify = Map2DForOrientation(planeOrientation);
    	var contents = mapToModify[v[0], v[1]];
    	if (contents == null) {
    		contents = new HashSet<WorldEntity2D>();
    		mapToModify[v[0], v[1]] = contents;
    	}
    	contents.Add(t);
    }

    private void Remove2DEntityFrom (IntVector2D v, PlaneOrientation planeOrientation, WorldEntity2D t) {
    	HashSet<WorldEntity2D>[,] mapToModify = Map2DForOrientation(planeOrientation);
    	var contents = mapToModify[v[0], v[1]];
    	if (contents == null) {
    		contents = new HashSet<WorldEntity2D>();
    		mapToModify[v[0], v[1]] = contents;
    	}
    	contents.Remove(t);
    }

    private void SetContentsAt (IntVector v, WorldEntity t) {
        SetContentsAt(v.x, v.y, v.z, t);
    }

    private void SetContentsAt (int x, int y, int z, WorldEntity t) {
        if (!_world.ContainsKey(y)) {
            _world[y] = new Dictionary<int, Dictionary<int, WorldEntity>>();
        }
        if (!_world[y].ContainsKey(x)) {
            _world[y][x] = new Dictionary<int, WorldEntity>();
        }
        _world[y][x][z] = t;
    }


    private bool IsEntity2DCovered (WorldEntity2D e) {
        List<IntVector2D> locations = e.AbsoluteLocations(e.Location);
        switch (e.Orientation) {
            case PlaneOrientation.XY:
                for (int i = 0; i < locations.Count; i++) {
                    if (!PassableAtXY(locations[i])) { return false; }
                }
                break;
            case PlaneOrientation.ZY:
                for (int i = 0; i < locations.Count; i++) {
                    if (!PassableAtZY(locations[i])) { return false; }
                }
                break;
            default:
                break;
        }
        return true;
    }

    float HeuristicCostEstimate (IntVector2D startNode, IntVector2D goalNode) {
        return Vector2.Distance(startNode.ToVector2(), goalNode.ToVector2());
        // return Mathf.Abs(startNode.x - goalNode.x) + Mathf.Abs(startNode.y - goalNode.y);
    }

    private float CostBetween (IntVector2D startNode, IntVector2D neighborNode) {
        return Mathf.Abs(startNode.x - neighborNode.x) + Mathf.Abs(startNode.y - neighborNode.y);
    }

    public IntVector2D CoerceToBounds2D (IntVector2D vec, PlaneOrientation orientation) {
        IntVector2D res = vec;
        res.y = Mathf.Min(Mathf.Max(0, res.y), _yDim - 1);
        switch (orientation) {
            case PlaneOrientation.XY:
                res.x = Mathf.Min(Mathf.Max(0, res.x), _xDim - 1);
                break;
            case PlaneOrientation.ZY:
                res[0] = Mathf.Min(Mathf.Max(0, res[0]), _zDim - 1);
                break;
            default:
                break;
        }
        return res;
    }

    public bool IsInBounds2D (int a, int y, PlaneOrientation orientation) {
        if (y >= _yDim || y < 0) { return false; }
        switch (orientation) {
            case PlaneOrientation.XY:
                if (a >= _xDim || a < 0) { return false; }
                break;
            case PlaneOrientation.ZY:
                if (a >= _zDim || a < 0) { return false; }
                break;
            default:
                break;
        }
        return true;
    }

    public bool IsInBounds2D (IntVector2D vec, PlaneOrientation orientation) {
        return IsInBounds2D(vec[0], vec.y, orientation);
    }

    private List<IntVector2D> ConnectedNodesForRelativeLocations (List<IntVector2D> relativeLocations, IntVector2D node, PlaneOrientation orientation) {
        var connected = new List<IntVector2D>();
        IntVector2D below = node + new IntVector2D(0, -1);
        if (IsInBounds2D(below.x, below.y, orientation) &&
        CanExistOn(relativeLocations, below, orientation)) {
            connected.Add(below);
            return connected; // Enforce falling if possible
        }

        // Move left and right
        for (int i = -1; i <= 1; i += 2) {
            int horizPos = node[0] + i;
            IntVector2D dest = new IntVector2D(horizPos, node.y);
            if (IsInBounds2D(horizPos, node.y, orientation) &&
            CanExistOn(relativeLocations, dest, orientation)) {
                connected.Add(dest);
            }
            else { // Move step up
                int vertPos = node[1] + 1;
                dest = new IntVector2D(horizPos, vertPos);
                if (IsInBounds2D(horizPos, node.y, orientation) &&
                CanExistOn(relativeLocations, dest, orientation)) {
                    connected.Add(dest);
                }
            }
        }

        return connected;
    }

    // Adapted From http://stackoverflow.com/questions/10983110/a-star-a-and-generic-find-method
    private class Path<Node> : IEnumerable<Node> {
        public Node LastStep { get; private set; }

        public Path<Node> PreviousSteps { get; private set; }

        public float TotalCost { get; private set; }

        private Path(Node lastStep, Path<Node> previousSteps, float totalCost) {
            LastStep = lastStep;
            PreviousSteps = previousSteps;
            TotalCost = totalCost;
        }

        public Path(Node start)
            : this(start, null, 0) {
        }
        public Path<Node> AddStep (Node step, float stepCost) {
            return new Path<Node>(step, this, TotalCost + stepCost);
        }

        public IEnumerator<Node> GetEnumerator () {
            for (Path<Node> p = this; p != null; p = p.PreviousSteps)
                yield return p.LastStep;
        }

        IEnumerator IEnumerable.GetEnumerator () {
            return this.GetEnumerator();
        }

        public List<Node> AsList {
            get {
                List<Node> nodes = new List<Node>();
                foreach (Node n in this) {
                    nodes.Add(n);
                }
                return nodes;
            }
        }
    }

    // Adapted From http://stackoverflow.com/questions/10983110/a-star-a-and-generic-find-method
    public List<IntVector2D> PlanPath (Char2D entity, IntVector2D start, IntVector2D destination) {
        PlaneOrientation orientation = entity.Orientation;
        List<IntVector2D> relativeLocations = entity.AbsoluteLocations(new IntVector2D(0, 0));
        var closed = new HashSet<IntVector2D>();
        var queue = new PriorityQueue<Path<IntVector2D>, float>();
        queue.Enqueue(new Path<IntVector2D>(start), 0f);
        while (!queue.IsEmpty) {
            var path = queue.Dequeue();
            if (closed.Contains(path.LastStep))
                continue;
            if (path.LastStep == destination)
                return path.AsList;
            IntVector2D leafNode = path.LastStep;
            closed.Add(leafNode);
            List<IntVector2D> neighbors = ConnectedNodesForRelativeLocations(relativeLocations, leafNode, orientation);
            for (int i = 0; i < neighbors.Count; i++) {
                IntVector2D n = neighbors[i];
                float stepCost = CostBetween(path.LastStep, n);
                var newPath = path.AddStep(n, stepCost);
                queue.Enqueue(newPath, newPath.TotalCost + HeuristicCostEstimate(n, destination));
            }
        }
        return null;
    }

    private IntVector2D MouseLocOnPlane (DPlane plane) {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        float rayDistance;
        if (plane.MathPlane.Raycast(ray, out rayDistance)) {
            Vector3 planePos = ray.GetPoint(rayDistance);
            Vector2 planePos2D = ProjectionMath.TwoDimCoordsOnPlane(planePos, plane);
            IntVector2D tileLoc = new IntVector2D(planePos2D / _tileSize - Vector2.one * _tileSize / 2f);
            if (plane.Orientation == PlaneOrientation.XY) {
                tileLoc.x *= -1;
                tileLoc.x -= 1;
            }
            return tileLoc;
        }
        return new IntVector2D(-1, -1);
    }

    public IntVector2D MouseLocOnPlaneXY () {
        return MouseLocOnPlane(_planeXY);
    }

    public IntVector2D MouseLocOnPlaneZY () {
        return MouseLocOnPlane(_planeZY);
    }

    public Vector3 WorldPosFromIntVecXY (IntVector2D vec) {
        return WorldPosFromIntVec(vec, PlaneOrientation.XY);
    }

    public Vector3 WorldPosFromIntVecZY (IntVector2D vec) {
        return WorldPosFromIntVec(vec, PlaneOrientation.ZY);
    }

    public Vector3 WorldPosFromIntVec (IntVector2D vec, PlaneOrientation orientation) {
        float tileSize = WorldManager.g.TileSize;
        Vector2 vector2 = vec.ToVector2();
        if (orientation == PlaneOrientation.XY) {
            vector2.x *= -1f;
            vector2.x -= 1f;
        }
        if (orientation == PlaneOrientation.XY) {
            return ProjectionMath.ThreeDimCoordsOnPlane(vector2 * tileSize + Vector2.one * tileSize / 2f, _planeXY);
        }
        else {
            return ProjectionMath.ThreeDimCoordsOnPlane(vector2 * tileSize + Vector2.one * tileSize / 2f, _planeZY);
        }
    }

    void Update () {
        Profiler.BeginSample("2d sim");
        for (int i = 0; i < _worldEntities2D.Count; i++) {
            WorldEntity2D entity = _worldEntities2D[i];
            Profiler.BeginSample("Clear 2d loc");
            RemoveEntityFrom2DLocations(entity.AbsoluteLocations(entity.Location), entity.Orientation, entity);
            Profiler.EndSample();
            Profiler.BeginSample("Simulate 2d entity");
            entity.Simulate();
            Profiler.EndSample();
            Profiler.BeginSample("Fill 2d loc");
            AddEntityTo2DLocations(entity.AbsoluteLocations(entity.Location), entity.Orientation, entity);
            Profiler.EndSample();
        }
        Profiler.EndSample();

        Profiler.BeginSample("3d sim");
        for (int i = 0; i < _worldEntities.Count; i++) {
            WorldEntity entity = _worldEntities[i];
            Profiler.BeginSample("Clear 3d loc");
            List<IntVector> all = entity.AbsoluteLocations(entity.Location, entity.Rotation);
            Set3DLocationsToEntity(all, null);
            Profiler.EndSample();
            Profiler.BeginSample("Simulate 3d entity");
            entity.Simulate();
            Profiler.EndSample();
            Profiler.BeginSample("Fill 3d loc");
            all = entity.AbsoluteLocations(entity.Location, entity.Rotation);
            Set3DLocationsToEntity(all, entity);
            Profiler.EndSample();
        }
        Profiler.EndSample();
        Profiler.BeginSample("Shadow Sim");
        UpdateShadows();
        Profiler.EndSample();
    }

    void OnDrawGizmos () {
        if (!Application.isPlaying) {
            EnsureGExists(); // For Debugging!
        }
        if (!_initialized)
            return;
        for (int y = 0; y < _yDim; y++) {
            for (int x = 0; x < _xDim; x++) {
                for (int z = 0; z < _zDim; z++) {
                    if (ContentsAt(x, y, z) != null) {
                        Gizmos.DrawSphere(new Vector3(x + 0.5f, y + 0.5f, z + 0.5f) * _tileSize, _tileSize / 2);
                    }
                }
            }
        }
        for (int y = 0; y < _yDim; y++) {
            for (int x = 0; x < _xDim; x++) {
                if (!_xyWorldShadows[x, y]) {
                    Gizmos.color = Color.black;
                    Gizmos.DrawLine(new Vector3(_tileSize * (x), _tileSize * (y), 0f),
                        new Vector3(_tileSize * (x + 1f), _tileSize * (y + 1f), 0f));
                    Gizmos.DrawLine(new Vector3(_tileSize * (x), _tileSize * (y + 1f), 0f),
                        new Vector3(_tileSize * (x + 1f), _tileSize * (y), 0f));
                }
                if (_xyWorldEntities[x, y] != null && _xyWorldEntities[x, y].Count > 0) {
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(new Vector3(_tileSize * (x), _tileSize * (y), 0f),
                        new Vector3(_tileSize * (x + 1f), _tileSize * (y + 1f), 0f));
                    Gizmos.DrawLine(new Vector3(_tileSize * (x), _tileSize * (y + 1f), 0f),
                        new Vector3(_tileSize * (x + 1f), _tileSize * (y), 0f));
                }
            }
        }
        for (int y = 0; y < _yDim; y++) {
            for (int z = 0; z < _zDim; z++) {
                if (!_zyWorldShadows[z, y]) {
                    Gizmos.color = Color.black;
                    Gizmos.DrawLine(new Vector3(0f, _tileSize * (y), _tileSize * (z)),
                        new Vector3(0f, _tileSize * (y + 1f), _tileSize * (z + 1f)));
                    Gizmos.DrawLine(new Vector3(0f, _tileSize * (y + 1f), _tileSize * (z)),
                        new Vector3(0f, _tileSize * (y), _tileSize * (z + 1f)));
                }
                if (_zyWorldEntities[z, y] != null && _zyWorldEntities[z, y].Count > 0) {
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(new Vector3(0f, _tileSize * (y), _tileSize * (z)),
                        new Vector3(0f, _tileSize * (y + 1f), _tileSize * (z + 1f)));
                    Gizmos.DrawLine(new Vector3(0f, _tileSize * (y + 1f), _tileSize * (z)),
                        new Vector3(0f, _tileSize * (y), _tileSize * (z + 1f)));
                }
            }
        }
    }

    void Serialize () {
        // This should be an intvector!
        // xdim
        // ydim
        // zdim
        // _tileSize
        // private List<WorldEntity> _worldEntities = new List<WorldEntity>();
        // private List<WorldEntity2D> _worldEntities2D = new List<WorldEntity2D>();

    }
}
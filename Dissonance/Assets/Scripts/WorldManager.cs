using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// TODO(Julian): Centralize the entity simulation; make all entities register with the world on enable
// and simulate when the world wills it

public class WorldManager : MonoBehaviour {

	[SerializeField]
	int _xDim = 40; // Right
	[SerializeField]
	int _zDim = 40; // Left
	[SerializeField]
	int _yDim = 40; // Up
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
	WorldEntity2D[,] _xyWorldEntities;
	WorldEntity2D[,] _zyWorldEntities;

	public static WorldManager g;
	void Awake () {
		EnsureGExists();
		InitLevel();
	}


	[SerializeField]
	Plane[] _planes;
	public Plane[] Planes {
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
		} else if (WorldManager.g != this) {
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
		Set2DLocationsToEntity(all, e.Orientation, e);
	}

	public void DeregisterEntity (WorldEntity e) {
		List<IntVector> all = e.AbsoluteLocations(e.Location, e.Rotation);
		Set3DLocationsToEntity(all, null);
		_worldEntities.Remove(e);
		UpdateShadows();
	}

	public void DeregisterEntity (WorldEntity2D e) {
		List<IntVector2D> all = e.AbsoluteLocations(e.Location);
		Set2DLocationsToEntity(all, e.Orientation, null);
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
		_xyWorldEntities = new WorldEntity2D[_xDim,_yDim];
		_zyWorldEntities = new WorldEntity2D[_zDim,_yDim];
		_xyWorldShadows = new bool[_xDim,_yDim];
		_zyWorldShadows = new bool[_zDim,_yDim];
		UpdatePlanes();
		_initialized = true;
		UpdateShadows();
	}

	private void UpdatePlanes () {
		for (int i = 0; i < _planes.Length; i++) {
			_planes[i].Init(_xDim, _yDim, _zDim);
		}
	}

	private void UpdateShadows () {

		if (_xyWorldShadows == null) _xyWorldShadows = new bool[_xDim,_yDim];
		if (_zyWorldShadows == null) _zyWorldShadows = new bool[_zDim,_yDim];

		// Fill _zyWorldShadows map
		for (int y = 0; y < _yDim; y++) {
			for (int z = 0; z < _zDim; z++) {
				_zyWorldShadows[z,y] = true;
				for (int x = 0; x < _xDim; x++) {
					if (CastsShadowsAt3D(x,y,z)) {
						_zyWorldShadows[z,y] = false;
						break;
					}
				}
			}
		}
		// Fill _xyWorldShadows map
		for (int y = 0; y < _yDim; y++) {
			for (int x = 0; x < _xDim; x++) {
				_xyWorldShadows[x,y] = true;
				for (int z = 0; z < _zDim; z++) {
					if (CastsShadowsAt3D(x,y,z)) {
						_xyWorldShadows[x,y] = false;
						break;
					}
				}
			}
		}
	}


	public bool CanMoveByDelta(WorldEntity2D e, IntVector2D v) {
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

	public bool CanJumpByDelta(WorldEntity2D e, IntVector2D v) {
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

	bool PassableAtXY(IntVector2D v) {
		return PassableAtXY(v[0], v[1]);
	}

	bool PassableAtXY(int x, int y) {
		if (x >= _xDim || y >= _yDim || x < 0 || y < 0) { return false; }
		return _xyWorldShadows[x,y];
	}

	bool PassableAtZY(IntVector2D v) {
		return PassableAtZY(v[0], v[1]);
	}

	bool PassableAtZY(int z, int y) {
		if (z >= _zDim || y >= _yDim || z < 0 || y < 0) { return false; }
		return _zyWorldShadows[z,y];
	}

	bool CastsShadowsAt3D(int x, int y, int z) {
		if (x < 0 || y < 0 || z < 0 ||
			x >= _xDim || y >= _yDim || z >= _zDim) {
			return false;
		}
		WorldEntity c = ContentsAt(x,y,z);
		if (c != null) {
			return c.CastsShadows;
		}
		return false;
	}

	private void Set2DLocationsToEntity (List<IntVector2D> locations, PlaneOrientation planeOrientation, WorldEntity2D e) {
		for (int i = 0; i < locations.Count; i++) {
			SetContents2DAt(locations[i], planeOrientation, e);
		}
	}

	private void Set3DLocationsToEntity (List<IntVector> locations, WorldEntity e) {
		for (int i = 0; i < locations.Count; i++) {
			SetContentsAt(locations[i].x, locations[i].y, locations[i].z, e);
		}
	}

	public WorldEntity2D Contents2DAt(IntVector2D v, PlaneOrientation planeOrientation) {
		switch (planeOrientation) {
			case PlaneOrientation.XY:
				return _xyWorldEntities[v[0],v[1]];
			case PlaneOrientation.ZY:
				return _zyWorldEntities[v[0],v[1]];
			default:
				return null;
		}
	}

	public WorldEntity ContentsAt(IntVector v) {
		return ContentsAt(v.x, v.y, v.z);
	}

	public WorldEntity ContentsAt(int x, int y, int z) {
		if (_world.ContainsKey(y)) {
			if (_world[y].ContainsKey(x)) {
				if (_world[y][x].ContainsKey(z)) {
					return _world[y][x][z];
				}
			}
		}
		return null;
	}

	private void SetContents2DAt(IntVector2D v, PlaneOrientation planeOrientation, WorldEntity2D t) {
		switch (planeOrientation) {
			case PlaneOrientation.XY:
				_xyWorldEntities[v[0],v[1]] = t;
			break;
			case PlaneOrientation.ZY:
				_zyWorldEntities[v[0],v[1]] = t;
			break;
			default:
			break;
		}
	}

	private void SetContentsAt(IntVector v, WorldEntity t) {
		SetContentsAt(v.x, v.y, v.z, t);
	}

	private void SetContentsAt(int x, int y, int z, WorldEntity t) {
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

	void OnDrawGizmos () {
		if (!Application.isPlaying) {
			EnsureGExists(); // For Debugging!
		}
		if (!_initialized) return;
		for (int y = 0; y < _yDim; y++) {
			for (int x = 0; x < _xDim; x++) {
				for (int z = 0; z < _zDim; z++) {
					if (ContentsAt(x,y,z) != null) {
						Gizmos.DrawSphere(new Vector3(x+0.5f,y+0.5f,z+0.5f) * _tileSize, _tileSize/2);
					}
				}
			}
		}
		for (int y = 0; y < _yDim; y++) {
			for (int x = 0; x < _xDim; x++) {
				if (!_xyWorldShadows[x,y]) {
					Gizmos.color = Color.black;
					Gizmos.DrawLine(new Vector3(_tileSize * (x), _tileSize * (y), 0f),
									new Vector3(_tileSize * (x + 1f), _tileSize * (y + 1f), 0f));
					Gizmos.DrawLine(new Vector3(_tileSize * (x), _tileSize * (y + 1f), 0f),
									new Vector3(_tileSize * (x + 1f), _tileSize * (y), 0f));
				}
				if (_xyWorldEntities[x,y] != null) {
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
				if (!_zyWorldShadows[z,y]) {
					Gizmos.color = Color.black;
					Gizmos.DrawLine(new Vector3(0f, _tileSize * (y), _tileSize * (z)),
									new Vector3(0f, _tileSize * (y + 1f), _tileSize * (z + 1f)));
					Gizmos.DrawLine(new Vector3(0f, _tileSize * (y + 1f), _tileSize * (z)),
									new Vector3(0f, _tileSize * (y), _tileSize * (z + 1f)));
				}
				if (_zyWorldEntities[z,y] != null) {
					Gizmos.color = Color.green;
					Gizmos.DrawLine(new Vector3(0f, _tileSize * (y), _tileSize * (z)),
									new Vector3(0f, _tileSize * (y + 1f), _tileSize * (z + 1f)));
					Gizmos.DrawLine(new Vector3(0f, _tileSize * (y + 1f), _tileSize * (z)),
									new Vector3(0f, _tileSize * (y), _tileSize * (z + 1f)));
				}
			}
		}
	}

	void Update () {
		Profiler.BeginSample("2d sim");
		for (int i = 0; i < _worldEntities2D.Count; i++) {
			WorldEntity2D entity = _worldEntities2D[i];
			Profiler.BeginSample("Clear 2d loc");
			Set2DLocationsToEntity(entity.AbsoluteLocations(entity.Location), entity.Orientation, null);
			Profiler.EndSample();
			Profiler.BeginSample("Simulate 2d entity");
			entity.Simulate();
			Profiler.EndSample();
			Profiler.BeginSample("Fill 2d loc");
			Set2DLocationsToEntity(entity.AbsoluteLocations(entity.Location), entity.Orientation, entity);
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
}
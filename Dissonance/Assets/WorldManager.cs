using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// TODO(Julian): Centralize the entity simulation; make all entities register with the world on enable
// and simulate when the world wills it

public class WorldManager : MonoBehaviour {

	int _xDim = 40; // Right
	int _zDim = 40; // Left
	int _yDim = 40; // Up
	float _tileSize = 0.5f;

	public float TileSize {
		get { return _tileSize; }
	}

	Dictionary<int, Dictionary<int, Dictionary<int, WorldEntity>>> _world =
		new Dictionary<int, Dictionary<int, Dictionary<int,WorldEntity>>>();
	bool[,] _xyWorldShadows;
	bool[,] _zyWorldShadows;

	// Things that only exist in 2d (not 3d shadows)
	WorldEntity2D[,] _xyWorldEntities;
	WorldEntity2D[,] _zyWorldEntities;

	float timeStepDuration = 0.2f;

	public static WorldManager g;
	void Awake () {
		if (g == null) {
			g = this;
			InitLevel();
		} else {
			Destroy(this);
		}
	}

	private List<WorldEntity> _worldEntities = new List<WorldEntity>();
	private List<WorldEntity2D> _worldEntities2D = new List<WorldEntity2D>();

	public void RegisterEntity (WorldEntity e) {
		_worldEntities.Add(e);

		List<IntVector> all = e.AbsoluteLocations(e.Location, e.Rotation);
		for (int i = 0; i < all.Count; i++) {
			SetContentsAt(all[i].x, all[i].y, all[i].z, e);
		}
		UpdateShadows();
	}

	public void RegisterEntity (WorldEntity2D e, PlaneOrientation planeOrientation) {
		List<IntVector2D> all = e.AbsoluteLocations(e.Location);
		_worldEntities2D.Add(e);
		for (int i = 0; i < all.Count; i++) {
			SetContents2DAt(all[i], planeOrientation, e);
		}
	}

	public void DeRegisterEntity (WorldEntity e) {
		_worldEntities.Remove(e);
	}

	public void DeRegisterEntity (WorldEntity2D e) {
		_worldEntities2D.Remove(e);
	}

	bool _initialized = false;
	void InitLevel () {
		// TODO(JULIAN): Load dimensions
		int _xDim = 40; // Right
		int _zDim = 40; // Left
		int _yDim = 40; // Up

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
		_zyWorldEntities = new WorldEntity2D[_xDim,_yDim];
		_xyWorldShadows = new bool[_xDim,_yDim];
		_zyWorldShadows = new bool[_zDim,_yDim];
		_initialized = true;
		UpdateShadows();
	}

	private void UpdateShadows () {

		if (_xyWorldShadows == null) _xyWorldShadows = new bool[_xDim,_yDim];
		if (_zyWorldShadows == null) _zyWorldShadows = new bool[_zDim,_yDim];

		// Fill _zyWorldShadows map
		for (int y = 0; y < _yDim; y++) {
			for (int z = 0; z < _zDim; z++) {
				_zyWorldShadows[z,y] = true;
				for (int x = 0; x < _xDim; x++) {
					if (!PassableAt3D(x,y,z)) {
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
					if (!PassableAt3D(x,y,z)) {
						_xyWorldShadows[x,y] = false;
						break;
					}
				}
			}
		}
	}


	private bool CanMoveByDelta(WorldEntity2D e, IntVector2D v) {
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

	bool PassableAt3D(int x, int y, int z) {
		if (x < 0 || y < 0 || z < 0 ||
			x >= _xDim || y >= _yDim || z >= _zDim) {
			return false;
		}
		WorldEntity c = ContentsAt(x,y,z);
		if (c != null) {
			return c.Passable;
		}
		return true;
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
		if (!_initialized) return;
		for (int y = 0; y < _yDim; y++) {
			for (int x = 0; x < _xDim; x++) {
				for (int z = 0; z < _zDim; z++) {
					if (ContentsAt(x,y,z) != null) {
						Gizmos.DrawSphere(new Vector3(x,y,z) * _tileSize, _tileSize/2);
					}
				}
			}
		}
		for (int y = 0; y < _yDim; y++) {
			for (int x = 0; x < _xDim; x++) {
				if (!_xyWorldShadows[x,y]) {
					Gizmos.DrawLine(new Vector3(_tileSize * (x - 0.5f), _tileSize * (y - 0.5f), 0f),
									new Vector3(_tileSize * (x + 0.5f), _tileSize * (y + 0.5f), 0f));
					Gizmos.DrawLine(new Vector3(_tileSize * (x - 0.5f), _tileSize * (y + 0.5f), 0f),
									new Vector3(_tileSize * (x + 0.5f), _tileSize * (y - 0.5f), 0f));
				}
			}
		}
		for (int y = 0; y < _yDim; y++) {
			for (int z = 0; z < _zDim; z++) {
				if (!_zyWorldShadows[z,y]) {
					Gizmos.DrawLine(new Vector3(0f, _tileSize * (y - 0.5f), _tileSize * (z - 0.5f)),
									new Vector3(0f, _tileSize * (y + 0.5f), _tileSize * (z + 0.5f)));
					Gizmos.DrawLine(new Vector3(0f, _tileSize * (y + 0.5f), _tileSize * (z - 0.5f)),
									new Vector3(0f, _tileSize * (y - 0.5f), _tileSize * (z + 0.5f)));
				}
			}
		}
	}

	void Update () {
		for (int i = 0; i < _worldEntities2D.Count; i++) {
			WorldEntity2D entity = _worldEntities2D[i];
			StateInformation eState = entity.StateInfo;


			if (eState.state == State.Idle) {
				IntVector2D delta = new IntVector2D(0,-1);
				if (CanMoveByDelta(entity, delta)) {
					if (eState.state != State.Falling) {
						entity.Fall();
					}
				} else if (eState.state == State.Falling) {
					entity.Land();
				} else if (entity.DesiredInput.x > 0f) {
					delta = new IntVector2D(1,0);
					if (CanMoveByDelta(entity, delta)) {
						entity.WalkInDirBy(FacingDir.Right, delta);
					}
				} else if (entity.DesiredInput.x < 0f) {
					delta = new IntVector2D(-1,0);
					if (CanMoveByDelta(entity, delta)) {
						entity.WalkInDirBy(FacingDir.Left, delta);
					}
				}
			}

			// Vector2 desDelta = entity.DesiredDelta;
			// Vector2 visOffset = entity.VisualOffset;
			// IntVector2D sqareDelta = new IntVector2D();
			// float margin = 0.0001f;
			// if (desDelta.x > margin) {
			// 	sqareDelta.x = 1;
			// } else if (desDelta.x < -margin) {
			// 	sqareDelta.x = -1;
			// }
			// if (desDelta.y > margin) {
			// 	sqareDelta.y = 1;
			// } else if (desDelta.y < -margin) {
			// 	sqareDelta.y = -1;
			// }
			// float speed = 4f;
			// if (CanMoveByDelta(entity, sqareDelta)) {
			// 	desDelta -= (speed * Time.deltaTime) * sqareDelta.ToVector2();
			// 	visOffset += (speed * Time.deltaTime) * sqareDelta.ToVector2();

			// 	if (desDelta.sqrMagnitude < margin) {
			// 		entity.Location += sqareDelta;
			// 		desDelta = Vector2.zero;
			// 		visOffset = Vector2.zero;
			// 	}
			// } else {
			// 	desDelta = Vector2.zero;
			// }

			// entity.DesiredDelta = desDelta;
			// entity.VisualOffset = visOffset;

			// if (desDelta.x > 0) {
				// if (CanMoveByDelta(entity, new IntVector2D(1,0))) {
					// desDelta.x -= Time.deltaTime
				// }
			// }
		}
	}
}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TileContents {
	public WorldEntity entity;
	public TileContents (WorldEntity we) {
		entity = we;
	}
}

public class WorldManager : MonoBehaviour {

	int _xDim = 20; // Right
	int _zDim = 20; // Left
	int _yDim = 20; // Up
	float _tileSize = 1f;

	public float TileSize {
		get { return _tileSize; }
	}

	Dictionary<int, Dictionary<int, Dictionary<int, TileContents>>> _world =
		new Dictionary<int, Dictionary<int, Dictionary<int,TileContents>>>();
	bool[,] _xyWorldPassability;
	bool[,] _zyWorldPassability;

	float timeStepDuration = 0.2f;

	public static WorldManager g;
	void Awake () {
		if (g == null) {
			g = this;
		} else {
			Destroy(this);
		}
	}

	// Use this for initialization
	void Start () {
		InitLevel();
	}

	bool _initialized = false;
	void InitLevel () {
		// TODO(JULIAN): Load dimensions
		int _xDim = 20; // Right
		int _zDim = 20; // Left
		int _yDim = 20; // Up

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
		_xyWorldPassability = new bool[_xDim,_yDim];
		_zyWorldPassability = new bool[_zDim,_yDim];
		_initialized = true;
		UpdatePassability();
	}

	public void UpdatePassability () {

		if (_xyWorldPassability == null) _xyWorldPassability = new bool[_xDim,_yDim];
		if (_zyWorldPassability == null) _zyWorldPassability = new bool[_zDim,_yDim];

		// Fill _zyWorldPassability map
		for (int y = 0; y < _yDim; y++) {
			for (int z = 0; z < _zDim; z++) {
				_zyWorldPassability[z,y] = true;
				for (int x = 0; x < _xDim; x++) {
					if (!PassableAt3D(x,y,z)) {
						_zyWorldPassability[z,y] = false;
						break;
					}
				}
			}
		}
		// Fill _xyWorldPassability map
		for (int y = 0; y < _yDim; y++) {
			for (int x = 0; x < _xDim; x++) {
				_xyWorldPassability[x,y] = true;
				for (int z = 0; z < _zDim; z++) {
					if (!PassableAt3D(x,y,z)) {
						_xyWorldPassability[x,y] = false;
						break;
					}
				}
			}
		}
	}

	bool PassableAtXY(int x, int y) {
		return _xyWorldPassability[x,y];
	}

	bool PassableAtZY(int z, int y) {
		return _zyWorldPassability[z,y];
	}

	bool PassableAt3D(int x, int y, int z) {
		if (x < 0 || y < 0 || z < 0 ||
			x >= _xDim || y >= _yDim || z >= _zDim) {
			return false;
		}
		TileContents c = ContentsAt(x,y,z);
		if (c != null && c.entity != null) {
			return c.entity.Passable;
		}
		return true;
	}

	public TileContents ContentsAt(IntVector v) {
		return ContentsAt(v.x, v.y, v.z);
	}

	public TileContents ContentsAt(int x, int y, int z) {
		if (_world.ContainsKey(y)) {
			if (_world[y].ContainsKey(x)) {
				if (_world[y][x].ContainsKey(z)) {
					return _world[y][x][z];
				}
			}
		}
		return null;
	}

	public void SetContentsAt(IntVector v, WorldEntity t) {
		SetContentsAt(v.x, v.y, v.z, t);
	}

	public void SetContentsAt(int x, int y, int z, WorldEntity t) {
		if (!_world.ContainsKey(y)) {
			_world[y] = new Dictionary<int, Dictionary<int, TileContents>>();
		}
		if (!_world[y].ContainsKey(x)) {
			_world[y][x] = new Dictionary<int, TileContents>();
		}
		_world[y][x][z] = new TileContents(t);
	}

	void OnDrawGizmos () {
		if (!_initialized) return;
		for (int y = 0; y < _yDim; y++) {
			for (int x = 0; x < _xDim; x++) {
				for (int z = 0; z < _zDim; z++) {
					if (ContentsAt(x,y,z) != null && ContentsAt(x,y,z).entity != null) {
						Gizmos.DrawSphere(new Vector3(x,y,z) * _tileSize, _tileSize/2);
					}
				}
			}
		}
		for (int y = 0; y < _yDim; y++) {
			for (int x = 0; x < _xDim; x++) {
				if (!_xyWorldPassability[x,y]) {
					Gizmos.DrawLine(new Vector3(_tileSize * (x - 0.5f), _tileSize * (y - 0.5f), 0f),
									new Vector3(_tileSize * (x + 0.5f), _tileSize * (y + 0.5f), 0f));
					Gizmos.DrawLine(new Vector3(_tileSize * (x - 0.5f), _tileSize * (y + 0.5f), 0f),
									new Vector3(_tileSize * (x + 0.5f), _tileSize * (y - 0.5f), 0f));
				}
			}
		}
		for (int y = 0; y < _yDim; y++) {
			for (int z = 0; z < _zDim; z++) {
				if (!_zyWorldPassability[z,y]) {
					Gizmos.DrawLine(new Vector3(0f, _tileSize * (y - 0.5f), _tileSize * (z - 0.5f)),
									new Vector3(0f, _tileSize * (y + 0.5f), _tileSize * (z + 0.5f)));
					Gizmos.DrawLine(new Vector3(0f, _tileSize * (y + 0.5f), _tileSize * (z - 0.5f)),
									new Vector3(0f, _tileSize * (y - 0.5f), _tileSize * (z + 0.5f)));
				}
			}
		}

	}
}
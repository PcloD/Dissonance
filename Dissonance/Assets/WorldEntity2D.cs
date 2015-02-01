using UnityEngine;
using System.Collections;

public class WorldEntity2D : MonoBehaviour {

	[SerializeField]
	int _rightAxis = 0;
	[SerializeField]
	int _upAxis = 1;

	[SerializeField]
	IntVector2D _loc = new IntVector2D();
	[SerializeField]
	Transform _visuals;

	void Update () {
		Vector3 v = Vector3.zero;
		v[_rightAxis] = 1 * _loc[0];
		v[_upAxis] = 1 * _loc[1];
		_visuals.position = v * WorldManager.g.TileSize;
	}
}

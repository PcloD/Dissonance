using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum PlaneOrientation {
	XY, ZY
}

public class WorldEntity2D : MonoBehaviour {

	[SerializeField]
	PlaneOrientation _planeOrientation;
	public PlaneOrientation Orientation {
		get { return _planeOrientation; }
	}


	[SerializeField]
	List<IntVector2D> _identityLocations = new List<IntVector2D>();
	[SerializeField]
	IntVector2D _loc = new IntVector2D();
	public IntVector2D Location {
		get { return _loc; }
		set { _loc = value; }
	}

	[SerializeField]
	Transform _visuals;

	private Vector2 _visualOffset;
	private Vector2 _desiredDelta;
	public Vector2 DesiredDelta {
		get { return _desiredDelta; }
		set { _desiredDelta = value; }
	}
	public Vector2 VisualOffset {
		get { return _visualOffset; }
		set { _visualOffset = value; }
	}

	void Start () {
		WorldManager.g.RegisterEntity(this, _planeOrientation);
	}

	void Update () {
		Vector3 v = Vector3.zero;
		switch (_planeOrientation) {
			case PlaneOrientation.XY:
				v = _loc.ToVector2() + _visualOffset;
				break;
			case PlaneOrientation.ZY:
				v.z = _loc.x + _visualOffset.x;
				v.y = _loc.y + _visualOffset.y;
				break;
			default:
				break;
		}
		_visuals.position = v * WorldManager.g.TileSize;
	}

	public List<IntVector2D> AbsoluteLocations (IntVector2D location) {
		List<IntVector2D> absoluteLocations = new List<IntVector2D>();
		for (int i = 0; i < _identityLocations.Count; i++) {
			absoluteLocations.Add(_identityLocations[i] + location);
		}
		return absoluteLocations;
	}
}

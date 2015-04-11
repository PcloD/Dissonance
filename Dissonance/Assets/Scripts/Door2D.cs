using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Door2D : MonoBehaviour
{

	private WorldEntity2D _worldEntity;

	private StateInformation _currStateInfo;
	public StateInformation StateInfo {
		get { return _currStateInfo; }
	}

	[SerializeField]
	Transform
		_visuals;

	public PlaneOrientation Orientation {
		get { return _worldEntity.Orientation; }
	}

	public IntVector2D Location {
		get { return _worldEntity.Location; }
		set { _worldEntity.Location = value; }
	}

	public List<IntVector2D> AbsoluteLocations (IntVector2D location)
	{
		return _worldEntity.AbsoluteLocations (location);
	}

	public Vector2 VisualPos {
		get {
			switch (Orientation) {
			case PlaneOrientation.XY:
				return (Vector2)_visuals.position;
			case PlaneOrientation.ZY:
				return new Vector2 (_visuals.position.z, _visuals.position.y);
			default:
				return Vector2.zero;
			}
		}
	}

	void OnEnable ()
	{
		_worldEntity.Simulators += Simulate;
	}

	void OnDisable ()
	{
		_worldEntity.Simulators += Simulate;
	}

	void Awake ()
	{
		_worldEntity = GetComponent<WorldEntity2D> ();
	}

	void Start ()
	{
		_currStateInfo.facingDirection = FacingDir.Left;
		_currStateInfo.lastLoc = Location;
	}

	void Simulate ()
	{

	}

	void Update ()
	{
		Vector3 v = Vector3.zero;
		Vector2 visualOffset = (Location.ToVector2 () - _currStateInfo.lastLoc.ToVector2 ()) * (_currStateInfo.fractionComplete);
		
		// TODO(Julian): Add Animation here!
		float epsilon = 0.0001f;
		switch (Orientation) {
		case PlaneOrientation.XY:
			v = _currStateInfo.lastLoc.ToVector2 () + visualOffset;
			v.z = epsilon;
			break;
		case PlaneOrientation.ZY:
			v.z = _currStateInfo.lastLoc.x + visualOffset.x;
			v.y = _currStateInfo.lastLoc.y + visualOffset.y;
			v.x = epsilon;
			break;
		default:
			break;
		}
		_visuals.position = v * WorldManager.g.TileSize;
	}
}

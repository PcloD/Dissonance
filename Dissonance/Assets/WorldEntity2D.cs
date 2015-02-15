using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum PlaneOrientation {
	XY, ZY
}

public enum FacingDir {
	Left,
	Right
}

public enum State {
	Idle,
	Walking,
	Jumping,
	Falling
}

[System.Serializable]
public struct StateInformation {
	public State lastState;
	public State state;

	public float fractionComplete; // Range from 0-1, inclusive
	public FacingDir facingDirection;
	public IntVector2D lastLoc;
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

	[System.Serializable]
	public class Speeds {
		public float movementSpeed = 4f;
		public float fallSpeed = 2f;
	}
	[SerializeField]
	private Speeds _speeds = new Speeds();

	private StateInformation _currStateInfo;
	public StateInformation StateInfo {
		get { return _currStateInfo; }
	}


	Vector2 _desiredInput;
	public Vector2 DesiredInput {
		get { return _desiredInput; }
		set { _desiredInput = value; }
	}

	public void WalkInDirBy (FacingDir dir, IntVector2D deltaLoc) {
		_currStateInfo.lastLoc = _loc;
		_loc += deltaLoc;
		_currStateInfo.state = State.Walking;
		_currStateInfo.facingDirection = dir;
		_currStateInfo.fractionComplete = 0f;
	}

	public void JumpInDirBy (FacingDir dir, IntVector2D deltaLoc) {
		_currStateInfo.lastLoc = _loc;
		_loc += deltaLoc;
		_currStateInfo.state = State.Jumping;
		_currStateInfo.facingDirection = dir;
		_currStateInfo.fractionComplete = 0f;
	}

	public void Fall () {
		_currStateInfo.lastLoc = _loc;
		_loc += new IntVector2D(0,-1);
		_currStateInfo.fractionComplete = 0f;
		_currStateInfo.state = State.Falling;
	}

	public void Land () {
		_currStateInfo.lastLoc = _loc;
		_currStateInfo.fractionComplete = 0f;
		_currStateInfo.state = State.Idle;
	}

	void Start () {
		WorldManager.g.RegisterEntity(this);
		_currStateInfo.facingDirection = FacingDir.Left;
		_currStateInfo.lastLoc = _loc;
	}

	void Update () {
		Vector3 v = Vector3.zero;
		Vector2 visualOffset = (_loc.ToVector2() - _currStateInfo.lastLoc.ToVector2()) * (_currStateInfo.fractionComplete);
		// TODO(Julian): Add Animation here!
		switch (_planeOrientation) {
			case PlaneOrientation.XY:
				v = _currStateInfo.lastLoc.ToVector2() + visualOffset;// - Vector2.one/2f;
				break;
			case PlaneOrientation.ZY:
				v.z = _currStateInfo.lastLoc.x + visualOffset.x;
				v.y = _currStateInfo.lastLoc.y + visualOffset.y;
				break;
			default:
				break;
		}
		_visuals.position = v * WorldManager.g.TileSize;

		float speed;
		switch (_currStateInfo.state) {
			case State.Falling:
				speed = _speeds.fallSpeed;
				break;
			default:
				speed = _speeds.movementSpeed;
				break;
		}

		_currStateInfo.fractionComplete += speed * Time.deltaTime;
		if (_currStateInfo.fractionComplete >= 1f) {
			_currStateInfo.lastState = _currStateInfo.state;
			// if (_currStateInfo.state != State.Falling) {
				_currStateInfo.state = State.Idle;
			// }
			_currStateInfo.lastLoc = _loc;
			_currStateInfo.fractionComplete = 0f;
		}

		// _currStateInfo.fractionComplete = Mathf.Max(_currStateInfo.fractionComplete, 0f);
		// _currStateInfo.fractionComplete = Mathf.Min(_currStateInfo.fractionComplete, 1f);
	}

	public List<IntVector2D> AbsoluteLocations (IntVector2D location) {
		List<IntVector2D> absoluteLocations = new List<IntVector2D>();
		for (int i = 0; i < _identityLocations.Count; i++) {
			absoluteLocations.Add(_identityLocations[i] + location);
		}
		return absoluteLocations;
	}
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

public class Char2D : MonoBehaviour {

	private WorldEntity2D _worldEntity;

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

	[SerializeField]
	Transform _visuals;

	public PlaneOrientation Orientation {
		get { return _worldEntity.Orientation; }
	}

	public IntVector2D Location {
		get { return _worldEntity.Location; }
		set { _worldEntity.Location = value; }
	}

	public List<IntVector2D> AbsoluteLocations (IntVector2D location) {
		return _worldEntity.AbsoluteLocations(location);
	}

	public Vector2 VisualPos {
		get {
			switch (Orientation) {
				case PlaneOrientation.XY:
					return (Vector2)_visuals.position;
				case PlaneOrientation.ZY:
					return new Vector2(_visuals.position.z, _visuals.position.y);
				default:
					return Vector2.zero;
			}
		}
	}

	void OnEnable () {
		_worldEntity.Simulators += Simulate;
	}

	void OnDisable () {
		_worldEntity.Simulators += Simulate;
	}

	void Awake () {
		_worldEntity = GetComponent<WorldEntity2D>();
	}

	void Start () {
		_currStateInfo.facingDirection = FacingDir.Left;
		_currStateInfo.lastLoc = Location;
	}

	void Simulate () {
		StateInformation eState = _currStateInfo;

		if (eState.state == State.Idle) {
			IntVector2D delta = new IntVector2D(0,-1);
			if (WorldManager.g.CanMoveByDelta(_worldEntity, delta)) {
				if (eState.state != State.Falling) {
					Fall();
				}
			} else if (eState.state == State.Falling) {
				Land();
			} else if (_desiredInput.x > 0f) {
				delta = new IntVector2D(1,0);
				IntVector2D jumpDelta = new IntVector2D(1,1);
				if (WorldManager.g.CanMoveByDelta(_worldEntity, delta)) {
					WalkInDirBy(FacingDir.Right, delta);
				} else if (WorldManager.g.CanJumpByDelta(_worldEntity, jumpDelta)) {
					JumpInDirBy(FacingDir.Right, jumpDelta);
				}
			} else if (_desiredInput.x < 0f) {
				delta = new IntVector2D(-1,0);
				IntVector2D jumpDelta = new IntVector2D(-1,1);
				if (WorldManager.g.CanMoveByDelta(_worldEntity, delta)) {
					WalkInDirBy(FacingDir.Left, delta);
				} else if (WorldManager.g.CanJumpByDelta(_worldEntity, jumpDelta)) {
					JumpInDirBy(FacingDir.Left, jumpDelta);
				}
			}
		}
	}


	private void WalkInDirBy (FacingDir dir, IntVector2D deltaLoc) {
		_currStateInfo.lastLoc = Location;
		Location += deltaLoc;
		_currStateInfo.state = State.Walking;
		_currStateInfo.facingDirection = dir;
		_currStateInfo.fractionComplete = 0f;
	}

	private void JumpInDirBy (FacingDir dir, IntVector2D deltaLoc) {
		_currStateInfo.lastLoc = Location;
		Location += deltaLoc;
		_currStateInfo.state = State.Jumping;
		_currStateInfo.facingDirection = dir;
		_currStateInfo.fractionComplete = 0f;
	}

	private void Fall () {
		_currStateInfo.lastLoc = Location;
		Location += new IntVector2D(0,-1);
		_currStateInfo.fractionComplete = 0f;
		_currStateInfo.state = State.Falling;
	}

	private void Land () {
		_currStateInfo.lastLoc = Location;
		_currStateInfo.fractionComplete = 0f;
		_currStateInfo.state = State.Idle;
	}

	void Update () {
		Vector3 v = Vector3.zero;
		Vector2 visualOffset = (Location.ToVector2() - _currStateInfo.lastLoc.ToVector2()) * (_currStateInfo.fractionComplete);

		// TODO(Julian): Add Animation here!
		float epsilon = 0.0001f;
		switch (Orientation) {
			case PlaneOrientation.XY:
				v = _currStateInfo.lastLoc.ToVector2() + visualOffset;
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
			_currStateInfo.lastLoc = Location;
			_currStateInfo.fractionComplete = 0f;
		}

		// _currStateInfo.fractionComplete = Mathf.Max(_currStateInfo.fractionComplete, 0f);
		// _currStateInfo.fractionComplete = Mathf.Min(_currStateInfo.fractionComplete, 1f);
	}
}

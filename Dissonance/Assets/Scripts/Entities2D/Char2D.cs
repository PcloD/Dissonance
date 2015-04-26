using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Char2D : MonoBehaviour {

    public enum Char2DState {
        Idle,
        Walking,
        Jumping,
        Falling
    }

    [System.Serializable]
    public struct StateInformation {
        [HideInInspector]
        public Char2DState lastState;
        public Char2DState state;

        [HideInInspector]
        public float fractionComplete;
        // Range from 0-1, inclusive
        public FacingDir facingDirection;
        [HideInInspector]
        public IntVector2D lastLoc;
        [HideInInspector]
        public bool inactive;
    }

    public void Activate () {
    	StateInformation i = _currStateInfo;
    	i.inactive = false;
    	_currStateInfo = i;
    }

    public void Deactivate () {
    	StateInformation i = _currStateInfo;
    	i.inactive = true;
    	_currStateInfo = i;
    }

    private WorldEntity2D _worldEntity;
    public WorldEntity2D Entity {
    	get { return _worldEntity; }
    }

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

    private int[] _animationStateIds;
    private Char2DState[] _allStates;

    // Vector2 _desiredInput;
    // public Vector2 DesiredInput {
    // 	get { return _desiredInput; }
    // 	set { _desiredInput = value; }
    // }

    List<IntVector2D> _desiredPath = new List<IntVector2D>();
    public List<IntVector2D> DesiredPath {
        get { return _desiredPath; }
        set {
        	bool canSet = (_desiredPath.Count == 0);
        	for (int i = 0; i < _desiredPath.Count && i < value.Count; i++) {
        		if (_desiredPath[i] != value[i]) {
        			canSet = true;
        			break;
        		}
        	}
        	if (canSet) {_desiredPath = value;}
        }
    }

    [SerializeField]
    Transform _visuals;
    [SerializeField]
    Animator _animator;
    Transform _animatorRotator;

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
        _worldEntity.Simulators -= Simulate;
    }

    void Awake () {
        _allStates = (Char2DState[])Enum.GetValues(typeof(Char2DState));
        _animationStateIds = new int[_allStates.Length];
        for (int i = 0; i < _allStates.Length; i++) {
            _animationStateIds[i] = Animator.StringToHash(_allStates[i].ToString());
        }
        _animatorRotator = _animator.transform;
        _worldEntity = GetComponent<WorldEntity2D>();
    }

    void Start () {
        _currStateInfo.facingDirection = FacingDir.Left;
        _currStateInfo.lastLoc = Location;
    }

    IntVector2D currPathSeg = new IntVector2D();
    void Simulate () {
        StateInformation eState = _currStateInfo;

        if (eState.state == Char2DState.Idle) {
            if (_desiredPath.Count > 0) {
                currPathSeg = _desiredPath[_desiredPath.Count - 1];
                _desiredPath.RemoveAt(_desiredPath.Count - 1);

                if (currPathSeg.y - Location.y < 0) {
                    Fall();
                }

                if (currPathSeg.x - Location.x > 0) {
                    MoveInDirTo(FacingDir.Right, currPathSeg);
                }
                else
                if (currPathSeg.x - Location.x < 0) {
                    MoveInDirTo(FacingDir.Left, currPathSeg);
                }
            }
            else {
                IntVector2D delta = new IntVector2D(0, -1);
                if (WorldManager.g.CanMoveByDelta(_worldEntity, delta)) {
                    if (eState.state != Char2DState.Falling) {
                        Fall();
                    }
                }
            }
        }

        // if (eState.state == State.Idle) {
        // 	IntVector2D delta = new IntVector2D(0,-1);
        // 	if (WorldManager.g.CanMoveByDelta(_worldEntity, delta)) {
        // 		if (eState.state != State.Falling) {
        // 			Fall();
        // 		}
        // 	} else if (eState.state == State.Falling) {
        // 		Land();
        // 	} else if (_desiredInput.x > 0f) {
        // 		delta = new IntVector2D(1,0);
        // 		IntVector2D jumpDelta = new IntVector2D(1,1);
        // 		if (WorldManager.g.CanMoveByDelta(_worldEntity, delta)) {
        // 			WalkInDirBy(FacingDir.Right, delta);
        // 		} else if (WorldManager.g.CanJumpByDelta(_worldEntity, jumpDelta)) {
        // 			JumpInDirBy(FacingDir.Right, jumpDelta);
        // 		}
        // 	} else if (_desiredInput.x < 0f) {
        // 		delta = new IntVector2D(-1,0);
        // 		IntVector2D jumpDelta = new IntVector2D(-1,1);
        // 		if (WorldManager.g.CanMoveByDelta(_worldEntity, delta)) {
        // 			WalkInDirBy(FacingDir.Left, delta);
        // 		} else if (WorldManager.g.CanJumpByDelta(_worldEntity, jumpDelta)) {
        // 			JumpInDirBy(FacingDir.Left, jumpDelta);
        // 		}
        // 	}
        // }
    }


    private void MoveInDirTo (FacingDir dir, IntVector2D dest) {
        _currStateInfo.lastLoc = Location;
        Location = dest;
        _currStateInfo.state = Char2DState.Walking;
        _currStateInfo.facingDirection = dir;
        _currStateInfo.fractionComplete = 0f;
    }

    private void WalkInDirBy (FacingDir dir, IntVector2D deltaLoc) {
        _currStateInfo.lastLoc = Location;
        Location += deltaLoc;
        _currStateInfo.state = Char2DState.Walking;
        _currStateInfo.facingDirection = dir;
        _currStateInfo.fractionComplete = 0f;
    }

    private void JumpInDirBy (FacingDir dir, IntVector2D deltaLoc) {
        _currStateInfo.lastLoc = Location;
        Location += deltaLoc;
        _currStateInfo.state = Char2DState.Jumping;
        _currStateInfo.facingDirection = dir;
        _currStateInfo.fractionComplete = 0f;
    }

    private void Fall () {
        _currStateInfo.lastLoc = Location;
        Location += new IntVector2D(0, -1);
        _currStateInfo.fractionComplete = 0f;
        _currStateInfo.state = Char2DState.Falling;
    }

    private void Land () {
        _currStateInfo.lastLoc = Location;
        _currStateInfo.fractionComplete = 0f;
        _currStateInfo.state = Char2DState.Idle;
    }

    void Update () {
        Vector3 v = Vector3.zero;
        Vector2 visualOffset = (Location.ToVector2() - _currStateInfo.lastLoc.ToVector2()) * (_currStateInfo.fractionComplete);

        // TODO(Julian): Add Animation here!
        float separationFromWall = 10f;
        switch (Orientation) {
            case PlaneOrientation.XY:
                v = _currStateInfo.lastLoc.ToVector2() + visualOffset;
                v.z = separationFromWall;
                if (_currStateInfo.inactive) {
                	_animatorRotator.rotation = Quaternion.AngleAxis(0f, Vector3.up);
                } else if (_currStateInfo.facingDirection == FacingDir.Left) {
                    _animatorRotator.rotation = Quaternion.AngleAxis(-90f, Vector3.up);
                } else {
                    _animatorRotator.rotation = Quaternion.AngleAxis(90f, Vector3.up);
                }
                break;
            case PlaneOrientation.ZY:
                v.z = _currStateInfo.lastLoc.x + visualOffset.x;
                v.y = _currStateInfo.lastLoc.y + visualOffset.y;
                v.x = separationFromWall;
                if (_currStateInfo.inactive) {
                	_animatorRotator.rotation = Quaternion.AngleAxis(90f, Vector3.up);
                } else if (_currStateInfo.facingDirection == FacingDir.Left) {
                    _animatorRotator.rotation = Quaternion.AngleAxis(180f, Vector3.up);
                } else {
                    _animatorRotator.rotation = Quaternion.AngleAxis(0f, Vector3.up);
                }
                break;
            default:
                break;
        }
        _visuals.position = v * WorldManager.g.TileSize;

        float speed;
        switch (_currStateInfo.state) {
            case Char2DState.Falling:
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
            _currStateInfo.state = Char2DState.Idle;
            // }
            _currStateInfo.lastLoc = Location;
            _currStateInfo.fractionComplete = 0f;
        }

        for (int i = 0; i < _allStates.Length; i++) {
            _animator.SetBool(_animationStateIds[i], (_currStateInfo.state == _allStates[i]));
        }

        // _currStateInfo.fractionComplete = Mathf.Max(_currStateInfo.fractionComplete, 0f);
        // _currStateInfo.fractionComplete = Mathf.Min(_currStateInfo.fractionComplete, 1f);
    }
}

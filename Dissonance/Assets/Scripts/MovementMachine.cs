using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum Axis {
	X = 0,
	Y = 1,
	Z = 2
}

[System.Serializable]
public struct MovementMachineStateInformation {
	[HideInInspector]
	public IntVector lastLocation;
	[HideInInspector]
	public float fractionComplete; // Range from 0-1, inclusive
	[HideInInspector]
	public int movementDeltaX;
	[HideInInspector]
	public int movementDeltaY;
	[HideInInspector]
	public int movementDeltaZ;
	//public Axis movementAxis; // 0, 1, 2
	public bool axisX;
	public bool axisY;
	public bool axisZ;
}

public class MovementMachine : MonoBehaviour, IControllableMachine {

	private WorldEntity _worldEntity;
	[SerializeField]
	private Rotatable _rotationController;

	[SerializeField]
	private MovementMachineStateInformation _currStateInfo;
	public MovementMachineStateInformation StateInfo {
		get { return _currStateInfo; }
	}
	public int singleMovementDistX=1;
	public int singleMovementDistY=1;
	public int singleMovementDistZ=1;
	public bool IsCached {
		get {
			return _worldEntity != null;
		}
	}

	public void Cache () {
		_worldEntity = GetComponent<WorldEntity>();
	}

	void Awake () {
		Cache();
	}

	void Start () {
		_currStateInfo.lastLocation = _worldEntity.Location;
		_currStateInfo.fractionComplete = 0f;
		_currStateInfo.movementDeltaX = 0;
		_currStateInfo.movementDeltaY = 0;
		_currStateInfo.movementDeltaZ = 0;
		// movementAxis should be user-defined
	}

	void OnEnable () {
		_worldEntity.Simulators += Simulate;
		if (_rotationController != null) {
			_rotationController.RotationHook += InitMoving;
		}
	}

	void OnDisable () {
		_worldEntity.Simulators += Simulate;
		if (_rotationController != null) {
			_rotationController.RotationHook -= InitMoving;
		}
	}

	private void InitMoving () {
		switch (_rotationController.StateInfo.state) {
			case RotationState.RotatingCounterClockwise:
				_currStateInfo.movementDeltaX = singleMovementDistX;
				_currStateInfo.movementDeltaY = singleMovementDistY;
				_currStateInfo.movementDeltaZ = singleMovementDistZ;
				break;
			case RotationState.RotatingClockwise:
				_currStateInfo.movementDeltaX = -singleMovementDistX;
				_currStateInfo.movementDeltaY = -singleMovementDistY;
				_currStateInfo.movementDeltaZ = -singleMovementDistZ;
				break;
			default:
				break;
		}
	}

	private void Update () {
		if (_rotationController != null) {
			_currStateInfo.fractionComplete = _rotationController.StateInfo.fractionComplete;
			if (_rotationController.StateInfo.state == RotationState.Idle) {
				_currStateInfo.lastLocation = _worldEntity.Location;
			}
		}
	}

	private void Simulate () {
		if (_currStateInfo.movementDeltaX!=0 || _currStateInfo.movementDeltaY!=0 || _currStateInfo.movementDeltaZ!=0) {
			_currStateInfo.lastLocation = _worldEntity.Location;
			IntVector delta = new IntVector();
		//	delta[(int)_currStateInfo.movementAxis] = _currStateInfo.movementDelta;
			if(_currStateInfo.axisX){
				delta[0]= _currStateInfo.movementDeltaX;
			}
			if(_currStateInfo.axisY){
				delta[1]= _currStateInfo.movementDeltaY;
			}
			if(_currStateInfo.axisZ){
				delta[2]= _currStateInfo.movementDeltaZ;
			}
			_worldEntity.Location = _worldEntity.Location + delta;
			// TODO(Julian): Prevent movement if not possible!
			_currStateInfo.movementDeltaX = 0;
			_currStateInfo.movementDeltaY = 0;
			_currStateInfo.movementDeltaZ = 0;
		}
	}

	void OnDrawGizmos () {
		// NOTE(Julian): For debug visualization in Unity editor
		if (!Application.isPlaying && WorldManager.g != null) {
			if (_rotationController != null) {
				if (!IsCached) { Cache(); }
				if (!_rotationController.IsCached) { _rotationController.Cache(); };
				Gizmos.color = Color.green;
				Gizmos.DrawLine(_rotationController.Location.ToVector3() * WorldManager.g.TileSize,
							   	_worldEntity.Location.ToVector3() * WorldManager.g.TileSize);
			}
		}
	}
}
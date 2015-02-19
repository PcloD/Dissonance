using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public struct MovementMachineStateInformation {
	public IntVector lastLocation;
	public float fractionComplete; // Range from 0-1, inclusive
	public int movementDelta;
	public int movementAxis; // 0, 1, 2
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

	void Awake () {
		_worldEntity = GetComponent<WorldEntity>();
	}

	void Start () {
		_currStateInfo.lastLocation = _worldEntity.Location;
		_currStateInfo.fractionComplete = 0f;
		_currStateInfo.movementDelta = 0;
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
				_currStateInfo.movementDelta = 1;
				break;
			case RotationState.RotatingClockwise:
				_currStateInfo.movementDelta = -1;
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
		if (_currStateInfo.movementDelta != 0) {
			_currStateInfo.lastLocation = _worldEntity.Location;
			IntVector delta = new IntVector();
			delta[_currStateInfo.movementAxis] = _currStateInfo.movementDelta;
			_worldEntity.Location = _worldEntity.Location + delta;
			// TODO(Julian): Prevent movement if not possible!
			_currStateInfo.movementDelta = 0;
		}
	}
}
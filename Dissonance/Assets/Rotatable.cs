using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Rotatable : MonoBehaviour {

	private WorldEntity _worldEntity;

	private enum RotationState {
		Idle,
		StartRotatingCounterClockwise,
		RotatingCounterClockwise,
		StartRotatingClockwise,
		RotatingClockwise
	}

	[System.Serializable]
	private struct RotationStateInformation {
		public RotationState lastState;
		public RotationState state;

		public float fractionComplete; // Range from 0-1, inclusive
		public Vector3 rotationAnchor;
		public Quaternion lastRotation;
	}

	[SerializeField]
	RotationStateInformation _currStateInfo;

	void Awake () {
		_worldEntity = GetComponent<WorldEntity>();
	}

	void OnEnable () {
		_worldEntity.Simulators += Simulate;
	}

	void OnDisable () {
		_worldEntity.Simulators += Simulate;
	}

	public void RotateCounterClockwise (Vector3 worldAnchor) {
		_currStateInfo.state = RotationState.StartRotatingCounterClockwise;
		_currStateInfo.rotationAnchor = worldAnchor;
	}

	public void RotateClockwise (Vector3 worldAnchor) {
		_currStateInfo.state = RotationState.StartRotatingClockwise;
		_currStateInfo.rotationAnchor = worldAnchor;
	}

	private void Simulate () {
		float speed = 1f;
		if (_currStateInfo.state == RotationState.StartRotatingCounterClockwise) {
			_currStateInfo.state = RotationState.RotatingCounterClockwise;
			RotateAroundY(_currStateInfo.rotationAnchor, -1);
			_currStateInfo.fractionComplete = 0f;
		} else if (_currStateInfo.state == RotationState.StartRotatingClockwise) {
			_currStateInfo.state = RotationState.RotatingClockwise;
			RotateAroundY(_currStateInfo.rotationAnchor, 1);
			_currStateInfo.fractionComplete = 0f;
		}

		if (_currStateInfo.state != RotationState.Idle) {
			_currStateInfo.fractionComplete += speed * Time.deltaTime;
			if (_currStateInfo.fractionComplete >= 1f) {
				_currStateInfo.lastState = _currStateInfo.state;
				_currStateInfo.state = RotationState.Idle;
				_currStateInfo.lastRotation = _worldEntity.Rotation;
				_currStateInfo.fractionComplete = 0f;
			}
		}
	}

	private bool RotateAroundY(Vector3 worldAnchor, int dir) {
		return RotateAroundAxis(worldAnchor, dir, 1);
	}

	private bool RotateAroundAxis(Vector3 worldAnchor, int dir, int axis) {
		// Debug.DrawLine((worldAnchor+Vector3.one/2f)*WorldManager.g.TileSize, ((worldAnchor+Vector3.one/2f) + (new IntVector(0,1,0)).ToVector3()*5)*WorldManager.g.TileSize, Color.red, 1000f);
		Vector3 axisVector = Vector3.zero;
		axisVector[axis] = dir*90;
		Quaternion additionalRotation = Quaternion.Euler(axisVector);
		Vector3 offset = (worldAnchor - _worldEntity.Location.ToVector3());
		Vector3 rotatedOffset = additionalRotation * offset;
		IntVector newLoc = new IntVector(offset - rotatedOffset + _worldEntity.Location.ToVector3());
		Quaternion newRotation = additionalRotation * _worldEntity.Rotation;

		// Test new location
		// List<IntVector> newLocs = AbsoluteLocations(newLoc, newRotation);
		// for (int i = 0; i < newLocs.Count; i++) {
		// 	TileContents c = WorldManager.g.ContentsAt(newLocs[i].x, newLocs[i].y, newLocs[i].z);
		// 	if (c != null && c.entity != null && c.entity != this) return false;
		// }

		// TODO(Zi): Test if swept rotation would intersect!
		// XXX: Without this test, objects can teleport through other objects via rotation

		// Clear old location
		// List<IntVector> oldLocs = AbsoluteLocations(_loc, _worldEntity.Rotation);
		// for (int i = 0; i < oldLocs.Count; i++) {
		// 	WorldManager.g.SetContentsAt(oldLocs[i].x, oldLocs[i].y, oldLocs[i].z, null);
		// }

		// Fill new location
		// for (int i = 0; i < newLocs.Count; i++) {
		// 	WorldManager.g.SetContentsAt(newLocs[i].x, newLocs[i].y, newLocs[i].z, this);
		// }

		_worldEntity.Location = newLoc;
		_worldEntity.Rotation = newRotation;

		// WorldManager.g.UpdatePassability();
		return true;
	}

}
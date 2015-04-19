using UnityEngine;
using System.Collections;

public class ModelWorldEntityVis : MonoBehaviour {

	[SerializeField]
	GameObject _visualChild;
	private WorldEntity _worldEntity;
	private Rotatable _rotatable;
	private MovementMachine _movementMachine;
	// private MeshFilter _meshFilter;
	// private MeshRenderer _meshRenderer;
	private Transform _meshTransform;
	private Transform _rotator;

	void Awake () {
		_worldEntity = GetComponent<WorldEntity>();
		_rotatable = GetComponent<Rotatable>();
		_movementMachine = GetComponent<MovementMachine>();
	}

	void OnEnable () {
		if (_rotatable != null) {
			_rotatable.RotationHook += RotationPrep;
		}
	}

	void OnDisable () {
		if (_rotatable != null) {
			_rotatable.RotationHook -= RotationPrep;
		}
	}

	void Start () {
		var rotatorObject = new GameObject();
		rotatorObject.name = "Rotator";
		_rotator = rotatorObject.transform;
		_rotator.parent = transform;

		_meshTransform = _visualChild.transform;
		_meshTransform.parent = transform;
		_meshTransform.position = (_worldEntity.Location.ToVector3() + Vector3.one/2f) * WorldManager.g.TileSize;
	}

	void RotationPrep () {
		_meshTransform.parent = _rotator;
		_rotator.position = (_rotatable.StateInfo.rotationAnchor + Vector3.one/2f) * WorldManager.g.TileSize;
		_meshTransform.position = (_worldEntity.Location.ToVector3() + Vector3.one/2f) * WorldManager.g.TileSize;
	}

	void Update () {
		bool shouldRotate = ((_rotatable != null) && (_rotatable.StateInfo.state != RotationState.Idle));
		bool shouldMove = (_movementMachine != null);

		if (shouldRotate) {
			_rotator.rotation = Quaternion.Slerp(_rotatable.StateInfo.lastRotation, _worldEntity.Rotation, _rotatable.StateInfo.fractionComplete);
		} else if (shouldMove) {
			_meshTransform.parent = transform;
			_meshTransform.position = (Vector3.Lerp(_movementMachine.StateInfo.lastLocation.ToVector3(),
													_worldEntity.Location.ToVector3(),
													_movementMachine.StateInfo.fractionComplete) + Vector3.one/2f) * WorldManager.g.TileSize;
		} else {
			_meshTransform.parent = transform;
			_meshTransform.position = (_worldEntity.Location.ToVector3() + Vector3.one/2f) * WorldManager.g.TileSize;
			_meshTransform.rotation = _worldEntity.Rotation;
		}
	}
}
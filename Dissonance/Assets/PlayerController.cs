using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour {

	[SerializeField]
	WorldEntity2D[] _avatars2D;
	[SerializeField]
	WorldEntity _avatar3d;

	// public PlayerController g;

	private int _avatarControlIndex = 0;
	private Vector2 _input;

	// void Awake () {
	// 	if (g == null) {
	// 		g = this;
	// 	} else {
	// 		Destroy(this);
	// 	}
	// }

	void Update () {

		// TODO(Julian): Make this more robust
		_input.x = Input.GetAxisRaw("Horizontal");
		_input.y = Input.GetAxisRaw("Vertical");
		if (Input.GetKeyDown(KeyCode.Space)) {
			_avatarControlIndex = (_avatarControlIndex+1)%_avatars2D.Length;
		}
		// if (Input.GetKeyDown(KeyCode.Z)) {
		// 	_avatar3d.RotateLeft();
		// }
		// if (Input.GetKeyDown(KeyCode.X)) {
		// 	_avatar3d.RotateRight();
		// }

		WorldEntity2D currAvatar = _avatars2D[_avatarControlIndex];

		if (currAvatar.Orientation == PlaneOrientation.XY) {
			_input.x *= -1f;
		}

		for (int i = 0; i < _avatars2D.Length; i++) {
			_avatars2D[i].DesiredInput = Vector2.zero;
		}
		_avatars2D[_avatarControlIndex].DesiredInput = _input;
	}
}
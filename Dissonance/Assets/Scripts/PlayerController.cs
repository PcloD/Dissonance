using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour {

	[SerializeField]
	Char2D[] _avatars2D;
	[SerializeField]
	XYZChar _avatar3d;

	private int _avatarControlIndex = 0;
	private Vector2 _input;

	void Update () {

		// TODO(Julian): Make this more robust
		_input.x = Input.GetAxisRaw("Horizontal");
		_input.y = Input.GetAxisRaw("Vertical");
		if (Input.GetKeyDown(KeyCode.Space)) {
			_avatarControlIndex = (_avatarControlIndex+1)%_avatars2D.Length;
		}

		Rotatable r = _avatar3d.ObjectToRotate;
		if (r != null) {
			if (Input.GetKey(KeyCode.Z)) { r.RotateCounterClockwise(_avatar3d.Anchor); }
			else if (Input.GetKey(KeyCode.X)) { r.RotateClockwise(_avatar3d.Anchor); }
		}

		Char2D currAvatar = _avatars2D[_avatarControlIndex];

		if (currAvatar.Orientation == PlaneOrientation.XY) {
			_input.x *= -1f;
		}

		for (int i = 0; i < _avatars2D.Length; i++) {
			_avatars2D[i].DesiredInput = Vector2.zero;
		}
		_avatars2D[_avatarControlIndex].DesiredInput = _input;
	}
}
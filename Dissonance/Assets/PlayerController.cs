using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour {

	[SerializeField]
	WorldEntity2D[] _avatars;

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
			_avatarControlIndex = (_avatarControlIndex+1)%_avatars.Length;
		}

		WorldEntity2D currAvatar = _avatars[_avatarControlIndex];

		if (currAvatar.Orientation == PlaneOrientation.XY) {
			_input.x *= -1f;
		}

		if (_avatars[_avatarControlIndex].DesiredDelta.sqrMagnitude < 0.0001f) {
			// THIS MUST BE BETWEEN 0 and 1 for each axis!
			_avatars[_avatarControlIndex].DesiredDelta = _input;
		}
	}
}
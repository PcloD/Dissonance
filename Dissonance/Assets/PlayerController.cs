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

		_avatars[_avatarControlIndex].DesiredInput = _input;
	}
}
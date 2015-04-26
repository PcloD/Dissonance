using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour {

	[SerializeField]
	Char2D _avatarXY;
	[SerializeField]
	Char2D _avatarZY;
	[SerializeField]
	XYZChar _avatar3d;

	Char2D[] _avatars;
	int _selectedAvatarIndex;
	void Awake () {
		_avatars = new Char2D[2];
		_avatars[0] = _avatarXY;
		_avatars[1] = _avatarZY;
		_selectedAvatarIndex = 0;
		_avatars[_selectedAvatarIndex].Activate();
	}

	List<IntVector2D> PlanPathToLocForAvatar(IntVector2D goalPos, Char2D avatar, bool sendThereIfPossible) {
		List<IntVector2D> list = null;
		PlaneOrientation orientation = avatar.Orientation;
		if (WorldManager.g.IsInBounds2D(goalPos, orientation)) {
			IntVector2D lastLoc = avatar.Location;
			list = WorldManager.g.PlanPath(avatar, lastLoc, goalPos);
			if (list != null) {
				for (int i = list.Count - 1; i >= 0; i--) {
					IntVector2D nextLoc = list[i];
					Vector3 a = WorldManager.g.WorldPosFromIntVec(lastLoc, orientation);
					Vector3 b = WorldManager.g.WorldPosFromIntVec(nextLoc, orientation);
					Debug.DrawLine(a, b, Color.red);
					lastLoc = nextLoc;
				}
				if (sendThereIfPossible) {
					avatar.DesiredPath = list;
				}
			} else {
				// Debug.Log("NO VALID PATH FOR "+orientation.ToString()+"!");
			}
		}
		return list;
	}

	void Update () {
		bool mousePressed = Input.GetMouseButtonDown(0);
		PlanPathToLocForAvatar(WorldManager.g.MouseLocOnPlaneXY(), _avatarXY, mousePressed);
		PlanPathToLocForAvatar(WorldManager.g.MouseLocOnPlaneZY(), _avatarZY, mousePressed);

		var avatar = _avatars[_selectedAvatarIndex];
		if (Input.GetKeyDown(KeyCode.Space)) {
			avatar.Deactivate();
			_selectedAvatarIndex++;
			_selectedAvatarIndex %= _avatars.Length;
			avatar = _avatars[_selectedAvatarIndex];
			avatar.Activate();
		} else {
			bool moveKeyPressed = false;
			IntVector2D delta = new IntVector2D();
			if (Input.GetKey(KeyCode.RightArrow)) {
				delta += new IntVector2D(1,0);
				moveKeyPressed = true;
			}
			if (Input.GetKey(KeyCode.LeftArrow)) {
				delta += new IntVector2D(-1,0);
				moveKeyPressed = true;
			}
			if (avatar.Orientation == PlaneOrientation.XY) {delta *= -1;}
			if (!WorldManager.g.CanMoveByDelta(avatar.Entity, delta)) {
				delta += new IntVector2D(0,1);
			}
			if (moveKeyPressed) {
				List<IntVector2D> list = PlanPathToLocForAvatar(avatar.Location + delta, avatar, false);
				if (list != null && list.Count > 0 &&  list[0] != avatar.Location) {
					avatar.DesiredPath = list;
				}
			}
		}



		Rotatable r = _avatar3d.ObjectToRotate;
		if (r != null) {
			r.AnimateAtAnchor(_avatar3d.Anchor);
			if (Input.GetKey(KeyCode.Z)) { r.RotateCounterClockwise(_avatar3d.Anchor); }
			else if (Input.GetKey(KeyCode.X)) { r.RotateClockwise(_avatar3d.Anchor); }
		}
	}
}
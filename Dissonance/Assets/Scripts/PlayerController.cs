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

	void Update () {
		IntVector2D goalPos = WorldManager.g.MouseLocOnPlaneXY();
		if (WorldManager.g.IsInBounds2D(goalPos, PlaneOrientation.XY)) {
			IntVector2D lastLoc = _avatarXY.Location;
			List<IntVector2D> list = WorldManager.g.PlanPath(_avatarXY, lastLoc, goalPos);
			if (list != null) {
				for (int i = list.Count - 1; i >= 0; i--) {
					IntVector2D nextLoc = list[i];
					Vector3 a = WorldManager.g.WorldPosFromIntVecXY(lastLoc);
					Vector3 b = WorldManager.g.WorldPosFromIntVecXY(nextLoc);
					Debug.DrawLine(a, b, Color.red);
					lastLoc = nextLoc;
				}
				if (Input.GetMouseButtonDown(0)) {
					_avatarXY.DesiredPath = list;
				}
			} else {
				// Debug.Log("NO VALID XY PATH!");
			}
		}

		goalPos = WorldManager.g.MouseLocOnPlaneZY();
		if (WorldManager.g.IsInBounds2D(goalPos, PlaneOrientation.ZY)) {
			IntVector2D lastLoc = _avatarZY.Location;
			List<IntVector2D> list = WorldManager.g.PlanPath(_avatarZY, lastLoc, goalPos);
			if (list != null) {
				for (int i = list.Count - 1; i >= 0; i--) {
					IntVector2D nextLoc = list[i];
					Vector3 a = WorldManager.g.WorldPosFromIntVecZY(lastLoc);
					Vector3 b = WorldManager.g.WorldPosFromIntVecZY(nextLoc);
					Debug.DrawLine(a, b, Color.cyan);
					lastLoc = nextLoc;
				}
				if (Input.GetMouseButtonDown(0)) {
					_avatarZY.DesiredPath = list;
				}
			} else {
				// Debug.Log("NO VALID ZY PATH!");
			}
		}


		Rotatable r = _avatar3d.ObjectToRotate;
		if (r != null) {
			if (Input.GetKey(KeyCode.Z)) { r.RotateCounterClockwise(_avatar3d.Anchor); }
			else if (Input.GetKey(KeyCode.X)) { r.RotateClockwise(_avatar3d.Anchor); }
		}
	}
}
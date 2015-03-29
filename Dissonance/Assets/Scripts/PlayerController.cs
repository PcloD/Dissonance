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
		Char2D currAvatar = _avatars2D[_avatarControlIndex];
		if (currAvatar.Orientation == PlaneOrientation.XY) { _input.x *= -1f; }
		_input.y = Input.GetAxisRaw("Vertical");
		if (Input.GetKeyDown(KeyCode.Space)) {
			_avatarControlIndex = (_avatarControlIndex+1)%_avatars2D.Length;
		}

		Rotatable r = _avatar3d.ObjectToRotate;
		if (r != null) {
			if (Input.GetKey(KeyCode.Z)) { r.RotateCounterClockwise(_avatar3d.Anchor); }
			else if (Input.GetKey(KeyCode.X)) { r.RotateClockwise(_avatar3d.Anchor); }
		}

		for (int i = 0; i < _avatars2D.Length; i++) {
			_avatars2D[i].DesiredInput = Vector2.zero;
		}
		currAvatar.DesiredInput = _input;
	}

	Vector3 GetWorldPosFromIntVec (IntVector2D vec, PlaneOrientation orientation) {
		float tileSize = WorldManager.g.TileSize;
		Vector2 vector2 = vec.ToVector2();
		if (orientation == PlaneOrientation.XY) {
			vector2.x *= -1f;
			vector2.x -= 1f;
		}
		if (orientation == PlaneOrientation.XY) {
			return ProjectionMath.ThreeDimCoordsOnPlane(vector2 * tileSize + Vector2.one * tileSize/2f, WorldManager.g.PlaneXY);
		} else {
			return ProjectionMath.ThreeDimCoordsOnPlane(vector2 * tileSize + Vector2.one * tileSize/2f, WorldManager.g.PlaneZY);
		}
	}

	void OnDrawGizmos () {
		if (!Application.isPlaying) {return;}
		IntVector2D goalPos = WorldManager.g.MouseLocOnPlaneXY();
		if (WorldManager.g.IsInBounds2D(goalPos, PlaneOrientation.XY)) {
			Gizmos.color = Color.blue;
			Gizmos.DrawSphere(GetWorldPosFromIntVec(goalPos, PlaneOrientation.XY), 0.5f);
			Gizmos.color = Color.green;
			IntVector2D lastLoc = _avatars2D[0].Location;
			Gizmos.DrawSphere(GetWorldPosFromIntVec(lastLoc, PlaneOrientation.XY), 0.5f);
			// Debug.Log("MOUSE: "+goalPos + " -> " + lastLoc);
			List<IntVector2D> list = WorldManager.g.PlanPathXY(goalPos, lastLoc);
			if (list != null) {
				for (int i = 0; i < list.Count; i++) {
					IntVector2D nextLoc = list[i];
					Vector3 a = GetWorldPosFromIntVec(lastLoc, PlaneOrientation.XY);
					Vector3 b = GetWorldPosFromIntVec(nextLoc, PlaneOrientation.XY);
					Debug.DrawLine(a, b, Color.red);
					lastLoc = nextLoc;
				}
			} else {
				Debug.Log("NO VALID XY PATH!");
			}
		}

		goalPos = WorldManager.g.MouseLocOnPlaneZY();
		if (WorldManager.g.IsInBounds2D(goalPos, PlaneOrientation.ZY)) {
			Gizmos.color = Color.blue;
			Gizmos.DrawSphere(GetWorldPosFromIntVec(goalPos, PlaneOrientation.ZY), 0.5f);
			Gizmos.color = Color.green;
			IntVector2D lastLoc = _avatars2D[0].Location;
			Gizmos.DrawSphere(GetWorldPosFromIntVec(lastLoc, PlaneOrientation.ZY), 0.5f);
			// Debug.Log("MOUSE: "+goalPos + " -> " + lastLoc);
			List<IntVector2D> list = WorldManager.g.PlanPathZY(goalPos, lastLoc);
			if (list != null) {
				for (int i = 0; i < list.Count; i++) {
					IntVector2D nextLoc = list[i];
					Vector3 a = GetWorldPosFromIntVec(lastLoc, PlaneOrientation.ZY);
					Vector3 b = GetWorldPosFromIntVec(nextLoc, PlaneOrientation.ZY);
					Debug.DrawLine(a, b, Color.cyan);
					lastLoc = nextLoc;
				}
			} else {
				Debug.Log("NO VALID ZY PATH!");
			}
		}
	}
}
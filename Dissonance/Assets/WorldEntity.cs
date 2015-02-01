using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public struct IntVector {
	public int x;
	public int y;
	public int z;

	public override string ToString(){
		return x+", "+y+", "+z;
	}

	public override int GetHashCode(){
		return x.GetHashCode() ^ y.GetHashCode() ^ z.GetHashCode();
	}

	public override bool Equals( object ob ){
		if( ob is IntVector ) {
			IntVector other = (IntVector) ob;
			return (other.x == x && other.y == y && other.z == z);
		}
		else {
			return false;
		}
	}

	public static bool operator ==(IntVector a, IntVector b) {
	  return (a.x == b.x && a.y == b.y && a.z == b.z);
	}

	public static bool operator !=(IntVector x, IntVector y) {
	  return !(x == y);
	}

	public static IntVector operator +(IntVector a, IntVector b) {
	  return new IntVector (a.x+b.x, a.y+b.y, a.z+b.z);
	}

	public static IntVector operator -(IntVector a, IntVector b) {
	  return new IntVector (a.x-b.x, a.y-b.y, a.z-b.z);
	}

	public Vector3 ToVector3 () {
		return new Vector3(x,y,z);
	}

	public IntVector(int nx, int ny, int nz) {
		x = nx;
		y = ny;
		z = nz;
	}

	public IntVector(Vector3 v) {
		x = Mathf.RoundToInt(v.x);
		y = Mathf.RoundToInt(v.y);
		z = Mathf.RoundToInt(v.z);
	}
}

public class WorldEntity : MonoBehaviour {

	private bool _passable = false;
	public bool Passable {
		get { return _passable; }
	}

	IntVector _loc = new IntVector(5,5,5);

	Quaternion _rotation = Quaternion.identity;
	[SerializeField]
	List<IntVector> _identityLocations = new List<IntVector>();

	private bool RotateAroundAxis(IntVector worldAnchor, int dir, int axis) {
		Vector3 axisVector = Vector3.zero;
		axisVector[axis] = dir*90;
		Quaternion additionalRotation = Quaternion.Euler(axisVector);
		IntVector offset = (worldAnchor - _loc);
		IntVector rotatedOffset = new IntVector(additionalRotation * offset.ToVector3());
		IntVector newLoc = offset - rotatedOffset + _loc;
		Quaternion newRotation = additionalRotation * _rotation;

		// Test new location
		List<IntVector> newLocs = AbsoluteLocations(newLoc, newRotation);
		for (int i = 0; i < newLocs.Count; i++) {
			TileContents c = WorldManager.g.ContentsAt(newLocs[i].x, newLocs[i].y, newLocs[i].z);
			if (c != null && c.entity != null && c.entity != this) return false;
		}

		// TODO(Zi): Test if swept rotation would intersect!
		// XXX: Without this test, objects can teleport through other objects via rotation

		// Clear old location
		List<IntVector> oldLocs = AbsoluteLocations(_loc, _rotation);
		for (int i = 0; i < oldLocs.Count; i++) {
			WorldManager.g.SetContentsAt(oldLocs[i].x, oldLocs[i].y, oldLocs[i].z, null);
		}

		// Fill new location
		for (int i = 0; i < newLocs.Count; i++) {
			WorldManager.g.SetContentsAt(newLocs[i].x, newLocs[i].y, newLocs[i].z, this);
		}

		_loc = newLoc;
		_rotation = newRotation;

		WorldManager.g.UpdatePassability();
		return true;
	}

	public bool RotateAroundX(IntVector worldAnchor, int dir) {
		return RotateAroundAxis(worldAnchor, dir, 0);
	}

	public bool RotateAroundY(IntVector worldAnchor, int dir) {
		return RotateAroundAxis(worldAnchor, dir, 1);
	}

	public bool RotateAroundZ(IntVector worldAnchor, int dir) {
		return RotateAroundAxis(worldAnchor, dir, 2);
	}

	List<IntVector> AbsoluteLocations (IntVector location, Quaternion rotation) {
		List<IntVector> absoluteLocations = new List<IntVector>();
		for (int i = 0; i < _identityLocations.Count; i++) {
			absoluteLocations.Add(new IntVector(rotation * _identityLocations[i].ToVector3()) + location);
		}
		return absoluteLocations;
	}

	IntVector AbsoluteFromLocalOffset(IntVector loc, IntVector offset, Quaternion rot) {
		return new IntVector(rot * offset.ToVector3()) + loc;
	}

	void Start () {
		List<IntVector> all = AbsoluteLocations(_loc, _rotation);
		for (int i = 0; i < all.Count; i++) {
			WorldManager.g.SetContentsAt(all[i].x, all[i].y, all[i].z, this);
		}
		WorldManager.g.UpdatePassability();
	}

	// Update is called once per frame
	void Update () {
		IntVector anchor = AbsoluteFromLocalOffset(_loc, new IntVector(1,0,0), _rotation);
		Debug.DrawLine(anchor.ToVector3()*WorldManager.g.TileSize, (anchor.ToVector3() + (new IntVector(0,1,0)).ToVector3()*5)*WorldManager.g.TileSize);
		if (Input.GetKeyDown(KeyCode.A)) {
			RotateAroundY(anchor, 1);
		}
		if (Input.GetKeyDown(KeyCode.D)) {
			RotateAroundY(anchor, -1);
		}
		if (Input.GetKeyDown(KeyCode.W)) {
			RotateAroundZ(anchor, 1);
		}
		if (Input.GetKeyDown(KeyCode.S)) {
			RotateAroundZ(anchor, -1);
		}
		if (Input.GetKeyDown(KeyCode.Q)) {
			RotateAroundX(anchor, 1);
		}
		if (Input.GetKeyDown(KeyCode.E)) {
			RotateAroundX(anchor, -1);
		}
	}
}
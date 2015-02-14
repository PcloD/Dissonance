using UnityEngine;
using System;
using System.Collections;

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

    public int this[int index]
    {
        get
        {
        	switch (index) {
        		case 0:
        			return x;
        		case 1:
        			return y;
        		case 2:
        			return z;
        		default:
        			throw new Exception("Can't access "+index);
        	}
        }

        set
        {
        	switch (index) {
        		case 0:
        			x = value;
        			break;
        		case 1:
        			y = value;
        			break;
        		case 2:
        			z = value;
        			break;
        		default:
        			throw new Exception("Can't access "+index);
        	}
        }
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

[System.Serializable]
public struct IntVector2D {
	public int x;
	public int y;

	public override string ToString(){
		return x+", "+y;
	}

	public override int GetHashCode(){
		return x.GetHashCode() ^ y.GetHashCode();
	}

	public override bool Equals( object ob ){
		if( ob is IntVector2D ) {
			IntVector2D other = (IntVector2D) ob;
			return (other.x == x && other.y == y);
		}
		else {
			return false;
		}
	}

	public static bool operator ==(IntVector2D a, IntVector2D b) {
	  return (a.x == b.x && a.y == b.y);
	}

	public static bool operator !=(IntVector2D x, IntVector2D y) {
	  return !(x == y);
	}

	public static IntVector2D operator +(IntVector2D a, IntVector2D b) {
	  return new IntVector2D (a.x+b.x, a.y+b.y);
	}

	public static IntVector2D operator -(IntVector2D a, IntVector2D b) {
	  return new IntVector2D (a.x-b.x, a.y-b.y);
	}

	public Vector2 ToVector2 () {
		return new Vector2(x,y);
	}

    public int this[int index]
    {
        get
        {
        	switch (index) {
        		case 0:
        			return x;
        		case 1:
        			return y;
        		default:
        			throw new Exception("Can't access "+index);
        	}
        }

        set
        {
        	switch (index) {
        		case 0:
        			x = value;
        			break;
        		case 1:
        			y = value;
        			break;
        		default:
        			throw new Exception("Can't access "+index);
        	}
        }
    }

	public IntVector2D(int nx, int ny) {
		x = nx;
		y = ny;
	}

	public IntVector2D(Vector2 v) {
		x = Mathf.RoundToInt(v.x);
		y = Mathf.RoundToInt(v.y);
	}
}

using UnityEngine;
using System.Collections;

public class DPlane : MonoBehaviour
{
	private float _xDim;
	private float _yDim;
	private float _zDim;
	private MeshFilter _meshFilter;

	private Plane _plane;
	public Plane MathPlane {
		get { return _plane; }
	}

	void Awake () {
		_plane = new Plane(Normal, transform.position);
	}

	public Vector3[] Coords {
		get {
			Vector3 c1, c2, c3, c4;
			if (Orientation == PlaneOrientation.XY) {
				c1 = Origin + Up * _yDim;
				c2 = Origin + Up * _yDim - Right * _xDim;
				c3 = Origin - Right * _xDim;
				c4 = Origin;
			} else {
				c1 = Origin + Up * _yDim;
				c2 = Origin + Up * _yDim + Right * _zDim;
				c3 = Origin + Right * _zDim;
				c4 = Origin;
			}
			Vector3[] _coords = {c1,c2,c3,c4};
			return _coords;
		}
	}

	public Vector3 Origin {
		get { return transform.position; }
	}

	public PlaneOrientation Orientation {
		get {
			if (transform.forward.x > transform.forward.z) {
				return PlaneOrientation.XY;
			} else {
				return PlaneOrientation.ZY;
			}
		}
	}
	public Vector3 Up {
		get { return -transform.forward; }
	}
	public Vector3 Right {
		get { return -transform.right; }
	}
	public Vector3 Normal {
		get { return transform.up; }
	}
	public int Layer {
		get { return gameObject.layer; }
	}

	public void Init (float x, float y, float z)
	{
		_xDim = x;
		_yDim = y;
		_zDim = z;
		_meshFilter = GetComponent<MeshFilter> ();
		Mesh mesh = _meshFilter.sharedMesh;
		if (mesh == null) {
			mesh = new Mesh ();
			_meshFilter.sharedMesh = mesh;
		}

		if (Orientation == PlaneOrientation.XY) {
			RedrawPlane (x, y);
		} else {
			RedrawPlane (z, y);
		}
	}

	private void RedrawPlane (float width, float height)
	{
		Vector3[] vertices = new Vector3[4];
		int[] triangles = new int[6];
		if (Orientation == PlaneOrientation.ZY) {
			triangles [0] = 0;
			triangles [1] = 2;
			triangles [2] = 1;
			triangles [3] = 0;
			triangles [4] = 1;
			triangles [5] = 3;
		} else {
			triangles [0] = 0;
			triangles [1] = 1;
			triangles [2] = 2;
			triangles [3] = 0;
			triangles [4] = 3;
			triangles [5] = 1;
		}
		Vector3[] normals = new Vector3[4];
		Vector2[] uvs = new Vector2[4];
		if (Orientation == PlaneOrientation.ZY) {
			uvs [0] = new Vector2 (0, 0);
			uvs [1] = new Vector2 (1, 1);
			uvs [2] = new Vector2 (0, 1);
			uvs [3] = new Vector2 (1, 0);
		} else {
			uvs [1] = new Vector2 (0, 1);
			uvs [0] = new Vector2 (1, 0);
			uvs [3] = new Vector2 (0, 0);
			uvs [2] = new Vector2 (1, 1);
		}

		Vector3 anchorPoint = Vector3.zero;
		vertices [0] = anchorPoint;
		vertices [1] = anchorPoint + Vector3.forward * -height
			+ (Orientation == PlaneOrientation.ZY ? -1 : 1) * Vector3.right * width;
		vertices [2] = anchorPoint + Vector3.forward * -height;
		vertices [3] = anchorPoint + (Orientation == PlaneOrientation.ZY ? -1 : 1) * Vector3.right * width;

		for (int i = 0; i<4; i++) {
			normals [i] = Normal;
		}

		Mesh mesh = _meshFilter.sharedMesh;
		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.normals = normals;
		mesh.uv = uvs;
	}
}

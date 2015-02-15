using UnityEngine;
using System.Collections;

public class GenerateWorldEntityVis : MonoBehaviour {

	private WorldEntity _worldEntity;
	private Rotatable _rotatable;
	private MeshFilter _meshFilter;
	private MeshRenderer _meshRenderer;
	private Transform _meshTransform;
	private Transform _transform;

	void Awake () {
		_worldEntity = GetComponent<WorldEntity>();
		_rotatable = GetComponent<Rotatable>();
		_transform = transform;
	}

	void OnEnable () {
		if (_rotatable != null) {
			_rotatable.RotationHook += RotationPrep;
		}
	}

	void OnDisable () {
		if (_rotatable != null) {
			_rotatable.RotationHook -= RotationPrep;
		}
	}

	void Start () {
		var graphicsContainer = new GameObject();
		graphicsContainer.name = "GeneratedVisuals";
		_meshFilter = graphicsContainer.AddComponent<MeshFilter>();
		_meshRenderer = graphicsContainer.AddComponent<MeshRenderer>();
		_meshTransform = graphicsContainer.transform;
		_meshTransform.parent = transform;
		_meshTransform.position = (_worldEntity.Location.ToVector3() + Vector3.one/2f) * WorldManager.g.TileSize;
		MakeMesh();
	}

	private void MakeMesh () {
		Mesh mesh = _meshFilter.mesh;
		mesh.Clear();

		float length = WorldManager.g.TileSize;
		float width = WorldManager.g.TileSize;
		float height = WorldManager.g.TileSize;

		// VERTS
		Vector3 p0 = new Vector3( -length * .5f,	-width * .5f, height * .5f );
		Vector3 p1 = new Vector3( length * .5f, 	-width * .5f, height * .5f );
		Vector3 p2 = new Vector3( length * .5f, 	-width * .5f, -height * .5f );
		Vector3 p3 = new Vector3( -length * .5f,	-width * .5f, -height * .5f );

		Vector3 p4 = new Vector3( -length * .5f,	width * .5f,  height * .5f );
		Vector3 p5 = new Vector3( length * .5f, 	width * .5f,  height * .5f );
		Vector3 p6 = new Vector3( length * .5f, 	width * .5f,  -height * .5f );
		Vector3 p7 = new Vector3( -length * .5f,	width * .5f,  -height * .5f );

		Vector3[] vertices = new Vector3[] {
			// Bottom
			p0, p1, p2, p3,
			// Left
			p7, p4, p0, p3,
			// Front
			p4, p5, p1, p0,
			// Back
			p6, p7, p3, p2,
			// Right
			p5, p6, p2, p1,
			// Top
			p7, p6, p5, p4
		};

		// NORMALS
		Vector3 up 	= Vector3.up;
		Vector3 down 	= Vector3.down;
		Vector3 front 	= Vector3.forward;
		Vector3 back 	= Vector3.back;
		Vector3 left 	= Vector3.left;
		Vector3 right 	= Vector3.right;

		Vector3[] normals = new Vector3[] {
			// Bottom
			down, down, down, down,
			// Left
			left, left, left, left,
			// Front
			front, front, front, front,
			// Back
			back, back, back, back,
			// Right
			right, right, right, right,
			// Top
			up, up, up, up
		};


		// UVS
		Vector2 _00 = new Vector2( 0f, 0f );
		Vector2 _10 = new Vector2( 1f, 0f );
		Vector2 _01 = new Vector2( 0f, 1f );
		Vector2 _11 = new Vector2( 1f, 1f );

		Vector2[] uvs = new Vector2[] {
			// Bottom
			_11, _01, _00, _10,
			// Left
			_11, _01, _00, _10,
			// Front
			_11, _01, _00, _10,
			// Back
			_11, _01, _00, _10,
			// Right
			_11, _01, _00, _10,
			// Top
			_11, _01, _00, _10,
		};

		int[] triangles = new int[] {
			// Bottom
			3, 1, 0,
			3, 2, 1,
			// Left
			3 + 4 * 1, 1 + 4 * 1, 0 + 4 * 1,
			3 + 4 * 1, 2 + 4 * 1, 1 + 4 * 1,
			// Front
			3 + 4 * 2, 1 + 4 * 2, 0 + 4 * 2,
			3 + 4 * 2, 2 + 4 * 2, 1 + 4 * 2,
			// Back
			3 + 4 * 3, 1 + 4 * 3, 0 + 4 * 3,
			3 + 4 * 3, 2 + 4 * 3, 1 + 4 * 3,
			// Right
			3 + 4 * 4, 1 + 4 * 4, 0 + 4 * 4,
			3 + 4 * 4, 2 + 4 * 4, 1 + 4 * 4,
			// Top
			3 + 4 * 5, 1 + 4 * 5, 0 + 4 * 5,
			3 + 4 * 5, 2 + 4 * 5, 1 + 4 * 5,
		};

		var locs = _worldEntity.AbsoluteLocations (new IntVector(), Quaternion.identity);
		Vector3[] allVerts = new Vector3[vertices.Length * locs.Count];
		Vector3[] allNormals = new Vector3[normals.Length * locs.Count];
		Vector2[] allUvs = new Vector2[uvs.Length * locs.Count];
		int[] allTris = new int[triangles.Length * locs.Count];
		for (int i = 0; i < locs.Count; i++) {
			for (int j = 0; j < vertices.Length; j++) {
				var v = vertices[j] + (WorldManager.g.TileSize * (locs[i].ToVector3()));
				allVerts[i*vertices.Length + j] = v;
			}
			for (int j = 0; j < normals.Length; j++) {
				allNormals[i*normals.Length + j] = normals[j];
			}
			for (int j = 0; j < uvs.Length; j++) {
				allUvs[i*uvs.Length + j] = uvs[j];
			}
			for (int j = 0; j < triangles.Length; j++) {
				allTris[i*triangles.Length + j] = triangles[j] + (i * vertices.Length);
			}
		}

		mesh.vertices = allVerts;
		mesh.normals = allNormals;
		mesh.uv = allUvs;
		mesh.triangles = allTris;

		mesh.RecalculateBounds();
		mesh.Optimize();
		_meshFilter.mesh = mesh;

		_meshRenderer.sharedMaterial = new Material(Shader.Find("Diffuse"));
	}

	void RotationPrep () {
		_transform.position = (_rotatable.StateInfo.rotationAnchor + Vector3.one/2f) * WorldManager.g.TileSize;
		_meshTransform.position = (_worldEntity.Location.ToVector3() + Vector3.one/2f) * WorldManager.g.TileSize;
	}

	void Update () {
		if (_rotatable != null) {
			_transform.rotation = Quaternion.Slerp(_rotatable.StateInfo.lastRotation, _worldEntity.Rotation, _rotatable.StateInfo.fractionComplete);
		} else {
			_transform.position = (_worldEntity.Location.ToVector3() + Vector3.one/2f) * WorldManager.g.TileSize;
			_meshTransform.position = (_worldEntity.Location.ToVector3() + Vector3.one/2f) * WorldManager.g.TileSize;
			_meshTransform.rotation = _worldEntity.Rotation;
		}
	}
}
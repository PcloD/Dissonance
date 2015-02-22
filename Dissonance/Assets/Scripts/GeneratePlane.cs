﻿using UnityEngine;
using System.Collections;

public class GeneratePlane : MonoBehaviour {


	[SerializeField]
	PlaneOrientation _planeOrientation;

	[SerializeField]
	private float _width = 10f;
	[SerializeField]
	private float _height = 10f;

    private MeshFilter _meshFilter;
	private Plane _plane;

	void Awake () {
		Init();
	}

	void Init () {
		_plane = GetComponent<Plane>();
		_meshFilter = GetComponent<MeshFilter>();
        // Create the mesh
        Mesh mesh = new Mesh();

        _meshFilter.mesh = mesh;

        RedrawPlane(_width, _height);
	}

	// public void OnWidthChange(string newValue) {
	// 	_width = float.Parse(newValue);
	// 	RedrawPlane(_width, _height);
	// }

	// public void OnHeightChange(string newValue) {
	// 	_height = float.Parse(newValue);
	// 	RedrawPlane(_width, _height);
	// }

	void OnValidate () {
		_width = Mathf.Max(0f, _width);
		_height = Mathf.Max(0f, _height);
		Init();
	}

	private void RedrawPlane (float width, float height) {
	    Vector3[] vertices = new Vector3[4];
	    int[] triangles = new int[6];
	    if (_planeOrientation == PlaneOrientation.ZY) {
		    triangles[0] = 0;
			triangles[1] = 2;
			triangles[2] = 1;
			triangles[3] = 0;
			triangles[4] = 1;
			triangles[5] = 3;
	    } else {
		    triangles[0] = 0;
			triangles[1] = 1;
			triangles[2] = 2;
			triangles[3] = 0;
			triangles[4] = 3;
			triangles[5] = 1;
	    }
	    Vector3[] normals = new Vector3[4];
	    Vector2[] uvs = new Vector2[4];
	    if (_planeOrientation == PlaneOrientation.ZY) {
	    	uvs[0] = new Vector2(0,0);
	    	uvs[1] = new Vector2(1,1);
	    	uvs[2] = new Vector2(0,1);
	    	uvs[3] = new Vector2(1,0);
	    } else {
	    	uvs[1] = new Vector2(0,1);
	    	uvs[0] = new Vector2(1,0);
	    	uvs[3] = new Vector2(0,0);
	    	uvs[2] = new Vector2(1,1);
	    }

		Vector3 anchorPoint = Vector3.zero;
	    vertices[0] = anchorPoint;
	    vertices[1] = anchorPoint + Vector3.forward * -height
	     							+ (_planeOrientation == PlaneOrientation.ZY? -1 : 1) * Vector3.right * width;
	    vertices[2] = anchorPoint + Vector3.forward * -height;
	    vertices[3] = anchorPoint + (_planeOrientation == PlaneOrientation.ZY? -1 : 1) * Vector3.right * width;

	    for (int i = 0; i<4; i++) {
	    	normals[i] = _plane.Normal;
	    }

	    Mesh mesh = _meshFilter.sharedMesh;
	    mesh.vertices = vertices;
	    mesh.triangles = triangles;
	    mesh.normals = normals;
	    mesh.uv = uvs;
	}
}
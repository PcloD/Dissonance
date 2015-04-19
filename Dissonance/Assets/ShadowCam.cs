﻿using UnityEngine;
using System.Collections;

public class ShadowCam : MonoBehaviour {
	[SerializeField]
    Shader _shadowShader;
	Camera _camera;
	void Awake () {
		_camera = GetComponent<Camera>();
		_camera.SetReplacementShader(_shadowShader, null);
	}
}
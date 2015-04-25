using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class ShadowCam : MonoBehaviour {
	[SerializeField]
    Shader _shadowShader;
	Camera _camera;
	void Awake () {
		_camera = GetComponent<Camera>();
		_camera.SetReplacementShader(_shadowShader, "RenderType");
	}
}
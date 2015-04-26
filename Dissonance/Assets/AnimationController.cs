using UnityEngine;
using System.Collections;

public class AnimationController : MonoBehaviour {

	[SerializeField]
	Animator[] _animators;

	int _isWalkingID;

	// Use this for initialization
	void Start () {
		_isWalkingID = Animator.StringToHash("isWalking");
	}

	// Update is called once per frame
	void Update () {
		for (int i = 0; i < _animators.Length; i++) {
			_animators[i].SetBool(_isWalkingID, true);
		}
	}
}

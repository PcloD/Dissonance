using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class controlledMovingIndicator : MonoBehaviour {
	public RotationAnchorAnimator _anchorController;
//	public Transform centerMesh;
	float speed = 6;
	List<Transform> childrenTrans =  new List<Transform>();
	List<Vector3> childrenStartScale =  new List<Vector3>();
//	List<Vector3> childrenStartPos =  new List<Vector3>();

	void Start () {
		foreach(Transform child in transform){
			childrenTrans.Add(child);
			childrenStartScale.Add(child.localScale);
			//childrenStartPos.Add(child.position);
			child.GetComponent<Renderer>().enabled = false;
		}
	}
	

	void FixedUpdate () {
		if(_anchorController.currentstate == 0)
			disableChildren();
		else
			displayChildren();

	
	}

	void displayChildren(){
		for(int i = 0; i < childrenTrans.Count; i++){
			childrenTrans[i].GetComponent<Renderer>().enabled = true;
			childrenTrans[i].localScale = Vector3.Lerp(childrenTrans[i].localScale,childrenStartScale[i],Time.deltaTime*speed);
		//	childrenTrans[i].position =  Vector3.Lerp(childrenTrans[i].position, childrenStartPos[i],Time.deltaTime*speed);
		}
	}

	void disableChildren(){
		for(int i = 0; i < childrenTrans.Count; i++){
			childrenTrans[i].GetComponent<Renderer>().enabled = false;
			childrenTrans[i].localScale = Vector3.Lerp(childrenTrans[i].localScale,new Vector3(0,0,0),Time.deltaTime*speed);
		//	childrenTrans[i].position =  Vector3.Lerp(childrenTrans[i].position, centerMesh.position,Time.deltaTime*speed);
		}
	}
}

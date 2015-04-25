using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RotationAnchorAnimator : MonoBehaviour
{
	public List<Transform> anchorObj = new List<Transform>();
	List<Vector3> anchorObjOPos = new List<Vector3>();
	List<Vector3> anchorObjOScale = new List<Vector3>();
	public List<Transform> anchorObjTarget = new List<Transform>();
	public List<int> anchorObjOrder = new List<int>();
	public int currentstate = 0;
	int counter = 6;
	float speed = 6;

	void Start(){
		for (int i = 0; i < anchorObj.Count; i++){
			anchorObjOPos.Add(anchorObj[i].position);
			anchorObjOScale.Add(anchorObj[i].localScale);
		}
	}
	
		
	public void underCharAnimation ()
	{
		if(counter > 0)
		counter--;
		for (int i = 0; i < anchorObj.Count; i++){
			if(anchorObjOrder[i] == 1)
				lerpPosScale (anchorObj[i],anchorObjTarget[i].position,anchorObjTarget[i].localScale,1);
			else
				if(counter <= 0)
					lerpPosScale (anchorObj[i],anchorObjTarget[i].position,anchorObjTarget[i].localScale,1);
		}
	}
		
	public void noAnimation ()
	{
		if(counter < 6)
			counter++;
		for (int i = 0; i < anchorObj.Count; i++){
			if(anchorObjOrder[i] == 1)
				lerpPosScale (anchorObj[i],anchorObjOPos[i],anchorObjOScale[i],0);
			else
				if(counter >= 6)
					lerpPosScale (anchorObj[i],anchorObjOPos[i],anchorObjOScale[i],0);
		}
	}
		
	public void rotatingAnimation ()
	{
		currentstate = 2;
//		for (int i = 0; i < childrenTrans.Count; i++)
//			lerpPosScale (childrenTrans [i], startPos [i] + deltaDistance * 2, new Vector3 (0, 0, 0), 2);
		
	}

	void lerpPosScale(Transform trans, Vector3 pos, Vector3 scale, int state){
			trans.position = Vector3.Lerp(trans.position,pos,Time.deltaTime*speed);
			trans.localScale = Vector3.Lerp(trans.localScale,scale,Time.deltaTime*speed);
			currentstate = state;
	}

}
//	List<Vector3> startPos =  new List<Vector3>();
//	List<Vector3> startScale =  new List<Vector3>();
//	List<Transform> childrenTrans =  new List<Transform>();
//	float speed = 5;
//	Vector3 deltaDistance;
//	public bool iAmTrigger = false;
//	public int currentstate = 0;
//
//	void Start () {
//		foreach(Transform child in transform){
//			startPos.Add(child.position);
//			startScale.Add(child.localScale);
//			childrenTrans.Add(child);
//		}
//		if(iAmTrigger)
//			deltaDistance = new Vector3(0,1,0);
//			else
//			deltaDistance = new Vector3(0,1.5f,0);
//
//	}
//	
//
//	public void underCharAnimation(){
//		for(int i = 0; i < childrenTrans.Count; i++)
//			lerpPosScale(childrenTrans[i],startPos[i] + deltaDistance,startScale[i],1);
//
//	}
//
//	public void noAnimation(){
//		for(int i = 0; i < childrenTrans.Count; i++)
//			lerpPosScale(childrenTrans[i],startPos[i],startScale[i],0);
//
//	}
//
//	public void rotatingAnimation(){
//		for(int i = 0; i < childrenTrans.Count; i++)
//			lerpPosScale(childrenTrans[i],startPos[i] + deltaDistance*2,new Vector3(0,0,0),2);
//
//	}
//
//	void lerpPosScale(Transform trans, Vector3 pos, Vector3 scale, int state){
//		trans.position = Vector3.Lerp(trans.position,pos,Time.deltaTime*speed);
//		trans.localScale = Vector3.Lerp(trans.localScale,scale,Time.deltaTime*speed);
//		currentstate = state;
//	}
//	
//}

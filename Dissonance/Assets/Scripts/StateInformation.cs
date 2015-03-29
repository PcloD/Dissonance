using UnityEngine;

[System.Serializable]
public struct StateInformation {
	[HideInInspector]
	public State lastState;
	public State state;

	[HideInInspector]
	public float fractionComplete; // Range from 0-1, inclusive
	public FacingDir facingDirection;
	[HideInInspector]
	public IntVector2D lastLoc;
}
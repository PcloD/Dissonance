using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public struct RotationStateInformation {
    [HideInInspector]
    public RotationState lastState;
    public RotationState state;

    [HideInInspector]
    public float fractionComplete;
    // Range from 0-1, inclusive
    [HideInInspector]
    public Vector3 rotationAnchor;
    [HideInInspector]
    public Quaternion lastRotation;
    [HideInInspector]
    public IntVector lastLocation;
}

public enum RotationState {
    Idle,
    StartRotatingCounterClockwise,
    RotatingCounterClockwise,
    StartRotatingClockwise,
    RotatingClockwise
}

public class Rotatable : MonoBehaviour {

    private WorldEntity _worldEntity;

    public RotationStateInformation _currStateInfo;
    public RotationStateInformation StateInfo {
        get { return _currStateInfo; }
    }

    [SerializeField]
    List<IntVector> _explicitRelativeRotationAnchors;
    [SerializeField]
    List<RotationAnchorAnimator> _explicitRelativeRotationAnchorAnimationSystems;

    public delegate void RotationBeganDelegates();
    public RotationBeganDelegates RotationHook;

    public IntVector Location {
        get { return _worldEntity.Location; }
    }

    public bool IsCached {
        get {
            return _worldEntity != null;
        }
    }

    public void Cache () {
        _worldEntity = GetComponent<WorldEntity>();
    }

    void Awake () {
        Cache();
    }

    void OnEnable () {
        _worldEntity.Simulators += Simulate;
    }

    void OnDisable () {
        _worldEntity.Simulators -= Simulate;
    }

    void Start () {
        _currStateInfo.lastRotation = _worldEntity.Rotation;
        _currStateInfo.lastState = RotationState.Idle;
        _currStateInfo.state = RotationState.Idle;

        _currStateInfo.fractionComplete = 0f;
        _currStateInfo.rotationAnchor = _worldEntity.Location.ToVector3();
        _currStateInfo.lastRotation = _worldEntity.Rotation;
        _currStateInfo.lastLocation = _worldEntity.Location;
    }

    public void AnimateAtAnchor (Vector3 worldAnchor) {
        int axis = 1;
        if (_explicitRelativeRotationAnchors.Count > 0) {
            Vector3 offset = (worldAnchor - _worldEntity.Location.ToVector3() * WorldManager.g.TileSize);
            offset[axis] = 0f;
            for (int i = 0; i < _explicitRelativeRotationAnchors.Count; i++) {
            	#if UNITY_EDITOR
	                if (_explicitRelativeRotationAnchorAnimationSystems.Count < i + 1) {
	                	Debug.LogError("Selected object does not have enough _explicitRelativeRotationAnchorAnimationSystems!");
						Selection.activeGameObject = gameObject;
						// Debug.Break();
						continue;
	                }
                #endif
                var currVec = (_worldEntity.Rotation * (_explicitRelativeRotationAnchors[i].ToVector3() + Vector3.one * 0.5f * WorldManager.g.TileSize));
                currVec[axis] = 0f;
                if ((currVec - offset).magnitude < 0.01f) {
                    if (_currStateInfo.state == RotationState.RotatingClockwise || _currStateInfo.state == RotationState.RotatingCounterClockwise) {
                        _explicitRelativeRotationAnchorAnimationSystems[i].rotatingAnimation();
                    }
                    else {
                        _explicitRelativeRotationAnchorAnimationSystems[i].underCharAnimation(); //animation for rotation
                    }
                    return;
                }
                else {
                    _explicitRelativeRotationAnchorAnimationSystems[i].noAnimation();
                }
            }
        }
    }

    private bool CanRotateAroundAnchor (Vector3 worldAnchor, int axis) {
        if (_explicitRelativeRotationAnchors.Count > 0) {
            Vector3 offset = (worldAnchor - _worldEntity.Location.ToVector3() * WorldManager.g.TileSize);
            offset[axis] = 0f;
            for (int i = 0; i < _explicitRelativeRotationAnchors.Count; i++) {
                var currVec = (_worldEntity.Rotation * (_explicitRelativeRotationAnchors[i].ToVector3() + Vector3.one * 0.5f * WorldManager.g.TileSize));
                currVec[axis] = 0f;
                if ((currVec - offset).magnitude < 0.01f) {
                    return true;
                }
            }
            return false;
        }
        return true;
    }

    public bool CanYRotateAroundAnchor (Vector3 worldAnchor) {
        return CanRotateAroundAnchor(worldAnchor, 1);
    }

    public void RotateCounterClockwise (Vector3 worldAnchor) {
        if (_currStateInfo.state != RotationState.Idle) { return; }
        if (!CanYRotateAroundAnchor(worldAnchor)) { return; }
        _currStateInfo.state = RotationState.StartRotatingCounterClockwise;
        _currStateInfo.rotationAnchor = worldAnchor;
    }

    public void RotateClockwise (Vector3 worldAnchor) {
        if (_currStateInfo.state != RotationState.Idle) { return; }
        if (!CanYRotateAroundAnchor(worldAnchor)) { return; }
        _currStateInfo.state = RotationState.StartRotatingClockwise;
        _currStateInfo.rotationAnchor = worldAnchor;
    }

    private void Simulate () {
        float speed = 1f;
        if (_currStateInfo.state == RotationState.StartRotatingCounterClockwise) {
            _currStateInfo.state = RotationState.RotatingCounterClockwise;
            RotateAroundY(_currStateInfo.rotationAnchor, -1);
            _currStateInfo.fractionComplete = 0f;
        }
        else
        if (_currStateInfo.state == RotationState.StartRotatingClockwise) {
            _currStateInfo.state = RotationState.RotatingClockwise;
            RotateAroundY(_currStateInfo.rotationAnchor, 1);
            _currStateInfo.fractionComplete = 0f;
        }

        if (_currStateInfo.state != RotationState.Idle) {
            _currStateInfo.fractionComplete += speed * Time.deltaTime;
            if (_currStateInfo.fractionComplete >= 1f) {
                _currStateInfo.lastState = _currStateInfo.state;
                _currStateInfo.state = RotationState.Idle;
                _currStateInfo.lastRotation = _worldEntity.Rotation;
                _currStateInfo.lastLocation = _worldEntity.Location;
                _currStateInfo.fractionComplete = 0f;
            }
        }
    }

    private bool RotateAroundY (Vector3 worldAnchor, int dir) {
        return RotateAroundAxis(worldAnchor, dir, 1);
    }

    private bool RotateAroundAxis (Vector3 worldAnchor, int dir, int axis) {
        _currStateInfo.lastLocation = _worldEntity.Location;
        _currStateInfo.lastRotation = _worldEntity.Rotation;
        if (RotationHook != null) {
            RotationHook();
        }
        // Debug.DrawLine((worldAnchor+Vector3.one/2f)*WorldManager.g.TileSize, ((worldAnchor+Vector3.one/2f) + (new IntVector(0,1,0)).ToVector3()*5)*WorldManager.g.TileSize, Color.red, 1000f);
        Vector3 axisVector = Vector3.zero;
        axisVector[axis] = dir * 90;
        Quaternion additionalRotation = Quaternion.Euler(axisVector);
        Vector3 offset = (worldAnchor - _worldEntity.Location.ToVector3());
        Vector3 rotatedOffset = additionalRotation * offset;
        IntVector newLoc = new IntVector(offset - rotatedOffset + _worldEntity.Location.ToVector3());
        Quaternion newRotation = additionalRotation * _worldEntity.Rotation;

        // Test new location
        // List<IntVector> newLocs = AbsoluteLocations(newLoc, newRotation);
        // for (int i = 0; i < newLocs.Count; i++) {
        // 	TileContents c = WorldManager.g.ContentsAt(newLocs[i].x, newLocs[i].y, newLocs[i].z);
        // 	if (c != null && c.entity != null && c.entity != this) return false;
        // }

        // TODO(Zi): Test if swept rotation would intersect!
        // XXX: Without this test, objects can teleport through other objects via rotation

        // Clear old location
        // List<IntVector> oldLocs = AbsoluteLocations(_loc, _worldEntity.Rotation);
        // for (int i = 0; i < oldLocs.Count; i++) {
        // 	WorldManager.g.SetContentsAt(oldLocs[i].x, oldLocs[i].y, oldLocs[i].z, null);
        // }

        // Fill new location
        // for (int i = 0; i < newLocs.Count; i++) {
        // 	WorldManager.g.SetContentsAt(newLocs[i].x, newLocs[i].y, newLocs[i].z, this);
        // }


        // TODO(Julian): Prevent rotation if not possible!
        _worldEntity.Location = newLoc;
        _worldEntity.Rotation = newRotation;

        // WorldManager.g.UpdatePassability();
        return true;
    }

    void OnDrawGizmos () {
        Cache();
        for (int i = 0; i < _explicitRelativeRotationAnchors.Count; i++) {
            var o = (_worldEntity.Rotation * (_explicitRelativeRotationAnchors[i].ToVector3() + Vector3.one * 0.5f * WorldManager.g.TileSize) + _worldEntity.Location.ToVector3()) + Vector3.one * 0.5f;
            var v = o;
            var w = v;
            v *= WorldManager.g.TileSize;
            w *= WorldManager.g.TileSize;
            v.y += 1f;
            w.y -= 1f;
            Gizmos.DrawCube(o, Vector3.one * 0.5f * WorldManager.g.TileSize);
            Gizmos.DrawLine(v, w);
        }
    }
}
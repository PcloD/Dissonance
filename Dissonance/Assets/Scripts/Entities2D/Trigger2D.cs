using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public struct Trigger2DStateInformation {
    [HideInInspector]
    public Trigger2DState lastState;
    public Trigger2DState state;

    [HideInInspector]
    public float fractionComplete;
    // Range from 0-1, inclusive
    [HideInInspector]
    public IntVector2D lastLoc;
}

public enum Trigger2DState {
    Inactive,
    Activating,
    Active,
    Deactivating
}

public class Trigger2D : MonoBehaviour {

    private WorldEntity2D _worldEntity;

    private Trigger2DStateInformation _currStateInfo;
    public Trigger2DStateInformation StateInfo {
        get { return _currStateInfo; }
    }

    [SerializeField]
    Transform _visuals;

    [SerializeField]
    float _transitionSpeed = 10f;

    public PlaneOrientation Orientation {
        get { return _worldEntity.Orientation; }
    }

    public IntVector2D Location {
        get { return _worldEntity.Location; }
        set { _worldEntity.Location = value; }
    }

    public List<IntVector2D> AbsoluteLocations (IntVector2D location) {
        return _worldEntity.AbsoluteLocations(location);
    }

    public Vector2 VisualPos {
        get {
            switch (Orientation) {
                case PlaneOrientation.XY:
                    return (Vector2)_visuals.position;
                case PlaneOrientation.ZY:
                    return new Vector2(_visuals.position.z, _visuals.position.y);
                default:
                    return Vector2.zero;
            }
        }
    }

    public delegate void TriggerStateChangedDelegates();
    public TriggerStateChangedDelegates ActiveHook;

    public bool IsActive {
        get { return _currStateInfo.state == Trigger2DState.Active; }
    }

    void OnEnable () {
        _worldEntity.Simulators += Simulate;
    }

    void OnDisable () {
        _worldEntity.Simulators -= Simulate;
    }

    void Awake () {
        _worldEntity = GetComponent<WorldEntity2D>();
    }

    void Start () {
        _currStateInfo.lastLoc = Location;
    }

    void Simulate () {
    	if (_currStateInfo.state == Trigger2DState.Inactive && OverlappedByCharacter) {
			_currStateInfo.state = Trigger2DState.Activating;
    	} else if (_currStateInfo.state == Trigger2DState.Active && !OverlappedByCharacter) {
			_currStateInfo.state = Trigger2DState.Deactivating;
    	}
    }

    private bool OverlappedByCharacter {
    	get {
	    	var locations = AbsoluteLocations(Location);
	        for (int i = 0; i < locations.Count; i++) {
	            var contents = WorldManager.g.Contents2DAt (locations[i], Orientation);
	            foreach (var entity in contents) {
	            	if (!entity.GetComponent<Char2D>()) {
	            		return false;
	            	}
	            }
                if (contents.Count == 0) {
                    return false;
                }
	        }
	        return true;
    	}
    }

    void Update () {
        Vector3 v = Vector3.zero;
        // TODO(Julian): Add Animation here!
        float epsilon = 0.0001f;
        switch (Orientation) {
            case PlaneOrientation.XY:
                v = _currStateInfo.lastLoc.ToVector2();
                v.z = epsilon;
                break;
            case PlaneOrientation.ZY:
                v.z = _currStateInfo.lastLoc.x;
                v.y = _currStateInfo.lastLoc.y;
                v.x = epsilon;
                break;
        }
        _visuals.position = v * WorldManager.g.TileSize;

        if (_currStateInfo.state != Trigger2DState.Active &&
        	_currStateInfo.state != Trigger2DState.Inactive) {
        	_currStateInfo.fractionComplete += _transitionSpeed * Time.deltaTime;
        }

		if (_currStateInfo.fractionComplete >= 1f) {
			_currStateInfo.lastState = _currStateInfo.state;
	        if (_currStateInfo.state == Trigger2DState.Activating) {
	        	_currStateInfo.state = Trigger2DState.Active;
                if (ActiveHook != null) {
                    ActiveHook();
                }
	        } else if (_currStateInfo.state == Trigger2DState.Deactivating) {
	        	_currStateInfo.state = Trigger2DState.Inactive;
                if (ActiveHook != null) {
                    ActiveHook();
                }
	        }
	        _currStateInfo.fractionComplete = 0f;
		}
    }
}

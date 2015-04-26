using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class WorldEntity : MonoBehaviour
{

    private bool _castsShadows = true;
    public bool CastsShadows
    {
        get { return _castsShadows; }
        set { _castsShadows = value; }
    }

    [SerializeField]
    protected IntVector
        _loc = new IntVector(5, 5, 5);
    public IntVector Location
    {
        get { return _loc; }
        set { _loc = value; }
    }

    Quaternion _rotation = Quaternion.identity;
    public Quaternion Rotation
    {
        get { return _rotation; }
        set { _rotation = value; }
    }

    [SerializeField]
    private List<IntVector>
        _identityLocations = new List<IntVector>();
    public void SetIdentityLocations(List<IntVector> newLocs)
    {
        _identityLocations = newLocs;
    }

    private bool _registered = false;

    public void RegisterMe()
    {
        if (!_registered)
        {
            WorldManager.g.RegisterEntity(this);
            _registered = true;
        }
    }

    public void DeregisterMe()
    {
        if (_registered)
        {
            WorldManager.g.DeregisterEntity(this);
            _registered = false;
        }
    }

    void Start()
    {
        RegisterMe();
    }

    public List<IntVector> AbsoluteLocations(IntVector location, Quaternion rotation)
    {
        List<IntVector> absoluteLocations = new List<IntVector>();
        for (int i = 0; i < _identityLocations.Count; i++)
        {
            absoluteLocations.Add(new IntVector(rotation * _identityLocations [i].ToVector3()) + location);
        }
        return absoluteLocations;
    }

    public delegate void SimulatorDelegates();
    public SimulatorDelegates Simulators;

    public void Simulate()
    {
        if (Simulators != null)
        {
            Simulators();
        }
    }

    void OnDrawGizmos()
    {
        // NOTE(Julian): For debug visualization in Unity editor
        if (!Application.isPlaying && WorldManager.g != null)
        {
            Rotatable r = GetComponent<Rotatable>();
            MovementMachine m = GetComponent<MovementMachine>();
            Color color;
            if (r != null)
            {
                color = Color.blue;
            } else if (m != null)
            {
                color = Color.green;
            } else
            {
                color = Color.white;
            }

            float tileSize = WorldManager.g.TileSize;
            var all = AbsoluteLocations(_loc, _rotation);
            int x, y, z;
            for (int i = 0; i < all.Count; i++)
            {
                x = all [i].x;
                y = all [i].y;
                z = all [i].z;
                Gizmos.color = color;
                Gizmos.DrawCube(new Vector3(x + 0.5f, y + 0.5f, z + 0.5f) * tileSize, Vector3.one * tileSize * 0.90f);

                Gizmos.color = Color.black;

                Gizmos.DrawLine(new Vector3(tileSize * (x), tileSize * (y), 0f),
								new Vector3(tileSize * (x + 1f), tileSize * (y + 1f), 0f));
                Gizmos.DrawLine(new Vector3(tileSize * (x), tileSize * (y + 1f), 0f),
								new Vector3(tileSize * (x + 1f), tileSize * (y), 0f));

                Gizmos.DrawLine(new Vector3(0f, tileSize * (y), tileSize * (z)),
								new Vector3(0f, tileSize * (y + 1f), tileSize * (z + 1f)));
                Gizmos.DrawLine(new Vector3(0f, tileSize * (y + 1f), tileSize * (z)),
								new Vector3(0f, tileSize * (y), tileSize * (z + 1f)));

            }
        }
    }

    // private bool RotateAroundAxis(IntVector worldAnchor, int dir, int axis) {
    // 	// Vector3 axisVector = Vector3.zero;
    // 	// axisVector[axis] = dir*90;
    // 	// Quaternion additionalRotation = Quaternion.Euler(axisVector);
    // 	// IntVector offset = (worldAnchor - _loc);
    // 	// IntVector rotatedOffset = new IntVector(additionalRotation * offset.ToVector3());
    // 	// IntVector newLoc = offset - rotatedOffset + _loc;
    // 	// Quaternion newRotation = additionalRotation * _rotation;

    // 	// // Test new location
    // 	// List<IntVector> newLocs = AbsoluteLocations(newLoc, newRotation);
    // 	// for (int i = 0; i < newLocs.Count; i++) {
    // 	// 	TileContents c = WorldManager.g.ContentsAt(newLocs[i].x, newLocs[i].y, newLocs[i].z);
    // 	// 	if (c != null && c.entity != null && c.entity != this) return false;
    // 	// }

    // 	// // TODO(Zi): Test if swept rotation would intersect!
    // 	// // XXX: Without this test, objects can teleport through other objects via rotation

    // 	// // Clear old location
    // 	// List<IntVector> oldLocs = AbsoluteLocations(_loc, _rotation);
    // 	// for (int i = 0; i < oldLocs.Count; i++) {
    // 	// 	WorldManager.g.SetContentsAt(oldLocs[i].x, oldLocs[i].y, oldLocs[i].z, null);
    // 	// }

    // 	// // Fill new location
    // 	// for (int i = 0; i < newLocs.Count; i++) {
    // 	// 	WorldManager.g.SetContentsAt(newLocs[i].x, newLocs[i].y, newLocs[i].z, this);
    // 	// }

    // 	// _loc = newLoc;
    // 	// _rotation = newRotation;

    // 	// WorldManager.g.UpdatePassability();
    // 	// return true;
    // 	return false;
    // }

    // public bool RotateAroundX(IntVector worldAnchor, int dir) {
    // 	return RotateAroundAxis(worldAnchor, dir, 0);
    // }

    // public bool RotateAroundY(IntVector worldAnchor, int dir) {
    // 	return RotateAroundAxis(worldAnchor, dir, 1);
    // }

    // public bool RotateAroundZ(IntVector worldAnchor, int dir) {
    // 	return RotateAroundAxis(worldAnchor, dir, 2);
    // }

    // IntVector AbsoluteFromLocalOffset(IntVector loc, IntVector offset, Quaternion rot) {
    // 	return new IntVector(rot * offset.ToVector3()) + loc;
    // }

    // Update is called once per frame
    // void Update () {
    // 	IntVector anchor = AbsoluteFromLocalOffset(_loc, new IntVector(1,0,0), _rotation);
    // 	Debug.DrawLine(anchor.ToVector3()*WorldManager.g.TileSize, (anchor.ToVector3() + (new IntVector(0,1,0)).ToVector3()*5)*WorldManager.g.TileSize);
    // 	if (Input.GetKeyDown(KeyCode.A)) {
    // 		RotateAroundY(anchor, 1);
    // 	}
    // 	if (Input.GetKeyDown(KeyCode.D)) {
    // 		RotateAroundY(anchor, -1);
    // 	}
    // 	if (Input.GetKeyDown(KeyCode.W)) {
    // 		RotateAroundZ(anchor, 1);
    // 	}
    // 	if (Input.GetKeyDown(KeyCode.S)) {
    // 		RotateAroundZ(anchor, -1);
    // 	}
    // 	if (Input.GetKeyDown(KeyCode.Q)) {
    // 		RotateAroundX(anchor, 1);
    // 	}
    // 	if (Input.GetKeyDown(KeyCode.E)) {
    // 		RotateAroundX(anchor, -1);
    // 	}
    // }
}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WorldEntity2D : MonoBehaviour {

    [SerializeField]
    PlaneOrientation _planeOrientation;
    public PlaneOrientation Orientation {
        get { return _planeOrientation; }
    }

    [SerializeField]
    List<IntVector2D> _identityLocations = new List<IntVector2D>();
    [SerializeField]
    IntVector2D _loc = new IntVector2D();
    public IntVector2D Location {
        get { return _loc; }
        set { _loc = value; }
    }

    void Start () {
        WorldManager.g.RegisterEntity(this);
    }

    public delegate void SimulatorDelegates();
    public SimulatorDelegates Simulators;

    public void Simulate () {
        if (Simulators != null) {
            Simulators();
        }
    }

    public List<IntVector2D> AbsoluteLocations (IntVector2D location) {
        List<IntVector2D> absoluteLocations = new List<IntVector2D>();
        for (int i = 0; i < _identityLocations.Count; i++) {
            absoluteLocations.Add(_identityLocations[i] + location);
        }
        return absoluteLocations;
    }

    void OnDrawGizmos () {
        // NOTE(Julian): For debug visualization in Unity editor
        if (!Application.isPlaying && WorldManager.g != null) {
            float tileSize = WorldManager.g.TileSize;
            List<IntVector2D> all = AbsoluteLocations(_loc);
            int x, y, z;
            if (GetComponent<Trigger2D>()) {
                Gizmos.color = Color.cyan;
            }
            else {
                Gizmos.color = Color.green;
            }

            for (int i = 0; i < all.Count; i++) {
                y = all[i][1];
                switch (_planeOrientation) {
                    case PlaneOrientation.XY:
                        x = all[i][0];
                        Gizmos.DrawLine(new Vector3(tileSize * (x), tileSize * (y), 0f),
                            new Vector3(tileSize * (x + 1f), tileSize * (y + 1f), 0f));
                        Gizmos.DrawLine(new Vector3(tileSize * (x), tileSize * (y + 1f), 0f),
                            new Vector3(tileSize * (x + 1f), tileSize * (y), 0f));
                        break;
                    case PlaneOrientation.ZY:
                        z = all[i][0];
                        Gizmos.DrawLine(new Vector3(0f, tileSize * (y), tileSize * (z)),
                            new Vector3(0f, tileSize * (y + 1f), tileSize * (z + 1f)));
                        Gizmos.DrawLine(new Vector3(0f, tileSize * (y + 1f), tileSize * (z)),
                            new Vector3(0f, tileSize * (y), tileSize * (z + 1f)));
                        break;
                }

            }
        }
    }
}
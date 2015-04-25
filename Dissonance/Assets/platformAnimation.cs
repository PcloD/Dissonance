using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlatformAnimation : MonoBehaviour {
    GameObject beneathVisualInst;
    List<GameObject> beneathVisualObjs = new List<GameObject>();
    List<Vector4> beneathVisualPlace = new List<Vector4>();
    float speed = 6;

    void Start () {
        if (GameObject.Find("beneathVisual").gameObject && GetComponent<ModelWorldEntityVis>()) {
            foreach (IntVector unit in GetComponent<WorldEntity>().all) {
                GameObject obj = Instantiate(GameObject.Find("beneathVisual").gameObject) as GameObject;
                obj.transform.position = (unit.ToVector3() + Vector3.one / 2) * WorldManager.g.TileSize;
                obj.transform.parent = GetComponent<ModelWorldEntityVis>()._visualChild.transform;
                beneathVisualObjs.Add(obj.gameObject);
                Vector4 p = new Vector4(unit.x, unit.y, unit.z, 0);
                beneathVisualPlace.Add(p);//Zi: If w=0, it means no Char is standing there
            }
        }
    }

    void FixedUpdate () {
        if (GetComponent<Rotatable>()) {
            if (GetComponent<Rotatable>()._currStateInfo.state == RotationState.Idle) {
                pfPositionIndicator();
            }
            if (GetComponent<Rotatable>()._currStateInfo.state == RotationState.RotatingClockwise ||
                GetComponent<Rotatable>()._currStateInfo.state == RotationState.RotatingCounterClockwise) {
                for (int i = 0; i < beneathVisualObjs.Count; i++) {
                    Vector3 nextScale = new Vector3(0, 0, 0);
                    beneathVisualObjs[i].transform.localScale = Vector3.Lerp(beneathVisualObjs[i].transform.localScale, nextScale, Time.deltaTime * speed);
                }
            }
        }
        else {
            pfPositionIndicator();
        }


    }

    void pfPositionIndicator () {
        for (int i = 0; i < beneathVisualObjs.Count; i++) {
            Vector4 p = beneathVisualPlace[i];
            p.w = 0;
            for (int j = 0; j < XYZChar.g.XYPositions.Length; j++) {
                if (p.x == (float)XYZChar.g.XYPositions[j].x &&
                    p.y == (float)XYZChar.g.XYPositions[j].y - 1f) {
                    p.w = p.w + 1;
                }
                if (p.z == (float)XYZChar.g.ZYPositions[j].x &&
                    p.y == (float)XYZChar.g.ZYPositions[j].y - 1f) {
                    p.w = p.w + 1;
                }
            }
            beneathVisualPlace[i] = p;

            Vector3 nextScale = new Vector3(0, 0, 0);
            if (beneathVisualPlace[i].w == 1) {
                nextScale = new Vector3(1f, 2.5f, 1f);
            } else if (beneathVisualPlace[i].w > 1) {
                nextScale = new Vector3(1f, 3.2f, 1f);
            }
            beneathVisualObjs[i].transform.localScale = Vector3.Lerp(beneathVisualObjs[i].transform.localScale, nextScale, Time.deltaTime * speed);
        }
    }
}

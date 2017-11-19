using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;

public class AdjustUI3D: MonoBehaviour {
    public int fieldOfView = 23;
    float oldZ = float.PositiveInfinity;

    void Start() {
        Update();
    }

    void Update() {
        if(transform.position.z != oldZ) {
            oldZ = transform.position.z;
            float l1 = LogicScreen.height / 2f / Mathf.Tan(fieldOfView * 0.5f * Mathf.Deg2Rad);
            float l2 = l1 + oldZ;
            float z = l2 / l1;
            transform.localScale = new Vector3(z, z, 1);
        }
    }
}
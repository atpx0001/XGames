using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;

[ExecuteInEditMode]
public class AdjustUICamera3D: MonoBehaviour {
    private int oldW;
    private int oldH;
    Camera c;

    void Awake() {
        c = GetComponent<Camera>();
        Update();
    }

    void Update() {
        if(Screen.width != oldW || Screen.height != oldH) {
            float size = LogicScreen.height / 2f;
            transform.position = new Vector3(0, 0, -size / Mathf.Tan(c.fieldOfView * 0.5f * Mathf.Deg2Rad));
            oldW = Screen.width;
            oldH = Screen.height;
        }
    }
}
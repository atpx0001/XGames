using UnityEngine;
using UnityEngine.UI;
using System.Linq;

[ExecuteInEditMode]
public class AdjustSubCanvas: MonoBehaviour {
    RectTransform c;
    RectTransform cp;

    void Awake() {
        Update();
    }

    void Update() {
        if(c == null) {
            Canvas cs = GetComponent<Canvas>();
            if(cs != null) {
                c = cs.GetComponent<RectTransform>();
            }
        }
        if(cp == null) {
            AdjustCanvas cs = GetComponentInParent<AdjustCanvas>();
            if(cs != null) {
                cp = cs.GetComponent<RectTransform>();
            }
        }
        if(c == null || cp == null) {
            return;
        }

        if(c.sizeDelta != cp.sizeDelta) {
            c.sizeDelta = cp.sizeDelta;
        }
        if(c.position != cp.position) {
            c.position = cp.position;
        }
    }
}
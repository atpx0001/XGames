using UnityEngine;
using System.Collections;

public class AutoMoveByPath: MonoBehaviour {
    public BezierCreater[] paths;
    public AnimationCurve[] curves;
    public Transform lookAt;
    public float duration = 1;
    BezierCreater path = null;
    AnimationCurve curve = null;
    float _dt = 0;
    public void Play(int i = 0) {
        path = paths[i];
        curve = curves[i];
        _dt = 0;
    }

    void FixedUpdate() {
        if(path == null) return;
        _dt += Time.deltaTime;
        _dt = Mathf.Min(_dt, duration);
        float rt = _dt / duration;
        if(curve != null)
            rt = curve.Evaluate(rt);
        Vector3 pos = path.GetPointAtRate(rt);
        transform.localPosition = pos;
        if(lookAt != null)
            transform.LookAt(lookAt);
        if(rt == 1) {
            path = null;
        }
    }

    public void TestPlay0() {
        Play(0);
    }
    public void TestPlay1() {
        Play(1);
    }
}

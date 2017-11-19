using UnityEngine;
using System.Collections.Generic;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class BezierCreater: MonoBehaviour {
    public float length = 0;
    public List<PointPart> points = new List<PointPart>();
    private Vector3 _srcPosition;

    public Vector3 SrcPosition {
        get {
            if(!Application.isPlaying) {
                _srcPosition = transform.position;
                return _srcPosition;
            } else {
                return _srcPosition;
            }
        }
        set { _srcPosition = value; }
    }
    void Start() {
        _srcPosition = transform.position;
    }

    Vector3 WorldPoint(Vector3 v) {
        return (transform.rotation * v * transform.lossyScale.x) + SrcPosition;
    }
    public Vector3 GetPointAtRate(float rate) {
        rate = rate < 0 ? 0 : rate;
        rate = rate > 1 ? 1 : rate;
        if(rate == 1) {
            PointPart pp = points[points.Count - 1];
            return WorldPoint(pp.points[pp.points.Count - 1]);
        }
        float dl = length * rate;
        float nowl = 0;
        for(int i = 0; i < points.Count; i++) {
            float nextl = nowl + points[i].length;
            if(dl >= nowl && dl <= nextl) {
                for(int j = 0; j < points[i].points.Count - 1; j++) {
                    float lp2p = Vector3.Distance(points[i].points[j], points[i].points[j + 1]);
                    nextl = nowl + lp2p;
                    if(dl >= nowl && dl <= nextl) {
                        float lat = dl - nowl;
                        float realr = lat / lp2p;
                        return WorldPoint(Vector3.Lerp(points[i].points[j], points[i].points[j + 1], realr));
                    }
                    nowl = nextl;
                }
            }
            nowl = nextl;
        }
        return Vector3.zero;
    }

    [Serializable]
    public class PointPart {
        public float length = 0;
        public List<Vector3> points = new List<Vector3>();
    }
#if UNITY_EDITOR
    public List<BCPoint> bcList = new List<BCPoint>();
    [Serializable]
    public class BCPoint {
        public Vector3 point = Vector3.zero;
        public Vector3 lp = Vector3.zero;
        public Vector3 rp = Vector3.zero;
        public bool locklr = true;
        public bool selected = false;
        public bool selectedlp = false;
        public bool selectedrp = false;
    }

    void OnDrawGizmos() {
        Gizmos.color = Color.blue;
        if(points.Count >= 1) {
            Handles.color = Color.blue;
            for(int i = 0; i < points.Count; i++) {
                BezierCreater.PointPart pp = points[i];
                for(int j = 0; j < pp.points.Count - 1; j++) {
                    Vector3 pos0 = WorldPoint(pp.points[j]);
                    Vector3 pos1 = WorldPoint(pp.points[j + 1]);
                    Handles.DrawLine(pos0, pos1);
                }
            }
        }
    }
#endif
}

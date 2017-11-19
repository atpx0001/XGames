using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
[CustomEditor(typeof(BezierCreater), true)]
public class BezierCreaterEditor: Editor {
    public override void OnInspectorGUI() {
        ShowDefault();
    }

    bool isInBox(Vector3 pos, Vector3 boxpos, Vector3 boxsize, bool passZ = true) {
        float hx = boxsize.x * 0.5f;
        float hy = boxsize.y * 0.5f;
        float hz = boxsize.z * 0.5f;
        float x1 = boxpos.x - hx;
        float x2 = boxpos.x + hx;
        float y1 = boxpos.y - hy;
        float y2 = boxpos.y + hy;
        float z1 = boxpos.z - hz;
        float z2 = boxpos.z + hz;
        if(passZ) {
            pos.z = boxpos.z;
        }
        if(pos.x >= x1 && pos.x <= x2
            && pos.y >= y1 && pos.y <= y2
            && pos.z >= z1 && pos.z <= z2) {
            return true;
        }
        return false;
    }

    void ReCalcBezier() {
        BezierCreater bc = target as BezierCreater;
        bc.points.Clear();
        bc.length = 0;
        List<Vector3> plist = new List<Vector3>();
        Vector3 prevPos;
        for(int i = 0; i < bc.bcList.Count - 1; i++) {
            BezierCreater.PointPart pp = new BezierCreater.PointPart();
            plist.Clear();
            //plist.Add(bc.bcList[i].point + bc.bcList[i].lp);
            plist.Add(bc.bcList[i].point);
            if(bc.bcList[i].rp != Vector3.zero)
                plist.Add(bc.bcList[i].point + bc.bcList[i].rp);

            if(bc.bcList[i + 1].lp != Vector3.zero)
                plist.Add(bc.bcList[i + 1].point + bc.bcList[i + 1].lp);
            plist.Add(bc.bcList[i + 1].point);
            //plist.Add(bc.bcList[i + 1].point + bc.bcList[i + 1].rp);

            CalcBeizer(plist, pp);
            bc.points.Add(pp);
            bc.length += pp.length;
        }
    }
    bool MouseLogic() {
        bool repaint = false;
        BezierCreater bc = target as BezierCreater;

        if(Event.current.isMouse) {
            if(Event.current.type == EventType.MouseDown) {
                foreach(BezierCreater.BCPoint p in bc.bcList) {
                    Vector3 pos = bc.SrcPosition + p.point;
                    Vector3 poslp = pos + p.lp;
                    Vector3 posrp = pos + p.rp;
                    Vector3 posOnGUI = HandleUtility.WorldToGUIPoint(pos);
                    if(Vector3.Distance(Event.current.mousePosition, posOnGUI) < _capSize * 100) {
                        if(!Event.current.shift) {
                            foreach(BezierCreater.BCPoint p2 in bc.bcList) {
                                p2.selected = false;
                                p2.selectedlp = false;
                                p2.selectedrp = false;
                            }
                        }
                        p.selected = true;
                        //Tools.current = Tool.View;
                    } else if(isInBox(Event.current.mousePosition, HandleUtility.WorldToGUIPoint(poslp), Vector3.one * _subCapSize * 1000)) {
                        foreach(BezierCreater.BCPoint p2 in bc.bcList) {
                            p2.selected = false;
                            p2.selectedlp = false;
                            p2.selectedrp = false;
                        }
                        p.selectedlp = true;
                        //Tools.current = Tool.View;
                    } else if(isInBox(Event.current.mousePosition, HandleUtility.WorldToGUIPoint(posrp), Vector3.one * _subCapSize * 1000)) {
                        foreach(BezierCreater.BCPoint p2 in bc.bcList) {
                            p2.selected = false;
                            p2.selectedlp = false;
                            p2.selectedrp = false;
                        }
                        p.selectedrp = true;
                        //Tools.current = Tool.View;
                    }
                }

            }
        }

        return repaint;
    }
    public void OnSceneGUI() {
        bool repaint = false;
        //repaint = MouseLogic();
        BezierCreater bc = target as BezierCreater;
        Vector3 pmove = Vector3.zero;
        foreach(BezierCreater.BCPoint p in bc.bcList) {
            Vector3 pos = bc.SrcPosition + p.point;
            Vector3 poslp = pos + p.lp;
            Vector3 posrp = pos + p.rp;

            // 绘制修正点
            if(p.selected || p.selectedlp || p.selectedrp) {
                Handles.color = p.selectedlp ? Color.red : Color.blue;
                if(Handles.Button(poslp, Quaternion.identity, _subCapSize, _subCapSize, Handles.DotCap)) {
                    if(!Event.current.shift) {
                        p.selected = false;
                        p.selectedrp = false;
                    }
                    p.selectedlp = true;
                }
                Handles.DrawLine(poslp, pos);

                Handles.color = p.selectedrp ? Color.red : Color.green;
                if(Handles.Button(posrp, Quaternion.identity, _subCapSize, _subCapSize, Handles.DotCap)) {
                    if(!Event.current.shift) {
                        p.selected = false;
                        p.selectedlp = false;
                    }
                    p.selectedrp = true;
                }
                Handles.DrawLine(posrp, pos);
            }
            // 绘制标准点
            Handles.color = p.selected ? Color.red : Color.white;
            if(Handles.Button(pos, Quaternion.identity, _capSize, _capSize, Handles.SphereCap)) {
                if(!Event.current.shift)
                    BCSelectNone();
                p.selected = true;
            }
            if(p.selectedlp || p.selectedrp) {
                Handles.color = Color.red * 0.5f;
            }

            if(p.selected) {
                Vector3 npos = Handles.DoPositionHandle(pos, Quaternion.identity);
                if(npos != pos) {
                    pmove = npos - pos;
                    repaint = true;
                }
            } else if(p.selectedlp) {
                Vector3 npos = Handles.DoPositionHandle(poslp, Quaternion.identity);
                if(npos != poslp) {
                    pmove = npos - poslp;
                    p.lp += pmove;
                    if(p.locklr)
                        p.rp = -p.lp;
                    repaint = true;
                }
            } else if(p.selectedrp) {
                Vector3 npos = Handles.DoPositionHandle(posrp, Quaternion.identity);
                if(npos != posrp) {
                    pmove = npos - posrp;
                    p.rp += pmove;
                    if(p.locklr)
                        p.lp = -p.rp;
                    repaint = true;
                }
            }
        }
        if(pmove != Vector3.zero) {
            foreach(BezierCreater.BCPoint p in bc.bcList) {
                if(p.selected) {
                    p.point += pmove;
                }
            }
        }
        if(_isLockPosition) {
            bc.transform.position = _lockPosition;
        }


        if(_showToggleTest) {
            if(_showTestAuto > 0) {
                _posTest += Time.deltaTime * _showTestAuto;
                _posTest = _posTest % 1;
                SceneView.RepaintAll();
            }
            Handles.color = Color.blue;
            Vector3 pos = bc.GetPointAtRate(_posTest);
            Handles.SphereCap(0, pos, Quaternion.identity, _capSize);
            if(Camera.main != null) {
                Camera.main.transform.position = pos;
                if(_cameraLookAt != null)
                    Camera.main.transform.LookAt(_cameraLookAt);
            }
        }
        if(repaint) {
            SceneView.RepaintAll();
            ReCalcBezier();
        }
    }


    static bool showDefault = false;
    static Vector3 _lockPosition = Vector3.zero;
    static bool _isLockPosition = false;
    static float _capSize = 0.2f;
    static float _posTest = 0;
    static bool _showToggleTest = false;
    static float _showTestAuto = 0;
    static Vector3 _cameraRot = Vector3.down;
    static Transform _cameraLookAt = null;
    float _subCapSize { get { return _capSize * 0.2f; } }
    void BCSelectNone() {
        BezierCreater bc = target as BezierCreater;
        foreach(BezierCreater.BCPoint bp in bc.bcList) {
            bp.selected = false;
            bp.selectedlp = false;
            bp.selectedrp = false;
        }
    }
    void ShowDefault() {
        BezierCreater bc = target as BezierCreater;
        bool repaint = false;
        GUILayout.Label("总长度: " + bc.length);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("标记大小");
        float ival = EditorGUILayout.Slider(_capSize, 0.02f, 1f);
        if(ival != _capSize) {
            _capSize = ival;
            repaint = true;
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("平滑度");
        ival = EditorGUILayout.Slider(_srate, 0.1f, 1f);
        if(ival != _srate) {
            _srate = ival;
            repaint = true;
        }
        EditorGUILayout.EndHorizontal();
        bool bval = EditorGUILayout.ToggleLeft("位置测试", _showToggleTest, GUILayout.Width(60));
        if(bval != _showToggleTest) {
            _showToggleTest = bval;
            repaint = true;
            _cameraRot = Camera.main.transform.rotation.eulerAngles;
        }
        if(_showToggleTest) {
            _cameraRot = EditorGUILayout.Vector3Field("摄像机角度", _cameraRot);
            _cameraLookAt = EditorGUILayout.ObjectField("观察目标", _cameraLookAt, typeof(Transform), true) as Transform;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("自动速度");
            _showTestAuto = EditorGUILayout.Slider(_showTestAuto, 0, 2);
            EditorGUILayout.EndHorizontal();
            if(_showTestAuto > 0) {
                SceneView.RepaintAll();
                repaint = true;
            }
            ival = EditorGUILayout.Slider(_posTest, 0.0f, 1f);
            if(ival != _posTest) {
                _posTest = ival;
                repaint = true;
            }

        }
        bval = EditorGUILayout.ToggleLeft("锁定位置", _isLockPosition);
        if(bval != _isLockPosition) {
            _isLockPosition = bval;
            _lockPosition = bc.transform.position;
        }
        EditorGUILayout.BeginHorizontal();
        if(GUILayout.Button("新路点")) {
            BezierCreater.BCPoint nbp = new BezierCreater.BCPoint();
            bc.bcList.Add(nbp);
            BCSelectNone();
            nbp.selected = true;
            repaint = true;
        }
        if(GUILayout.Button("全选")) {
            BCSelectNone();
            foreach(BezierCreater.BCPoint p in bc.bcList) {
                p.selected = true;
            }
            repaint = true;
        }
        if(GUILayout.Button("全不选")) {
            BCSelectNone();
            repaint = true;
        }
        EditorGUILayout.EndHorizontal();
        int delIdx = -1;
        int moveUpIdx = -1;
        int moveDownIdx = -1;
        for(int i = 0; i < bc.bcList.Count; i++) {
            Rect rect = EditorGUILayout.BeginHorizontal();
            if(bc.bcList[i].selected || bc.bcList[i].selectedlp || bc.bcList[i].selectedrp) {
                rect.size = new Vector2(rect.size.x, rect.size.y * 2 + 5);
                EditorGUI.DrawRect(rect, Color.blue);
            } else {
                EditorGUI.DrawRect(rect, Color.black);
            }
            bool val = EditorGUILayout.ToggleLeft("", bc.bcList[i].selected, GUILayout.Width(15));
            if(val != bc.bcList[i].selected) {
                if(!Event.current.shift) {
                    BCSelectNone();
                }
                bc.bcList[i].selected = val;
                repaint = true;
            }
            Vector3 newpos = EditorGUILayout.Vector3Field("", bc.bcList[i].point);
            if(newpos != bc.bcList[i].point) {
                bc.bcList[i].point = newpos;
                repaint = true;
            }

            val = GUILayout.Toggle(bc.bcList[i].locklr, "对称锁定");
            if(val != bc.bcList[i].locklr) {
                bc.bcList[i].locklr = val;
                bc.bcList[i].rp = -bc.bcList[i].lp;
                repaint = true;
            }
            if(bc.bcList[i].selected || bc.bcList[i].selectedlp || bc.bcList[i].selectedrp) {
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                if(GUILayout.Button("↑", GUILayout.Width(20))) {
                    moveUpIdx = i;
                    repaint = true;
                }
                if(GUILayout.Button("↓", GUILayout.Width(20))) {
                    moveDownIdx = i;
                    repaint = true;
                }

                if(GUILayout.Button("无弧度", GUILayout.Width(50))) {
                    bc.bcList[i].lp = Vector3.zero;
                    bc.bcList[i].rp = Vector3.zero;
                    repaint = true;
                }

                if(GUILayout.Button("左右", GUILayout.Width(40))) {
                    SetSwapLR(bc.bcList[i], Vector3.left);
                    repaint = true;
                }
                if(GUILayout.Button("上下", GUILayout.Width(40))) {
                    SetSwapLR(bc.bcList[i], Vector3.up);
                    repaint = true;
                }
                if(GUILayout.Button("前后", GUILayout.Width(40))) {
                    SetSwapLR(bc.bcList[i], Vector3.forward);
                    repaint = true;
                }
                if(GUILayout.Button("删除", GUILayout.Width(40))) {
                    if(EditorUtility.DisplayDialog("删除节点", "要删除这个节点吗？", "是", "否")) {
                        BCSelectNone();
                        delIdx = i;
                        repaint = true;
                    }
                }
                if(GUILayout.Button("复制", GUILayout.Width(40))) {
                    BezierCreater.BCPoint nbp = new BezierCreater.BCPoint();
                    nbp.point = bc.bcList[i].point;
                    bc.bcList.Add(nbp);
                    BCSelectNone();
                    nbp.selected = true;
                    repaint = true;
                }
            }
            if(i == 0) {
                bc.bcList[i].lp = Vector3.zero;
            }
            if(i == bc.bcList.Count - 1) {
                bc.bcList[i].rp = Vector3.zero;
            }
            EditorGUILayout.EndHorizontal();
        }
        if(moveUpIdx > 0) {
            BezierCreater.BCPoint bp = bc.bcList[moveUpIdx];
            bc.bcList.RemoveAt(moveUpIdx);
            bc.bcList.Insert(moveUpIdx - 1, bp);
        }
        if(moveDownIdx >= 0 && moveDownIdx < bc.bcList.Count - 1) {
            BezierCreater.BCPoint bp = bc.bcList[moveDownIdx];
            bc.bcList.RemoveAt(moveDownIdx);
            bc.bcList.Insert(moveDownIdx + 1, bp);
        }
        if(delIdx >= 0) {
            bc.bcList.RemoveAt(delIdx);
        }
        if(GUILayout.Button("Save")) {
            repaint = true;
        }


        showDefault = EditorGUILayout.Foldout(showDefault, "原始信息");
        if(showDefault) {
            base.DrawDefaultInspector();
        }

        if(repaint) {
            EditorUtility.SetDirty(target);
            SceneView.RepaintAll();
            ReCalcBezier();
        }
    }

    void SetSwapLR(BezierCreater.BCPoint bp, Vector3 dir) {
        if(bp.lp == Vector3.zero)
            bp.lp = dir;
        if(bp.rp == Vector3.zero)
            bp.rp = -dir;
        if(bp.lp == dir && bp.rp == -dir) {
            bp.lp = -dir;
            bp.rp = dir;
        } else {
            bp.lp = dir;
            bp.rp = -dir;
        }
    }

    float _srate = 0.3f;
    Vector3[] CalcBeizer(List<Vector3> plist, BezierCreater.PointPart part) {
        int n = 0;
        if((n = plist.Count) < 2)
            return null;
        float step = _srate / (float)n;

        List<Vector3> ret = new List<Vector3>();

        float[] xarray = new float[n - 1];
        float[] yarray = new float[n - 1];
        float[] zarray = new float[n - 1];
        float x = plist[0].x;
        float y = plist[0].y;
        float z = plist[0].z;
        Vector3 npos = Vector3.zero;
        for(float t = 0.0f; t <= 1; t += step) // 调整参数t,计算贝塞尔曲线上的点的坐标,t即为上述u  
        {
            for(int i = 1; i < n; ++i) {
                for(int j = 0; j < n - i; ++j) {
                    if(i == 1) // i==1时,第一次迭代,由已知控制点计算  
                    {
                        xarray[j] = plist[j].x * (1 - t) + plist[j + 1].x * t;
                        yarray[j] = plist[j].y * (1 - t) + plist[j + 1].y * t;
                        zarray[j] = plist[j].z * (1 - t) + plist[j + 1].z * t;
                        continue;
                    }
                    // i != 1时,通过上一次迭代的结果计算  
                    xarray[j] = xarray[j] * (1 - t) + xarray[j + 1] * t;
                    yarray[j] = yarray[j] * (1 - t) + yarray[j + 1] * t;
                    zarray[j] = zarray[j] * (1 - t) + zarray[j + 1] * t;
                }
            }

            x = xarray[0];
            y = yarray[0];
            z = zarray[0];
            npos = new Vector3(x, y, z);
            ret.Add(npos);
            if(part != null) {
                if(part.points.Count > 0) {
                    part.length += Vector3.Distance(part.points[part.points.Count - 1], npos);
                }
                part.points.Add(npos);
            }
        }
        if(npos != plist[plist.Count - 1]) {
            npos = plist[plist.Count - 1];
            ret.Add(npos);
            if(part != null) {
                if(part.points.Count > 0) {
                    part.length += Vector3.Distance(part.points[part.points.Count - 1], npos);
                }
                part.points.Add(npos);
            }
        }

        // 优化掉过路点
        if(part.points.Count > 2) {
            Vector3 prevDir = Vector3.zero;
            List<Vector3> removePoints = new List<Vector3>();
            for(int i = 1; i < part.points.Count; i++) {
                Vector3 p0 = part.points[i - 1];
                Vector3 p1 = part.points[i];
                Vector3 dir = p1 - p0;
                if(dir != prevDir) {
                    prevDir = dir;
                } else {
                    removePoints.Add(p0);
                }
            }
            foreach(var p in removePoints) {
                part.points.Remove(p);
            }
        }

        return ret.ToArray();
    }
}

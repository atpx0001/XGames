using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class TouchData {
    public int id;
    public Vector2 downPos;
    public Vector2 lastPos;
    public Vector2 nowPos;
    public Vector3 rawPos;
    public float startTime;

    public TouchData(int fingerId) {
        this.id = fingerId;
        SetNewRawPos(Input.mousePosition);
        this.downPos = nowPos;
        this.lastPos = nowPos;
        this.startTime = Time.time;
    }

    public TouchData(Touch touch) {
        this.id = touch.fingerId;
        SetNewRawPos(touch.position);
        this.downPos = nowPos;
        this.lastPos = nowPos;
        this.startTime = Time.time;
    }

    public TouchData(int fingerId, Vector2 pos) {
        this.id = fingerId;
        SetNewPos(pos);
        this.downPos = nowPos;
        this.lastPos = nowPos;
        this.startTime = Time.time;
    }

    public void SetNewRawPos(Vector3 raw) {
        this.lastPos = this.nowPos;
        this.rawPos = raw;
        this.nowPos = TransPos(raw);
    }

    public void SetNewPos(Vector2 pos) {
        this.lastPos = this.nowPos;
        this.nowPos = pos;
    }

    public static Vector2 TransPos(Vector2 pos) {
        return pos * (LogicScreen.width / (float)Screen.width) - new Vector2(LogicScreen.width / 2f, LogicScreen.height / 2f);
    }

    public static Vector2 TransPosToRaw(Vector2 pos) {
        return (pos + new Vector2(LogicScreen.width / 2f, LogicScreen.height / 2f)) / (LogicScreen.width / (float)Screen.width);
    }
}


public class TouchInput {
    static TouchInput instance;
    public static TouchInput Instance {
        get {
#if UNITY_EDITOR
            if(!UnityEditor.EditorApplication.isPlaying) {
                return null;
            }
#endif
            if(instance == null) {
                instance = new TouchInput();
                Updater.Instance.OnPostUpdate.Add(instance.Update, int.MaxValue / 2);
            }
            return instance;
        }
    }

    public const int MAX_TOUCH = 2;
    const int SIM_FINGER_ID = -1;

    public event Action<TouchData> onTouchBegin;
    public event Action<TouchData> onTouchMove;
    public event Action<TouchData> onTouchEnd;

    private CachedDictionary<int, TouchData> allTouchs = new CachedDictionary<int, TouchData>();
    private HashSet<int> processed = new HashSet<int>();

    public int TouchCount {
        get {
            return allTouchs.Count;
        }
    }

    public TouchData[] Touchs {
        get {
            return allTouchs.Values;
        }
    }

    void Update() {
        processed.Clear();

        bool onUI = false;

        int count = Mathf.Min(MAX_TOUCH, Input.touchCount);
        if(count > 0) {
            for(int i = 0; i < count; i++) {
                Touch touch = Input.GetTouch(i);
                //onUI = GameUtil.IsTouchOnUI(touch.fingerId);
                TouchData t;
                switch(touch.phase) {
                    case TouchPhase.Began: {
                        if(!onUI) {
                            processed.Add(touch.fingerId);
                            if(allTouchs.TryGetValue(touch.fingerId, out t)) {
                                OnTouchEnd(t);
                            }
                            t = new TouchData(touch);
                            OnTouchBegin(t);
                        }
                        break;
                    }
                    case TouchPhase.Moved: {
                        if(allTouchs.TryGetValue(touch.fingerId, out t)) {
                            processed.Add(touch.fingerId);
                            t.SetNewRawPos(touch.position);
                            OnTouchMove(t);
                        }
                        break;
                    }
                    case TouchPhase.Canceled:
                    case TouchPhase.Ended: {
                        if(allTouchs.TryGetValue(touch.fingerId, out t)) {
                            processed.Add(touch.fingerId);
                            t.SetNewRawPos(touch.position);
                            OnTouchEnd(t);
                        }
                        break;
                    }
                    case TouchPhase.Stationary: {
                        if(allTouchs.TryGetValue(touch.fingerId, out t)) {
                            processed.Add(touch.fingerId);
                            t.lastPos = t.nowPos;
                        }
                        break;
                    }
                }
            }
        } else {
            if(Input.GetMouseButtonDown(0)) {
                //onUI = GameUtil.IsTouchOnUI(-1);
                if(!onUI) {
                    processed.Add(SIM_FINGER_ID);
                    TouchData t;
                    if(allTouchs.TryGetValue(SIM_FINGER_ID, out t)) {
                        OnTouchEnd(t);
                    }
                    t = new TouchData(SIM_FINGER_ID);
                    OnTouchBegin(t);
                }
            } else if(Input.GetMouseButton(0)) {
                TouchData t;
                if(allTouchs.TryGetValue(SIM_FINGER_ID, out t)) {
                    processed.Add(SIM_FINGER_ID);
                    if(Input.mousePosition != t.rawPos) {
                        t.SetNewRawPos(Input.mousePosition);
                        OnTouchMove(t);
                    } else {
                        t.lastPos = t.nowPos;
                    }
                }
            } else {
                TouchData t;
                if(allTouchs.TryGetValue(SIM_FINGER_ID, out t)) {
                    processed.Add(SIM_FINGER_ID);
                    t.SetNewRawPos(Input.mousePosition);
                    OnTouchEnd(t);
                }
            }
        }

        foreach(TouchData touch in allTouchs.Values.Where(t => !processed.Contains(t.id))) {//防止因为各种原因漏掉end的touch
            OnTouchEnd(touch);
        }

        foreach(TouchData touch in allTouchs.Values) {
            OnTouchHold(touch);
        }
    }

    void EndAll() {
        foreach(TouchData t in allTouchs.Values) {
            OnTouchEnd(t);
        }
    }

    void OnTouchBegin(TouchData t) {
        allTouchs[t.id] = t;
        if(onTouchBegin != null) {
            onTouchBegin(t);
        }
    }

    void OnTouchHold(TouchData t) {

    }

    void OnTouchMove(TouchData t) {
        if(onTouchMove != null) {
            onTouchMove(t);
        }
    }

    void OnTouchEnd(TouchData t) {
        allTouchs.Remove(t.id);
        if(onTouchEnd != null) {
            onTouchEnd(t);
        }
    }

}
using UnityEngine;
using System.Collections;

public class ShowFPS: MonoBehaviour {
    /// <summary>
    /// 每次刷新计算的时间      帧/秒
    /// </summary>
    public float updateInterval = 0.5f;
    /// <summary>
    /// 最后间隔结束时间
    /// </summary>
    private double lastInterval;
    private int frames = 0;
    private float currFPS;


    // Use this for initialization
    void Start() {
        lastInterval = Time.realtimeSinceStartup;
        frames = 0;
    }

    // Update is called once per frame
    void Update() {
        ++frames;
        float timeNow = Time.realtimeSinceStartup;
        if(timeNow > lastInterval + updateInterval) {
            currFPS = (float)(frames / (timeNow - lastInterval));
            frames = 0;
            lastInterval = timeNow;
        }
    }

    public GameObject obj;
    private void OnGUI() {
        GUI.color = Color.white;
        GUI.Label(new Rect(0, 0, 100, 20), "FPS:" + currFPS.ToString("f2"));
        GUI.Label(new Rect(2, 2, 100, 20), "FPS:" + currFPS.ToString("f2"));
        GUI.Label(new Rect(0, 2, 100, 20), "FPS:" + currFPS.ToString("f2"));
        GUI.Label(new Rect(2, 0, 100, 20), "FPS:" + currFPS.ToString("f2"));
        GUI.Label(new Rect(1, 0, 100, 20), "FPS:" + currFPS.ToString("f2"));
        GUI.Label(new Rect(1, 2, 100, 20), "FPS:" + currFPS.ToString("f2"));
        GUI.Label(new Rect(0, 1, 100, 20), "FPS:" + currFPS.ToString("f2"));
        GUI.Label(new Rect(2, 1, 100, 20), "FPS:" + currFPS.ToString("f2"));
        GUI.color = Color.red;
        GUI.Label(new Rect(1, 1, 100, 20), "FPS:" + currFPS.ToString("f2"));
        GUI.color = Color.white;
        if(obj != null)
            if(GUI.Button(new Rect(0, 100, 200, 100), "TEST")) {
                obj.SetActive(!obj.activeSelf);
            }
    }

}
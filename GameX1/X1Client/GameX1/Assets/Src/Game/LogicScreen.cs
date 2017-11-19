using UnityEngine;
using System.Collections;

public class LogicScreen {
    public static int width {
        get {
            Update();
            return _width;
        }
    }
    public static int height {
        get {
            Update();
            return _height;
        }
    }
    private static int _width;
    private static int _height;
    private static int oldW;
    private static int oldH;

    public static void Update() {
        if (Screen.width != oldW || Screen.height != oldH)
        {
            if (Screen.width * 2 < Screen.height * 3)
            {
                _width = 1080;
                _height = Mathf.CeilToInt((float)Screen.height * _width / Screen.width);
            }
            else
            {
                //_width = 720;
                _height = 720;// Mathf.CeilToInt((float)Screen.height * _width / Screen.width);
                //if(_height < 1080) {
                //_height = 1080;
                _width = Mathf.CeilToInt((float)Screen.width * _height / Screen.height);
            }

            //}
            oldW = Screen.width;
            oldH = Screen.height;
        }
    }
}

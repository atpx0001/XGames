using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

public struct LngLat {
    public float lat;
    public float lng;
    public static readonly LngLat zero = new LngLat(0, 0);

    public LngLat(float lng, float lat) {
        this.lat = lat;
        this.lng = lng;
    }

    public static bool operator ==(LngLat a, LngLat b) {
        return (a.lng == b.lng && a.lat == b.lat);
    }

    public static bool operator !=(LngLat a, LngLat b) {
        return !(a == b);
    }
    public override int GetHashCode() {
        return (this.lng.GetHashCode() ^ (this.lat.GetHashCode() << 2));
    }

    public override bool Equals(object other) {
        if(!(other is LngLat)) {
            return false;
        }
        LngLat lngLat = (LngLat)other;
        return (this.lng.Equals(lngLat.lng) && this.lat.Equals(lngLat.lat));
    }

    public override string ToString() {
        return string.Format("(lng: {0}, lat: {1})", lng, lat);
    }

    public static implicit operator Vector2(LngLat loc) {
        return new Vector2(loc.lng, loc.lat);
    }
}

public static class LocationManagerX {
#if UNITY_ANDROID && !UNITY_EDITOR

    static AndroidJavaClass cls;
    static AndroidJavaClass Cls {
        get {
            if(cls == null) {
                cls = new AndroidJavaClass("com.go2play.lib.LocationManagerX");
            }
            return cls;
        }
    }

    public static void Start() {
        Cls.CallStatic("startLocation");
    }

    public static void Stop() {
        Cls.CallStatic("stopLocation");
    }

    public static LocationServiceStatus Status {
        get {
            return (LocationServiceStatus)Cls.CallStatic<int>("getStatus");
        }
    }

    public static bool IsEnabledByUser {
        get {
            return Input.location.isEnabledByUser;
        }
    }

    public static LngLat LastLocation {
        get {
            LngLat loc = new LngLat();
            string locStr = Cls.CallStatic<string>("getLocation");
            if(!string.IsNullOrEmpty(locStr)) {
                string[] tmp = locStr.Split(',');
                double p = 0;
                if(double.TryParse(tmp[0], out p)) {
                    loc.lng = (float)p;
                }
                if(double.TryParse(tmp[1], out p)) {
                    loc.lat = (float)p;
                }
            }
            return loc;
        }
    }
#elif UNITY_EDITOR
    static float time;
    static LocationServiceStatus state = LocationServiceStatus.Stopped;
    public static void Start() {
        if(state == LocationServiceStatus.Stopped) {
            time = Time.realtimeSinceStartup + 3;
            state = LocationServiceStatus.Initializing;
        }
    }

    public static void Stop() {
        state = LocationServiceStatus.Stopped;
    }

    public static LocationServiceStatus Status {
        get {
            if(state == LocationServiceStatus.Initializing && Time.realtimeSinceStartup > time) {
                state = LocationServiceStatus.Running;
            }
            return state;
        }
    }

    public static bool IsEnabledByUser {
        get {
            return true;
        }
    }

    public static LngLat LastLocation {
        get {
            return new LngLat(116.3f, 39.9f);
        }
    }
#else
    public static void Start() {
        Input.location.Start(100, 10);
    }

    public static void Stop() {
        Input.location.Stop();
    }

    public static LocationServiceStatus Status {
        get {
            return Input.location.status;
        }
    }

    public static bool IsEnabledByUser {
        get {
            return Input.location.isEnabledByUser;
        }
    }

    public static LngLat LastLocation {
        get {
            return new LngLat(Input.location.lastData.longitude, Input.location.lastData.latitude);
        }
    }
#endif
}
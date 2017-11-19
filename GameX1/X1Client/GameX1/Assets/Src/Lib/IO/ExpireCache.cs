using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ExpireCache {
    static readonly string savepath = Application.temporaryCachePath + "/expire/";
    const int timeout = 3;
    static string todayPath;
    static List<string> dirs = new List<string>();

    static string EnsurePath() {
        if(!Directory.Exists(savepath)) {
            Directory.CreateDirectory(savepath);
        }
        if(dirs.Count == 0) {
            string[] ds = Directory.GetDirectories(savepath);
            dirs.Capacity = ds.Length;
            DateTime today = DateTime.UtcNow.Date;
            for(int i = ds.Length; --i >= 0;) {
                string d = Path.GetFileName(ds[i]);
                long t;
                if(!long.TryParse(d, out t) || (today - DateTime.FromBinary(t)).TotalDays > timeout) {
                    Directory.Delete(ds[i], true);
                    continue;
                }
                dirs.Add(ds[i].Replace("\\", "/") + "/");
            }
        }
        if(todayPath == null) {
            todayPath = savepath + DateTime.UtcNow.Date.ToBinary() + "/";
            dirs.Remove(todayPath);
            dirs.Add(todayPath);
        }
        if(!Directory.Exists(todayPath)) {
            Directory.CreateDirectory(todayPath);
        }
        return todayPath;
    }

    public static string Save(string key, byte[] data) {
        string p = EnsurePath() + key;
        string dir = Path.GetDirectoryName(p);
        if(!Directory.Exists(dir)) {
            Directory.CreateDirectory(dir);
        }
        File.WriteAllBytes(p, data);
        return p;
    }

    public static string GetPath(string key) {
        EnsurePath();
        string p = dirs[dirs.Count - 1] + key;
        return p;
    }

    public static bool TryGetPath(string key, out string path, out bool isToday) {
        EnsurePath();
        for(int i = dirs.Count; --i >= 0;) {
            string p = dirs[i] + key;
            if(File.Exists(p)) {
                isToday = dirs[i] == todayPath;
                path = p;
                return true;
            }
        }
        isToday = false;
        path = null;
        return false;
    }

    public static bool TryLoad(string key, out byte[] data) {
        EnsurePath();
        for(int i = dirs.Count; --i >= 0;) {
            string p = dirs[i] + key;
            if(File.Exists(p)) {
                data = File.ReadAllBytes(p);
                return true;
            }
        }
        data = null;
        return false;
    }
}

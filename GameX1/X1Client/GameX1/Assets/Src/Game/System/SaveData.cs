using UnityEngine;
using System.IO;
using System;

public class SaveData {
    static readonly string savepath = Application.temporaryCachePath;
    static readonly string savepath2 = Application.persistentDataPath;
    static readonly string seedpath = savepath + "/seeds.txt";
    public static void Init() {
        Debug.Log(savepath);
    }

    public static void SaveSeed(BonValue seed) {
        byte[] bytes = seed.ToBonBytes();
        bytes = EncryptDecrypt.Encrypt(bytes);
        File.WriteAllBytes(seedpath, bytes);
        Debug.Log("SD_AllSeeds saved." + seedpath);
    }

    public static void LoadSeedOnce() {
        if(MainData.allseeds == null) {
            LoadSeed();
        }
    }

    public static void LoadSeed() {
        if(File.Exists(seedpath)) {
            try {
                byte[] bytes = File.ReadAllBytes(seedpath);
                bytes = EncryptDecrypt.Decrypt(bytes);
                BonDocument bd = BonDocument.FromBonBytes(bytes);
                SD_AllSeeds seeds = new SD_AllSeeds();
                seeds.FromBon(bd);
                MainData.allseeds = seeds;
                Debug.Log("SD_AllSeeds loaded." + seedpath);
            } catch(Exception e) {
                Debug.LogError("SD_AllSeeds load failed.");
                Debug.LogError(e);
            }
        } else {
            Debug.Log("SD_AllSeeds was not found.");
        }
    }

    public static void ClearSeed() {
        if(File.Exists(seedpath)) {
            try {
                File.Delete(seedpath);
                Debug.Log("SD_AllSeeds deleted." + seedpath);
            } catch(Exception e) {
                Debug.LogError("SD_AllSeeds deleted failed.");
                Debug.LogError(e);
            }
        }
    }

    static void _SaveObj(string path, object obj) {
        string dir = Path.GetDirectoryName(path);
        if(!Directory.Exists(dir)) {
            Directory.CreateDirectory(dir);
        }
        byte[] bytes = BinarySerialize.Serialize(obj);
        bytes = EncryptDecrypt.Encrypt(bytes);
        File.WriteAllBytes(path, bytes);
        Debug.Log(path + " saved.");
    }

    static object _LoadObj(string path) {
        if(!File.Exists(path)) {
            return null;
        }
        try {
            byte[] bytes = File.ReadAllBytes(path);
            bytes = EncryptDecrypt.Decrypt(bytes);
            return BinarySerialize.Deserialize(bytes);
        } catch(Exception e) {
            return null;
        }
    }

    public static object LoadObj(string name) {
        string path = savepath + "/" + name + ".sav";
        return _LoadObj(path);
    }

    public static T LoadObj<T>(string name) where T : class {
        return LoadObj(name) as T;
    }

    public static void SaveObj(string name, object obj) {
        string path = savepath + "/" + name + ".sav";
        _SaveObj(path, obj);
    }

    public static object LoadObj(int rid, string name) {
        string path = string.Format("{0}/{1}/{2}.sav", savepath, rid, name);
        return _LoadObj(path);
    }

    public static T LoadObj<T>(int rid, string name) where T : class {
        return LoadObj(rid, name) as T;
    }

    public static void SaveObj(int rid, string name, object obj) {
        string path = string.Format("{0}/{1}/{2}.sav", savepath, rid, name);
        _SaveObj(path, obj);
    }

    public static object LoadObjPersistent(string name) {
        string path = savepath2 + "/" + name + ".sav";
        return _LoadObj(path);
    }

    public static T LoadObjPersistent<T>(string name) where T : class {
        return LoadObjPersistent(name) as T;
    }

    public static void SaveObjPersistent(string name, object obj) {
        string path = savepath2 + "/" + name + ".sav";
        _SaveObj(path, obj);
    }

    public static object LoadObjPersistent(int uId, string name) {
        string path = string.Format("{0}/{1}/{2}.sav", savepath2, uId, name);
        Debug.Log("path = " + path);
        return _LoadObj(path);
    }

    public static T LoadObjPersistent<T>(int uId, string name) where T : class {
        return LoadObjPersistent(uId, name) as T;
    }

    public static void SaveObjPersistent(int uId, string name, object obj) {
        string path = string.Format("{0}/{1}/{2}.sav", savepath2, uId, name);
        _SaveObj(path, obj);
    }


    public static void DeleteObj(string name) {
        string path = savepath + "/" + name + ".sav";
        if(File.Exists(path)) {
            File.Delete(path);
        }
    }

    public static void DeleteObj(int uId, string name) {
        string path = string.Format("{0}/{1}/{2}.sav", savepath, uId, name);
        if(File.Exists(path)) {
            File.Delete(path);
        }
    }

    public static void DeleteObjPersistent(string name) {
        string path = savepath2 + "/" + name + ".sav";
        if(File.Exists(path)) {
            File.Delete(path);
        }
    }

    public static void DeleteObjPersistent(int uId, string name) {
        string path = string.Format("{0}/{1}/{2}.sav", savepath2, uId, name);
        if(File.Exists(path)) {
            File.Delete(path);
        }
    }
}

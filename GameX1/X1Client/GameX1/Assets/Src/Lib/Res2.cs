#define DEBUG_RES
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;
using UnityEngine.Events;
using System.Threading;
using UnityEngine.SceneManagement;

public class LoadProgress {
    public UnityAction<LoadProgress> onComplete;
    public UnityAction<LoadProgress> onProgress;
    private float progress;
    public float Progress {
        get { return progress; }
        set {
            float p = Mathf.Clamp01(value);
            if(p != progress) {
                progress = p;
                if(onProgress != null) {
                    onProgress(this);
                }
            }
        }
    }
    public bool IsDone { get; private set; }
    public string Error { get; private set; }
    public bool IsError {
        get {
            return Error != null;
        }
    }

    public void SetDone(string err = null) {
        this.Error = err;
        if(err == null) {
            this.Progress = 1;
        }
        IsDone = true;
        if(onComplete != null) {
            onComplete(this);
        }
    }
}

public class LoadProgress<T>: LoadProgress {
    public T obj;
}

public class PakRoot {
    public string ver;
    public List<PakInfo> paks = new List<PakInfo>();
    public Dictionary<string, PakInfo> paksMap { get; set; }
    public Dictionary<string, PakInfo> res2pak { get; set; }

    public PakRoot() {
        paksMap = new Dictionary<string, PakInfo>();
        res2pak = new Dictionary<string, PakInfo>();
    }

    public PakRoot(BonValue bon) {
        if(bon == null) {
            ver = "";
            paksMap = new Dictionary<string, PakInfo>();
            res2pak = new Dictionary<string, PakInfo>();
        } else {
            BonUtil.ToObj<PakRoot>(bon, this, null);
            paksMap = paks.ToDictionary(p => p.name);
            for(int ii = paks.Count; --ii >= 0; ) {
                PakInfo pak = paks[ii];
                pak.allDependences = new List<string>(pak.dependences);
                int i = 0;
                while(i < pak.allDependences.Count) {
                    PakInfo pak2 = paksMap[pak.allDependences[i]];
                    for(int i2 = pak2.dependences.Count; --i2 >= 0; ) {
                        string dpn = pak2.dependences[i2];
                        if(!pak.allDependences.Contains(dpn)) {
                            pak.allDependences.Add(dpn);
                        }
                    }
                    i++;
                }
            }
            res2pak = new Dictionary<string, PakInfo>();
            for(int ii = paks.Count; --ii >= 0; ) {
                PakInfo pak = paks[ii];
                for(int i = pak.ress.Count; --i >= 0; ) {
                    res2pak[pak.ress[i].ToLower()] = pak;
                }
            }
        }
    }
}

public class PakInfo {
    public string name;
    public string hash;
    public int size;
    public bool isStatic;
    public List<string> ress;
    public List<string> dependences;
    public List<string> allDependences { get; set; }
}

public class LoadInfo {
    /// <summary>
    /// 需要加载的包
    /// </summary>
    public List<string> paks = new List<string>();
    public int totalSize;
}

public static class Res2 {
    static string[] PAK_PATH_SPLIT = new string[] {
        "00","01","02","03","04","05","06","07","08","09","0a","0b","0c","0d","0e","0f","10","11","12","13","14","15","16","17","18","19","1a","1b","1c","1d","1e","1f","20","21","22","23","24","25","26","27","28","29","2a","2b","2c","2d","2e","2f","30","31","32","33","34","35","36","37","38","39","3a","3b","3c","3d","3e","3f","40","41","42","43","44","45","46","47","48","49","4a","4b","4c","4d","4e","4f","50","51","52","53","54","55","56","57","58","59","5a","5b","5c","5d","5e","5f","60","61","62","63","64","65","66","67","68","69","6a","6b","6c","6d","6e","6f","70","71","72","73","74","75","76","77","78","79","7a","7b","7c","7d","7e","7f","80","81","82","83","84","85","86","87","88","89","8a","8b","8c","8d","8e","8f","90","91","92","93","94","95","96","97","98","99","9a","9b","9c","9d","9e","9f","a0","a1","a2","a3","a4","a5","a6","a7","a8","a9","aa","ab","ac","ad","ae","af","b0","b1","b2","b3","b4","b5","b6","b7","b8","b9","ba","bb","bc","bd","be","bf","c0","c1","c2","c3","c4","c5","c6","c7","c8","c9","ca","cb","cc","cd","ce","cf","d0","d1","d2","d3","d4","d5","d6","d7","d8","d9","da","db","dc","dd","de","df","e0","e1","e2","e3","e4","e5","e6","e7","e8","e9","ea","eb","ec","ed","ee","ef","f0","f1","f2","f3","f4","f5","f6","f7","f8","f9","fa","fb","fc","fd","fe","ff"
    };
    static bool skip = false;
    public static void Init() {
        Updater.Instance.ToString();
        try {
            TextAsset txt = Resources.Load<TextAsset>(IDX_INNER);
            BonValue bon = BonDocument.FromBonBytes(txt.bytes);
            rootInner = new PakRoot(bon);
#if !RELEASE
            skip = true;
#endif
            if(Debug.isDebugBuild) {
                skip = true;
            }
        } catch(Exception e) {
            Debug.Log("未能加载内部资源列表文件");
            Debug.Log(e);
            rootInner = new PakRoot();
        }
        root = rootInner;
    }
    private static PakRoot rootInner;
    private static PakRoot root;

    public static string IDX_INNER = "file";

#if UNITY_ANDROID
    public static string PAK_PATH = Application.persistentDataPath + "/paks/";
    public static string RES_URL = "http://gsdcdn.go2play.com/monpet/AssetBundle/" + Application.version + "/Android/";
#elif UNITY_IOS
    public static string PAK_PATH = Application.temporaryCachePath + "/paks/";
    public static string RES_URL = "http://gsdcdn.go2play.com/monpet/AssetBundle/" + Application.version + "/iOS/";
#else
    public static string PAK_PATH = Application.temporaryCachePath + "/paks/";
    public static string RES_URL = "file://" + Application.dataPath + "/../AssetBundle/StandaloneWindows/";
#endif
#if UNITY_ANDROID && !UNITY_EDITOR
    public static string RES_INNER = "assets/paks/";
#else
    public static string RES_INNER = Application.streamingAssetsPath + "/paks/";
#endif



    //#if UNITY_EDITOR || !UNITY_ANDROID
    //    public static string RES_URL_INNER = "file://" + Application.streamingAssetsPath + "/paks/";
    //#else
    //    public static string RES_URL_INNER = Application.streamingAssetsPath + "/paks/";
    //#endif

    private static Dictionary<string, AssetBundle> loadedBundle = new Dictionary<string, AssetBundle>();
    private static HashSet<string> loadingBundles = new HashSet<string>();
    private static HashSet<string> downloadingBundles = new HashSet<string>();

    private static bool CheckCache(PakInfo pak) {
        string fn = PakNameToFilename(pak.hash);
        return File.Exists(fn + ".done") && File.Exists(fn);
    }

    static string PakNameToFilename(string pn) {
        return new System.Text.StringBuilder(PAK_PATH).Append(pn[0]).Append(pn[1]).Append('/').Append(pn).ToString();
    }

    public static void UnloadAll() {
        foreach(AssetBundle ab in loadedBundle.Values.ToArray()) {
            ab.Unload(true);
            GameObject.Destroy(ab);
        }
        loadedBundle.Clear();
        System.GC.Collect();
    }

    public static void GC() {
        //foreach(AssetBundle ab in loadedBundle.Values) {
        //    ab.Unload(false);
        //    GameObject.Destroy(ab);
        //}
        //loadedBundle.Clear();
        System.GC.Collect(1);
    }

    static void DelFile(string fn) {
        if(File.Exists(fn)) {
            File.Delete(fn);
        }
    }

    /// <summary>
    /// 检查更新
    /// </summary>
    /// <returns></returns>
    public static LoadProgress<LoadInfo> CheckUpgradeAsync() {
        //int w, c;
        //ThreadPool.GetMaxThreads(out w, out c);
        //Debug.Log(w + "," + c);
        LoadProgress<LoadInfo> lp = new LoadProgress<LoadInfo>();
        Updater.Instance.proxy.StartCoroutine(_CheckUpgradeAsyns(lp));
        return lp;
    }

    private static IEnumerator _CheckUpgradeAsyns(LoadProgress<LoadInfo> lp) {
        yield return null;
        if(!Directory.Exists(PAK_PATH)) {
            Directory.CreateDirectory(PAK_PATH);
        }
        for(int i = PAK_PATH_SPLIT.Length; --i >= 0; ) {
            string d = PAK_PATH + PAK_PATH_SPLIT[i];
            if(!Directory.Exists(d)) {
                Directory.CreateDirectory(d);
            }
        }
        string pakPath = PAK_PATH;
        if(skip) {
            HashSet<string> allHash2 = new HashSet<string>(root.paks.Select(p => p.hash));
            ThreadPool.QueueUserWorkItem(o => {
                Thread.Sleep(5000);
                for(int i = PAK_PATH_SPLIT.Length; --i >= 0; ) {
                    string d = pakPath + PAK_PATH_SPLIT[i] + "/";
                    string[] fns = Directory.GetFiles(d);
                    for(int i2 = fns.Length; --i2 >= 0; ) {
                        string fn = fns[i2];
                        if(!allHash2.Contains(Path.GetFileNameWithoutExtension(fn))) {
                            Debug.Log("清除无效包：" + fn);
                            Res2.DelFile(fn);
                            Thread.Sleep(1);
                        }
                    }
                }
            });
            lp.obj = new LoadInfo();
            lp.SetDone();
            yield break;
        }
        BonValue bon = null;
        using(WWW www = new WWW(RES_URL + "file.bin?r=" + TimeUtil.UnixTimestampNow())) {
            while(!www.isDone) {
                lp.Progress = www.progress * 0.9f;
                yield return null;
            }
            if(www.error != null) {
                Debug.Log("下载更新列表出错" + www.error);
                lp.SetDone(www.error);
                yield break;
            }
            try {
                bon = BonDocument.FromBonBytes(Lzma.Decompress(www.bytes));
            } catch(Exception e) {
                Debug.Log("未能加载网络资源列表文件");
                Debug.Log(e);
                lp.SetDone("未能加载网络资源列表文件");
                yield break;
            }
        }
        root = new PakRoot(bon);
        List<string> paks = new List<string>(root.paks.Count * 5);
        for(int i = root.paks.Count; --i >= 0; ) {
            PakInfo pak = root.paks[i];
            if(pak.isStatic) {
                paks.Add(pak.name);
                paks.AddRange(pak.allDependences);
            }
        }
        paks = paks.Distinct().ToList();
        for(int i = paks.Count; --i >= 0; ) {
            string pkn = paks[i];
            PakInfo pak = root.paksMap[pkn];
            PakInfo old;
            if(rootInner.paksMap.TryGetValue(pkn, out old) && old.hash == pak.hash) {
                paks.RemoveAt(i);
            } else {
                if(CheckCache(pak)) {
                    paks.RemoveAt(i);
                }
            }
        }

        lp.Progress = 0.9f;
        yield return null;

        HashSet<string> allHash = new HashSet<string>(root.paks.Select(p => p.hash));
        ThreadPool.QueueUserWorkItem(o => {
            Thread.Sleep(5000);
            for(int i = PAK_PATH_SPLIT.Length; --i >= 0; ) {
                string d = pakPath + PAK_PATH_SPLIT[i] + "/";
                string[] fns = Directory.GetFiles(d);
                for(int i2 = fns.Length; --i2 >= 0; ) {
                    string fn = fns[i2];
                    if(!allHash.Contains(Path.GetFileNameWithoutExtension(fn))) {
                        Debug.Log("清除无效包：" + fn);
                        Res2.DelFile(fn);
                        Thread.Sleep(1);
                    }
                }
            }
        });

        LoadInfo up = new LoadInfo();
        up.paks = paks;
        up.totalSize = Mathf.Max(1, paks.Sum(p => root.paksMap[p].size));
        lp.obj = up;
        lp.Progress = 1;
        yield return null;
        lp.SetDone();
    }

    public static LoadInfo GetDecompressInnerPaksInfo() {
        LoadInfo info = new LoadInfo();
        for(int i = rootInner.paks.Count; --i >= 0; ) {
            PakInfo pak = rootInner.paks[i];
            PakInfo pak2;
            if(root.paksMap.TryGetValue(pak.name, out pak2) && pak2.hash == pak.hash && (!CheckCache(pak)

                //|| false
                )) {
                info.paks.Add(pak.name);
                info.totalSize += pak.size;
            }
        }
        return info;
    }

    public static LoadProgress DecompressInnerPaks(LoadInfo info) {
        LoadProgress lp = new LoadProgress();
        Updater.Instance.proxy.StartCoroutine(_DecompressInnerPaks(lp, info));
        return lp;
    }

    private static IEnumerator _DecompressInnerPaks(LoadProgress lp, LoadInfo info) {
        yield return null;
        if(info.paks.Count <= 0) {
            lp.SetDone();
            yield break;
        }
        string dataPath = Application.dataPath;
        int size = 0;
        int totalSize = info.totalSize;
        int leftNum = info.paks.Count;
        SafedQueue<string> pns = new SafedQueue<string>(info.paks);
        int maxThreadNum = Mathf.Min(leftNum, Environment.ProcessorCount);
        byte[][] buffs = new byte[maxThreadNum][];
        for(int i = maxThreadNum; --i >= 0; ) {
            buffs[i] = new byte[1024 * 1024];
        }
        float[] progresses = new float[maxThreadNum];
        bool error = false;
        Updater updater = Updater.Instance;
        WaitCallback worker = obj => {
            int id = (int)obj;
            while(!error) {
                string pn;
                if(!pns.TryDequeue(out pn)) {
                    break;
                }
                progresses[id] = 0;
                PakInfo pak = rootInner.paksMap[pn];
                string fn = PakNameToFilename(pak.hash);
                float tp = (float)pak.size / totalSize; //这个文件占用百分比

                try {
#if UNITY_ANDROID && !UNITY_EDITOR
                    using(ZipFileInputStream fs0 = new ZipFileInputStream(dataPath, new System.Text.StringBuilder(RES_INNER).Append(pak.hash[0]).Append(pak.hash[1]).Append('/').Append(pak.hash).ToString())) {
#else
                    string fn0 = new System.Text.StringBuilder(RES_INNER).Append(pak.hash[0]).Append(pak.hash[1]).Append('/').Append(pak.hash).ToString();
                    using(FileStream fs0 = new FileStream(fn0, FileMode.Open, FileAccess.Read)) {
#endif
                        bool maybeLzma = fs0.Length > 13;//lzma的头长度至少13
                        bool isLzma = false;
                        byte[] head = null;
                        if(maybeLzma) {
                            head = new byte[13];
                            fs0.Read(head, 0, head.Length);
                            isLzma = head[0] == 0x5d;
                        }
                        if(isLzma) {
                            try {
                                using(LzmaInputStream lzma = new LzmaInputStream(head, fs0)) {
                                    using(FileStream fs = new FileStream(fn, FileMode.Create, FileAccess.Write)) {
                                        byte[] buff = buffs[id];
                                        while(lzma.Position < lzma.Length) {
                                            int len = lzma.Read(buff, 0, Mathf.Min(buff.Length, (int)(lzma.Length - lzma.Position)));
                                            fs.Write(buff, 0, len);
                                            float p = lzma.Length == 0 ? 1 : (float)lzma.Position / lzma.Length;
                                            progresses[id] = tp * p;
                                        }
                                    }
                                }
                            } catch(Exception e) {
                                Debug.LogException(e);
                                Res2.DelFile(fn + ".done");
                                Res2.DelFile(fn);
                                Debug.LogError("_DecompressInnerPaks解压展开AB:" + pak.name + "失败\n");
                                error = true;
                                break;
                            }
                        } else {
                            try {
                                using(FileStream fs = new FileStream(fn, FileMode.Create, FileAccess.Write)) {
                                    if(maybeLzma) {
                                        fs.Write(head, 0, head.Length);
                                    }
                                    byte[] buff = buffs[id];
                                    while(fs0.Position < fs0.Length) {
                                        int len = fs0.Read(buff, 0, Mathf.Min(buff.Length, (int)(fs0.Length - fs0.Position)));
                                        fs.Write(buff, 0, len);
                                        float p = fs0.Length == 0 ? 1 : (float)fs0.Position / fs0.Length;
                                        progresses[id] = tp * p;
                                    }
                                }
                            } catch(Exception e) {
                                Debug.LogException(e);
                                Res2.DelFile(fn + ".done");
                                Res2.DelFile(fn);
                                Debug.LogError("_DecompressInnerPaks拷贝展开AB:" + pak.name + "失败\n");
                                error = true;
                                break;
                            }
                        }
                    }
                } catch(Exception e) {
                    Debug.LogException(e);
                    Res2.DelFile(fn + ".done");
                    Res2.DelFile(fn);
                    Debug.LogError("_DecompressInnerPaks展开AB:" + pak.name + "失败\n");
                    error = true;
                    break;
                }

                File.WriteAllBytes(fn + ".done", new byte[0]);
                lock(progresses) {
                    size += pak.size;
                    progresses[id] = 0;
                }
                Interlocked.Decrement(ref leftNum);
            }
        };

        for(int i = maxThreadNum; --i >= 0; ) {
            ThreadPool.QueueUserWorkItem(worker, i);
        }

        while(!error && leftNum > 0) {
            float p = 0;
            lock(progresses) {
                p = progresses.Sum();
                p += (float)size / totalSize;
            }
            lp.Progress = p;
            yield return new WaitForSeconds(0.1f);
        }

        if(error) {
            lp.SetDone("error");
            yield break;
        }

        lp.Progress = 1;
        yield return null;
        lp.SetDone();
    }

    public static LoadProgress UpgradeAsync(LoadInfo upInfo) {
        LoadProgress lp = new LoadProgress();
        Updater.Instance.proxy.StartCoroutine(_UpgradeAsync(upInfo, lp));
        return lp;
    }

    struct DownloadPak {
        public PakInfo pak;
        public byte[] data;

        public DownloadPak(PakInfo pak, byte[] data) {
            this.pak = pak;
            this.data = data;
        }
    }
    private static IEnumerator _UpgradeAsync(LoadInfo upInfo, LoadProgress lp) {
        yield return null;
        if(upInfo.paks.Count <= 0) {
            lp.SetDone();
            yield break;
        }
        int size = 0;
        int totalSize = upInfo.totalSize;
        int leftNum = upInfo.paks.Count;
        bool error = false;
        WaitCallback dec = obj => {
            DownloadPak dp = (DownloadPak)obj;
            PakInfo pak = dp.pak;
            string fn = PakNameToFilename(pak.hash);
            byte[] data = dp.data;
            if(data.Length > 13 && data[0] == 0x5d) {
                try {
                    using(LzmaInputStream lzma = new LzmaInputStream(data)) {
                        using(FileStream fs = new FileStream(fn, FileMode.Create)) {
                            byte[] buff = new byte[Mathf.Min(1024 * 1024, (int)lzma.Length)];
                            while(lzma.Position < lzma.Length) {
                                int len = lzma.Read(buff, 0, Mathf.Min((int)(lzma.Length - lzma.Position), buff.Length));
                                fs.Write(buff, 0, len);
                            }
                        }
                    }
                } catch(Exception e) {
                    Debug.LogException(e);
                    Res2.DelFile(fn + ".done");
                    Res2.DelFile(fn);
                    Debug.LogError("_UpgradeAsync解压展开AB:" + pak.name + "失败\n");
                    error = true;
                    return;
                }
            } else {
                try {
                    using(FileStream fs = new FileStream(fn, FileMode.Create)) {
                        int idx = 0;
                        while(idx < data.Length) {
                            int len = Mathf.Min(data.Length - idx, 1024 * 1024);
                            fs.Write(data, idx, len);
                            idx += len;
                        }
                    }
                } catch(Exception e) {
                    Debug.LogException(e);
                    Res2.DelFile(fn + ".done");
                    Res2.DelFile(fn);
                    Debug.LogError("_UpgradeAsync拷贝展开AB:" + pak.name + "失败\n");
                    error = true;
                    return;
                }
            }
            File.WriteAllBytes(fn + ".done", new byte[0]);
            Interlocked.Decrement(ref leftNum);
        };

        float p = 0;
        for(int i = upInfo.paks.Count; --i >= 0; ) {
            PakInfo pak = root.paksMap[upInfo.paks[i]];
            string url = RES_URL + root.ver + "/" + pak.name + "?r=" + TimeUtil.UnixTimestampNow();
            float tp = (float)pak.size / totalSize; //这个文件占用百分比
            int retry = 1;
            string errMsg = null;
            while(retry >= 0) {
                retry--;
                using(WWW www = new WWW(url)) {
                    while(!www.isDone) {
                        yield return null;
                        lp.Progress = p + tp * www.progress;
                    }
                    errMsg = www.error;
                    if(errMsg != null) {
                        lp.Progress = p;
                        continue;
                    }
                    ThreadPool.QueueUserWorkItem(dec, new DownloadPak(pak, www.bytes));
                }
                break;
            }
            if(errMsg != null) {
                lp.SetDone(errMsg);
                yield break;
            }
            size += pak.size;
            p = (float)size / totalSize;
            lp.Progress = p;
        }

        while(!error && leftNum > 0) {
            yield return new WaitForSeconds(0.1f);
        }

        if(error) {
            lp.SetDone("error");
            yield break;
        }

        lp.Progress = 1;
        yield return null;
        lp.SetDone();
    }

    //--------------------------------------------------------------------------------------------------------------------------------
    const string RES_PATH = "Assets/Res/{0}";
    private static AssetBundle InitAssetBundle(string abname) {
        List<PakInfo> paks = GetPaksNeedInit(abname);
        AssetBundle ab = null;
        if(paks.Count == 0) {
            loadedBundle.TryGetValue(abname, out ab);
            return ab;
        }
        if(!root.paksMap[abname].isStatic) {
            Debug.LogError(abname + "是动态包，不能使用同步加载");
            return null;
        }
        for(int i = paks.Count; --i >= 0; ) {
            PakInfo pak = paks[i];
            string fn = PakNameToFilename(pak.hash);
            ab = AssetBundle.LoadFromFile(fn);
            if(ab == null) {
                Res2.DelFile(fn + ".done");
                Res2.DelFile(fn);
                Debug.LogError("InitAssetBundle加载AB:" + pak.name + "失败");
                return null;
            }
            loadedBundle[pak.name] = ab;
        }
        loadedBundle.TryGetValue(abname, out ab);
        return ab;
    }

    public static T Load<T>(string resname) where T: UnityEngine.Object {
        Type t = typeof(T);
        if(t == typeof(Sprite)) {
            return _LoadSprite(resname) as T;
        }
        if(typeof(MonoBehaviour).IsAssignableFrom(t)) {
            GameObject go = _Load<GameObject>(resname);
            if(go == null) {
                return null;
            }
            return go.GetComponent<T>();
        }
        return _Load<T>(resname);
    }

    private static T _Load<T>(string resname) where T: UnityEngine.Object {
#if UNITY_EDITOR && DEBUG_RES
        string path2 = string.Format(RES_PATH, resname).ToLower();
        if(File.Exists(path2)) {
            return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path2);
        }
        return default(T);
#else
        float time = Time.realtimeSinceStartup;
        string path2 = string.Format(RES_PATH, resname).ToLower();
        PakInfo pak;
        if(root.res2pak.TryGetValue(path2, out pak)) {
            time = Time.realtimeSinceStartup;
            AssetBundle ab = InitAssetBundle(pak.name);
            //Debug.Log("计算包: " + (Time.realtimeSinceStartup - time));
            if(ab == null) {
                return null;
            }
            time = Time.realtimeSinceStartup;
            T obj = ab.LoadAsset<T>(path2);
            //Debug.Log("从包中加载资源: " + (Time.realtimeSinceStartup - time));
            return obj;
        }
        return null;
#endif
    }

    private static Sprite _LoadSprite(string resname) {
        int idx = resname.LastIndexOf(".");
        if(idx < 0) {
            return null;
        }
        string path = resname.Substring(0, idx);
        string sprtName = resname.Substring(idx + 1);
        string path2 = string.Format(RES_PATH, path).ToLower();
#if UNITY_EDITOR && DEBUG_RES
        if(File.Exists(path2)) {
            var objs = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(path2);
            foreach(var obj in objs.OfType<Sprite>()) {
                if(obj.name == sprtName) {
                    return obj;
                }
            }
        }
        return null;
#else
        PakInfo pak;
        if(root.res2pak.TryGetValue(path2, out pak)) {
            AssetBundle ab = InitAssetBundle(pak.name);
            if(ab == null) {
                return null;
            }
            Sprite[] all = ab.LoadAssetWithSubAssets<Sprite>(path2);
            for(int i = all.Length; --i >= 0;) {
                Sprite obj = all[i];
                if(obj.name == sprtName) {
                    return obj;
                }
            }
        }
        return null;
#endif
    }

    public static bool LoadLevel(string scene, bool additive = false, bool unloadRes = false) {
        string path2 = string.Format(RES_PATH, scene).ToLower();
#if UNITY_EDITOR && DEBUG_RES
        if(UnityEditor.AssetDatabase.AssetPathToGUID(path2) == null)
            return false;
        if(additive) {
            UnityEditor.EditorApplication.LoadLevelAdditiveInPlayMode(path2);
        } else {
            UnityEditor.EditorApplication.LoadLevelInPlayMode(path2);
        }
#else
        PakInfo pak;
        if(root.res2pak.TryGetValue(path2, out pak)) {
            if(unloadRes) {
                List<string> needRemove = new List<string>();
                foreach(var abn in loadedBundle.Keys) {
                    if(pak.allDependences.Contains(abn) || abn == pak.name) {
                    } else {
                        loadedBundle[abn].Unload(false);
                        needRemove.Add(abn);
                    }
                }
                for(int i = needRemove.Count; --i >= 0;) {
                    loadedBundle.Remove(needRemove[i]);
                }
            }
            AssetBundle ab = InitAssetBundle(pak.name);
            object o = ab.GetAllScenePaths();
        } else {
            return false;
        }
        if(additive) {
            SceneManager.LoadScene(Path.GetFileNameWithoutExtension(scene), LoadSceneMode.Additive);
        } else {
            SceneManager.LoadScene(Path.GetFileNameWithoutExtension(scene), LoadSceneMode.Single);
        }
#endif
        return true;
    }

    //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------
    private static List<PakInfo> GetPaksNeedInit(string abname) {
        PakInfo pak0 = root.paksMap[abname];
        List<PakInfo> paks = new List<PakInfo>(pak0.allDependences.Count + 1);
        for(int i = pak0.allDependences.Count; --i >= 0; ) {
            string abn = pak0.allDependences[i];
            if(loadedBundle.ContainsKey(abn)) {
                continue;
            }
            paks.Add(root.paksMap[abn]);
        }
        if(!loadedBundle.ContainsKey(abname)) {
            paks.Add(pak0);
        }
        return paks;
    }

    private static LoadProgress<AssetBundle> InitAssetBundleAsync(string abname) {
        LoadProgress<AssetBundle> lp = new LoadProgress<AssetBundle>();
        Updater.Instance.proxy.StartCoroutine(_InitAssetBundleAsync(abname, lp));
        return lp;
    }

    private static IEnumerator _InitAssetBundleAsync(string abname, LoadProgress<AssetBundle> lp) {
        yield return null;
        List<PakInfo> paks = GetPaksNeedInit(abname);
        AssetBundle ab = null;
        if(paks.Count == 0) {
            loadedBundle.TryGetValue(abname, out ab);
            lp.obj = ab;
            lp.Progress = 1;
            yield return null;
            lp.SetDone();
            yield break;
        }
        int allTotalSize = paks.Sum(pk => pk.size);
        float p = 0;
        PakInfo[] dlPaks = paks.Where(pk => !CheckCache(pk)).ToArray();
        if(dlPaks.Length > 0) {
            int size = 0;
            int totalSize = dlPaks.Sum(pk => pk.size);
            float dlp = 0.8f * totalSize / allTotalSize;//下载占总共的百分比
            int leftNum = dlPaks.Length;
            bool error = false;
            WaitCallback dec = obj => {
                DownloadPak dp = (DownloadPak)obj;
                PakInfo pak = dp.pak;
                string fn = PakNameToFilename(pak.hash);
                byte[] data = dp.data;
                if(data.Length > 13 && data[0] == 0x5d) {
                    try {
                        using(LzmaInputStream lzma = new LzmaInputStream(data)) {
                            using(FileStream fs = new FileStream(fn, FileMode.Create)) {
                                byte[] buff = new byte[Mathf.Min(1024 * 1024, (int)lzma.Length)];
                                while(lzma.Position < lzma.Length) {
                                    int len = lzma.Read(buff, 0, Mathf.Min((int)(lzma.Length - lzma.Position), buff.Length));
                                    fs.Write(buff, 0, len);
                                }
                            }
                        }
                    } catch(Exception e) {
                        Debug.LogException(e);
                        Res2.DelFile(fn + ".done");
                        Res2.DelFile(fn);
                        Debug.LogError("_UpgradeAsync解压展开AB:" + pak.name + "失败\n");
                        error = true;
                        lock(downloadingBundles) {
                            downloadingBundles.Remove(pak.name);
                        }
                        return;
                    }
                } else {
                    try {
                        using(FileStream fs = new FileStream(fn, FileMode.Create)) {
                            int idx = 0;
                            while(idx < data.Length) {
                                int len = Mathf.Min(data.Length - idx, 1024 * 1024);
                                fs.Write(data, idx, len);
                                idx += len;
                            }
                        }
                    } catch(Exception e) {
                        Debug.LogException(e);
                        Res2.DelFile(fn + ".done");
                        Res2.DelFile(fn);
                        Debug.LogError("_UpgradeAsync拷贝展开AB:" + pak.name + "失败\n");
                        error = true;
                        lock(downloadingBundles) {
                            downloadingBundles.Remove(pak.name);
                        }
                        return;
                    }
                }
                File.WriteAllBytes(fn + ".done", new byte[0]);
                lock(downloadingBundles) {
                    downloadingBundles.Remove(pak.name);
                }
                Interlocked.Decrement(ref leftNum);
            };

            for(int i = dlPaks.Length; --i >= 0; ) {
                PakInfo pak = dlPaks[i];
                while(true) {
                    lock(downloadingBundles) {
                        if(!downloadingBundles.Contains(pak.name)) {
                            downloadingBundles.Add(pak.name);
                            break;
                        }
                    }
                    yield return new WaitForSeconds(0.1f);
                }
                if(CheckCache(pak)) {
                    lock(downloadingBundles) {
                        downloadingBundles.Remove(pak.name);
                    }
                    size += pak.size;
                    p = dlp * size / totalSize;
                    lp.Progress = p;
                    Interlocked.Decrement(ref leftNum);
                    continue;
                }
                string url = RES_URL + root.ver + "/" + pak.name + "?r=" + TimeUtil.UnixTimestampNow();
                float tp = (float)pak.size / totalSize; //这个文件占用百分比
                int retry = 1;
                string errMsg = null;
                while(retry >= 0) {
                    retry--;
                    using(WWW www = new WWW(url)) {
                        while(!www.isDone) {
                            yield return null;
                            lp.Progress = p + dlp * tp * www.progress;
                        }
                        errMsg = www.error;
                        if(errMsg != null) {
                            lp.Progress = p;
                            continue;
                        }
                        ThreadPool.QueueUserWorkItem(dec, new DownloadPak(pak, www.bytes));
                    }
                    break;
                }
                if(errMsg != null) {
                    lock(downloadingBundles) {
                        downloadingBundles.Remove(pak.name);
                    }
                    lp.SetDone(errMsg);
                    yield break;
                }
                size += pak.size;
                p = dlp * size / totalSize;
                lp.Progress = p;
            }

            while(!error && leftNum > 0) {
                yield return new WaitForSeconds(0.1f);
            }

            if(error) {
                lp.SetDone("error");
                yield break;
            }
        }

        {
            float p0 = p;
            float ldp = 1 - p;
            int size = 0;
            int totalSize = allTotalSize;
            int num = paks.Count;
            for(int i = 0; i < num; i++) {
                PakInfo pak = paks[i];
                while(true) {
                    lock(loadingBundles) {
                        if(!loadingBundles.Contains(pak.name)) {
                            loadingBundles.Add(pak.name);
                            break;
                        }
                    }
                    yield return new WaitForSeconds(0.1f);
                }
                if(loadedBundle.ContainsKey(pak.name)) {
                    lock(loadingBundles) {
                        loadingBundles.Remove(pak.name);
                    }
                    size += pak.size;
                    p = p0 + ldp * size / totalSize;
                    lp.Progress = p;
                    continue;
                }
                string fn = PakNameToFilename(pak.hash);
                float tp = (float)pak.size / totalSize; //这个文件占用百分比
                var async = AssetBundle.LoadFromFileAsync(fn);
                while(!async.isDone) {
                    lp.Progress = p + ldp * tp * async.progress;
                    yield return null;
                }
                ab = async.assetBundle;
                if(ab == null) {
                    lock(loadingBundles) {
                        loadingBundles.Remove(pak.name);
                    }
                    Res2.DelFile(fn + ".done");
                    Res2.DelFile(fn);
                    Debug.LogError("_InitAssetBundleAsync加载AB:" + pak.name + "失败。" + fn);
                    lp.SetDone("error");
                    yield break;
                }
                size += pak.size;
                p = p0 + ldp * size / totalSize;
                lp.Progress = p;
                loadedBundle[pak.name] = ab;
                lock(loadingBundles) {
                    loadingBundles.Remove(pak.name);
                }
            }
        }

        loadedBundle.TryGetValue(abname, out ab);
        lp.obj = ab;
        lp.Progress = 1;
        yield return null;
        lp.SetDone();
    }


    public static LoadProgress<T> LoadAsync<T>(string resname) where T: UnityEngine.Object {
        Type t = typeof(T);
        if(t == typeof(Sprite)) {
            return _LoadSpriteAsync(resname) as LoadProgress<T>;
        } else {
            return _LoadAsync<T>(resname);
        }
    }

    private static LoadProgress<T> _LoadAsync<T>(string resname) where T: UnityEngine.Object {
        LoadProgress<T> lp = new LoadProgress<T>();
        Updater.Instance.proxy.StartCoroutine(_LoadAsync<T>(resname, lp));
        return lp;
    }

    private static IEnumerator _LoadAsync<T>(string resname, LoadProgress<T> lp) where T: UnityEngine.Object {
        yield return null;
#if UNITY_EDITOR && DEBUG_RES
        string path2 = string.Format(RES_PATH, resname).ToLower();
        if(File.Exists(path2)) {
            if(typeof(MonoBehaviour).IsAssignableFrom(typeof(T))) {
                GameObject go = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path2);
                if(go == null) {
                    lp.obj = null;
                } else {
                    lp.obj = go.GetComponent<T>();
                }
            } else {
                lp.obj = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path2);
            }
            lp.Progress = 1;
            yield return null;
            lp.SetDone();
            yield break;
        }
        lp.SetDone("未找到资源" + resname);
#else
        string path2 = string.Format(RES_PATH, resname).ToLower();
        PakInfo pak;
        if(root.res2pak.TryGetValue(path2, out pak)) {
            LoadProgress<AssetBundle> lp2 = InitAssetBundleAsync(pak.name);
            while(!lp2.IsDone) {
                lp.Progress = lp2.Progress * 0.9f;
                yield return null;
            }
            if(lp2.IsError) {
                lp.SetDone(lp2.Error);
                yield break;
            }
            AssetBundle ab = lp2.obj;
            if(ab == null) {
                lp.SetDone();
                yield break;
            }
            AssetBundleRequest abr = null;
            bool isMonoBehaviour = typeof(MonoBehaviour).IsAssignableFrom(typeof(T));
            if(isMonoBehaviour) {
                abr = ab.LoadAssetAsync<GameObject>(path2);
            } else {
                abr = ab.LoadAssetAsync<T>(path2);
            }
            while(!abr.isDone) {
                lp.Progress = 0.9f + 0.1f * abr.progress;
                yield return null;
            }
            if(isMonoBehaviour) {
                GameObject go = abr.asset as GameObject;
                if(go == null) {
                    lp.obj = null;
                } else {
                    lp.obj = go.GetComponent<T>();
                }
            } else {
                lp.obj = abr.asset as T;
            }
            lp.Progress = 1;
            yield return null;
            lp.SetDone();
            yield break;
        }
        lp.SetDone("未找到资源" + resname);
#endif
    }

    private static LoadProgress<Sprite> _LoadSpriteAsync(string resname) {
        LoadProgress<Sprite> lp = new LoadProgress<Sprite>();
        Updater.Instance.proxy.StartCoroutine(_LoadSpriteAsync(resname, lp));
        return lp;
    }

    public static IEnumerator _LoadSpriteAsync(string resname, LoadProgress<Sprite> lp) {
        yield return null;
        int idx = resname.LastIndexOf(".");
        if(idx < 0) {
            lp.SetDone("未找到资源" + resname);
            yield break;
        }
        string path = resname.Substring(0, idx);
        string sprtName = resname.Substring(idx + 1);
        string path2 = string.Format(RES_PATH, path).ToLower();
#if UNITY_EDITOR && DEBUG_RES
        if(File.Exists(path2)) {
            var objs = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(path2);
            foreach(var obj in objs.OfType<Sprite>()) {
                if(obj.name == sprtName) {
                    lp.obj = obj as Sprite;
                    break;
                }
            }
            lp.Progress = 1;
            yield return null;
            lp.SetDone();
            yield break;
        }
        lp.SetDone("未找到资源" + resname);
#else
        PakInfo pak;
        if(root.res2pak.TryGetValue(path2, out pak)) {
            LoadProgress<AssetBundle> lp2 = InitAssetBundleAsync(pak.name);
            while(!lp2.IsDone) {
                lp.Progress = lp2.Progress * 0.9f;
                yield return null;
            }
            if(lp2.IsError) {
                lp.SetDone(lp2.Error);
                yield break;
            }
            AssetBundle ab = lp2.obj;
            if(ab == null) {
                lp.SetDone();
                yield break;
            }
            AssetBundleRequest abr = ab.LoadAssetWithSubAssetsAsync<Sprite>(path2);
            while(!abr.isDone) {
                lp.Progress = 0.9f + 0.1f * abr.progress;
                yield return null;
            }
            for(int i = abr.allAssets.Length;--i>=0;) {
                var obj = abr.allAssets[i];
                if(obj.name == sprtName) {
                    lp.obj = obj as Sprite;
                    break;
                }
            }
            lp.Progress = 1;
            yield return null;
            lp.SetDone();
            yield break;
        }
        lp.SetDone("未找到资源" + resname);
#endif
    }

    /// <summary>
    /// 获取加载场景需要下载的信息
    /// </summary>
    /// <returns></returns>
    public static LoadInfo GetLoadLevelInfo(string scene) {
        LoadInfo up = new LoadInfo();
#if UNITY_EDITOR && DEBUG_RES

#else
        string path2 = string.Format(RES_PATH, scene).ToLower();
        List<string> paks = new List<string>();
        PakInfo pak;
        if(root.res2pak.TryGetValue(path2, out pak)) {
            paks.Capacity = pak.allDependences.Count + 1;
            paks.Add(pak.name);
            paks.AddRange(pak.allDependences);
            for(int i = paks.Count; --i >= 0;) {
                string pkn = paks[i];
                pak = root.paksMap[pkn];
                PakInfo old;
                if(rootInner.paksMap.TryGetValue(pkn, out old) && old.hash == pak.hash) {
                    paks.RemoveAt(i);
                } else {
                    string url = RES_URL + root.ver + "/" + pkn;
                    Hash128 hash = Hash128.Parse(pak.hash);
                    if(Caching.IsVersionCached(url, hash)) {
                        paks.RemoveAt(i);
                    }
                }
            }
        }
        up.paks = paks;
        up.totalSize = Mathf.Max(1, paks.Sum(p => root.paksMap[p].size));
#endif
        return up;
    }

    public static bool loadingLevel = false;
    public static LoadProgress<AsyncOperation> LoadLevelAsync(string scene, bool additive = false, bool autoActive = false, bool unloadRes = false) {
        LoadProgress<AsyncOperation> lp = new LoadProgress<AsyncOperation>();
        if(loadingLevel) {
            Debug.LogError("不允许同时异步加载多个场景" + scene);
            lp.SetDone("error");
            return lp;
        }
        loadingLevel = true;
        Updater.Instance.proxy.StartCoroutine(_LoadLevelAsync(scene, additive, autoActive, unloadRes, lp));
        return lp;
    }

    private static IEnumerator _LoadLevelAsync(string scene, bool additive, bool autoActive, bool unloadRes, LoadProgress<AsyncOperation> lp) {
        yield return null;
        string path2 = string.Format(RES_PATH, scene).ToLower();
#if UNITY_EDITOR && DEBUG_RES
        if(scene != null && scene.StartsWith("Assets/test")) {//这个是给亮子切测试关卡用的，不要删
            path2 = scene;
        }
        if(!File.Exists(path2)) {
            loadingLevel = false;
            lp.SetDone("场景不存在");
            yield break;
        }
        AsyncOperation ao = null;
        if(additive) {
            ao = UnityEditor.EditorApplication.LoadLevelAdditiveAsyncInPlayMode(path2);
        } else {
            ao = UnityEditor.EditorApplication.LoadLevelAsyncInPlayMode(path2);
        }
        if(!autoActive) {
            ao.allowSceneActivation = false;
        }
        while(!ao.isDone && ao.progress < 0.9f) {
            lp.Progress = ao.progress;
            yield return null;
        }
        lp.obj = ao;
        lp.Progress = 1;
        yield return null;
        loadingLevel = false;
        lp.SetDone();
#else
        LoadProgress<AssetBundle> lp2 = null;
        PakInfo pak;
        if(root.res2pak.TryGetValue(path2, out pak)) {
            if(unloadRes) {
                List<string> needRemove = new List<string>();
                foreach(var abn in loadedBundle.Keys) {
                    if(pak.allDependences.Contains(abn) || abn == pak.name) {
                    } else {
                        loadedBundle[abn].Unload(false);
                        needRemove.Add(abn);
                    }
                }
                for(int i = needRemove.Count; --i >= 0;) {
                    loadedBundle.Remove(needRemove[i]);
                }
            }
            lp2 = InitAssetBundleAsync(pak.name);
            while(!lp2.IsDone) {
                lp.Progress = lp2.Progress * 0.8f;
                yield return null;
            }
            if(lp2.IsError) {
                loadingLevel = false;
                lp.SetDone(lp2.Error);
                yield break;
            }
        }
        if(lp2 == null || lp2.obj == null) {
            loadingLevel = false;
            lp.SetDone("场景不存在");
            yield break;
        }
        AsyncOperation ao = null;
        if(additive) {
            ao = SceneManager.LoadSceneAsync(Path.GetFileNameWithoutExtension(scene), LoadSceneMode.Additive);
        } else {
            ao = SceneManager.LoadSceneAsync(Path.GetFileNameWithoutExtension(scene), LoadSceneMode.Single);
        }
        if(!autoActive) {
            ao.allowSceneActivation = false;
        }
        while(!ao.isDone && ao.progress < 0.9f) {
            lp.Progress = 0.8f + ao.progress * 0.2f;
            yield return null;
        }
        lp.obj = ao;
        lp.Progress = 1;
        yield return null;
        loadingLevel = false;
        lp.SetDone();
#endif
    }
}
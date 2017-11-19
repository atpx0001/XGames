using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System;

public class FileDownloader {
    static FileDownloader downloader;
    static Dictionary<string, WeakReference> textures = new Dictionary<string, WeakReference>();
    /// <summary>
    /// 下载文件
    /// </summary>
    /// <param name="url"></param>
    /// <param name="type"></param>
    /// <param name="callback"></param>
    /// <param name="cachePath">缓存到本地的相对目录，如果为null则不缓存</param>
    /// <param name="retry"></param>
    public static void Download(string url, FileDownloadType type, FileDownloadCallback callback, string cachePath = null, int retry = 0) {
        if(downloader == null) {
            downloader = new FileDownloader();
            Updater.Instance.OnPreUpdate.Add(downloader.OnUpdate);
            Updater.Instance.OnPostDestroy.Add(downloader.OnDestroy);
        }
        FileDownloadContext ctx = new FileDownloadContext();
        ctx.url = url;
        ctx.type = type;
        ctx.callback = callback;
        ctx.retry = retry;
        ctx.cachePath = cachePath;
        if(downloader.CheckCache(ctx)) {
            if(ctx.callback != null) {
                ctx.callback(ctx);
            }
            return;
        }
        downloader.sendQueue.Enqueue(ctx);
    }

    public static void DeleteCache(string url, string cachePath) {
        string path = GetCacheFilename(url, cachePath);
        if(File.Exists(path)) {
            File.Delete(path);
        }
    }

    public static void ClearCache(string cachePath) {
        if(Directory.Exists(cachePath)) {
            Directory.Delete(cachePath, true);
        }
    }

    static string GetCacheFilename(string url, string cachePath) {
        string cacheFn = string.Format("{0}/{1}/{2}", Application.temporaryCachePath, cachePath, MD5.CalcMD5(url));
        return cacheFn;
    }

    Queue<FileDownloadContext> sendQueue = new Queue<FileDownloadContext>();
    bool busy = false;

    void OnDestroy() {
        Updater.Instance.OnPostUpdate.Remove(downloader.OnUpdate);
        Updater.Instance.OnPreDestroy.Remove(downloader.OnDestroy);
    }
    void OnUpdate() {
        if(!busy && sendQueue.Count > 0) {
            Updater.Instance.proxy.StartCoroutine(_Send());
        }
    }

    bool CheckCache(FileDownloadContext ctx) {
        switch(ctx.type) {
            case FileDownloadType.Texture: {
                WeakReference wr;
                if(textures.TryGetValue(ctx.url, out wr) && wr.IsAlive) {
                    ctx.texture = wr.Target as Texture2D;
                    return true;
                }
                break;
            }
        }
        return false;
    }

    IEnumerator _Send() {
        busy = true;
        FileDownloadContext ctx = sendQueue.Dequeue();

        string url = ctx.url;
        Debug.Log("下载: " + url);
        if(CheckCache(ctx)) {
            if(ctx.callback != null) {
                ctx.callback(ctx);
            }
            busy = false;
            yield break;
        }
        string cacheFn = null;
        if(ctx.cachePath != null) {
            cacheFn = GetCacheFilename(url, cacheFn);
            using(WWW www = new WWW("file://" + WWW.EscapeURL(cacheFn, Encoding.Default))) {
                yield return www;
                if(www.error != null) {
                } else {
                    switch(ctx.type) {
                        case FileDownloadType.Texture: {
                            ctx.texture = www.textureNonReadable;
                            textures[ctx.url] = new WeakReference(ctx.texture);
                            break;
                        }
                        case FileDownloadType.Binary: {
                            ctx.bytes = www.bytes;
                            break;
                        }
                    }
                    if(ctx.callback != null) {
                        ctx.callback(ctx);
                    }
                    busy = false;
                    yield break;
                }
            }
        }
        while(ctx.retry >= 0) {
            using(WWW www = new WWW(url)) {
                yield return www;
                if(www.error != null) {
                    ctx.error = www.error;
                    Debug.LogError("下载: " + url + " Err:" + ctx.error);
                } else {
                    bool save = false;
                    switch(ctx.type) {
                        case FileDownloadType.Texture: {
                            ctx.texture = www.texture;
                            textures[ctx.url] = new WeakReference(ctx.texture);
                            save = ctx.texture != null;
                            break;
                        }
                        case FileDownloadType.Binary: {
                            ctx.bytes = www.bytes;
                            save = ctx.bytes != null;
                            break;
                        }
                    }
                    if(save && cacheFn != null) {
                        string dir = Path.GetDirectoryName(cacheFn);
                        if(!Directory.Exists(dir)) {
                            Directory.CreateDirectory(dir);
                        }
                        File.WriteAllBytes(cacheFn, www.bytes);
                    }
                }
            }
            ctx.retry--;
        }
        if(ctx.callback != null) {
            ctx.callback(ctx);
        }
        busy = false;
    }
}

public class FileDownloadContext {
    public string url;
    public string cachePath;
    public FileDownloadType type;
    public int retry;
    public string error;
    public byte[] bytes;
    public Texture2D texture;
    public FileDownloadCallback callback;
}

public enum FileDownloadType {
    Binary = 0,
    Texture = 1,
}
public delegate void FileDownloadCallback(FileDownloadContext ctx);


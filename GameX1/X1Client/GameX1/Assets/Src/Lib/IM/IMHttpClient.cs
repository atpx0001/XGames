#define HTTP_DEBUG_LOG1
using go2play.lib.im;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;


public class IMHttpClient {
    public event Action<IMHttpContext> OnResult;

    Queue<IMHttpContext> sendQueue = new Queue<IMHttpContext>(10);

    private Action updater;
    private bool destroyed = false;
    public string url;
    bool busy = false;
    static readonly Dictionary<string, string> header = new Dictionary<string, string>() {
        {"Content-Type", "application/octet-stream" }
    };

    public IMHttpClient(string url) {
        this.url = url;
        updater = Update;
        Updater.Instance.OnPreUpdate.Add(updater);
    }

    public void Destroy() {
        destroyed = true;
        Updater.Instance.OnPreUpdate.Remove(updater);
    }

    public void Update() {
        if(!busy && sendQueue.Count > 0) {
            busy = true;
            Updater.Instance.proxy.StartCoroutine(_Send());
        }
    }

    public IMHttpContext Send(Base_Upload ul, Action<IMHttpContext> callback) {
        IMHttpContext ctx = new IMHttpContext();
        ctx.url = url;
        ctx.ul = ul;
        ctx.retry = 2;
        ctx.callback = callback;
        sendQueue.Enqueue(ctx);
        return ctx;
    }

    public bool IsBusy() {
        return busy || sendQueue.Count > 0;
    }

    public void CancelAll() {
        sendQueue.Clear();
    }

    public static int snoSeed = (int)(DateTime.UtcNow - new DateTime(2015, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;

    IEnumerator _Send() {
        IMHttpContext ctx = sendQueue.Dequeue();
        BonDocument request = new BonDocument();
        ctx.request = request;
        request["ul"] = ctx.ul.GetType().Name;
        BonDocument p = BonUtil.ToBon(ctx.ul, null, null) as BonDocument;
        request["p"] = p;
        ctx.sno = (++snoSeed);
        request["sno"] = ctx.sno;
        IMMe me = IM.Instance.me;
        if(me != null) {
            p["uid"] = me.id;
            if(me.auth != null) {
                request["auth"] = me.auth;
            }
        }
        byte[] postdata = ctx.request.ToBonBytes();
        postdata = EncryptDecrypt.Encrypt(postdata);

        while(ctx.retry >= 0) {
#if HTTP_DEBUG_LOG
            Debug.Log("发送数据{" + postdata.Length + "}: " + ctx.request.ToJsonString());
            Debug.Log("到: " + ctx.url);
#endif
            ctx.sendTime = TimeUtil.UnixTimestampNow();
            float lastProgress = 0;
            double timeout = TimeUtil.UnixTimestampNow() + 10;
            bool isTimeout = false;
            using(WWW www = new WWW(ctx.url, postdata, header)) {
                while(!www.isDone) {
                    double now = TimeUtil.UnixTimestampNow();
                    if(now >= timeout) {
                        isTimeout = true;
                        break;
                    }
                    if(lastProgress != www.progress) {
                        lastProgress = www.progress;
                        timeout = now + 20;
                    }
                    yield return null;
                }
                if(destroyed) {
                    busy = false;
                    yield break;
                }
                if(isTimeout) {
                    ctx.error = "UI.网络错误";
                    ctx.code = -1;
                    Debug.LogError("访问超时");
                } else if(www.error != null) {
                    ctx.error = "UI.网络错误";
                    ctx.code = -1;
                    Debug.LogError("Err:" + www.error);
                } else {
                    byte[] data = www.bytes;
                    try {
                        if(data[0] == 31) {
                            try {
                                data = GZip.Decompress(data);
                            } catch(Exception) { }
                        }
                        ctx.response = BonDocument.FromBonBytes(data);
#if HTTP_DEBUG_LOG
                        Debug.Log("收到数据: " + ctx.response);
#endif
                        ctx.code = ctx.response.GetInt("code");
                        ctx.error = null;
                        ctx.retry = 0;
                    } catch(Exception e) {
                        ctx.error = "数据解析错误";
                        Debug.LogError("下行数据解析错误 " + e.ToString());
                    }
                }
            }
            ctx.retry--;
        }
        busy = false;
        if(ctx.callback != null) {
            ctx.callback(ctx);
        }
        if(OnResult != null) {
            OnResult(ctx);
        }
    }

}
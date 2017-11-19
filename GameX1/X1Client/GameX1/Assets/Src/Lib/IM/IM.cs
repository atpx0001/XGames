using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace go2play.lib.im {
    public class IM {
        public static string LOGIN_URL =
        //"http://localhost:47514/us.ashx";
#if RELEASE //|| RELEASE1
        "http://gsdim.go2play.com/us.ashx";
#else
        "http://work.go2play.com:83/monpet_im/us.ashx";
#endif
        public string voiceUrlRoot;

        static IM instance;
        public static IM Instance {
            get {
                if(instance == null) {
                    instance = new IM();
                }
                return instance;
            }
        }


        public static void Destroy() {
            if(instance != null) {
                instance.newMsgReceived = null;
                instance.sm.ChangeState(State.离线);
                Updater.Instance.OnPreUpdate.Remove(instance.update);
                instance = null;
            }
        }

        enum State {
            离线,
            登录,
            在线,
        }

        CachedDictionary<string, IMChannel> myChannels = new CachedDictionary<string, IMChannel>(5);
        CachedDictionary<string, IMChatSession> chatSessions = null;
        CachedDictionary<string, IMChatServerChannel> channelServers = new CachedDictionary<string, IMChatServerChannel>();
        Dictionary<string, IMUser> allUsers = new Dictionary<string, IMUser>(50);
        List<IMMsgForSend> sendingMsgs = new List<IMMsgForSend>(10);
        List<IMMsg> newMsgs = new List<IMMsg>(100);
        Action update;
        SimpleStateMachine<State> sm;
        public IMMe me;
        IMHttpClient loginHttp;
        IMChatServerPrivate privateServer;
        public event Action<IMMsg> newMsgReceived;
        bool needSaveSessions = false;


        public IMChatSession[] GetAllSessions() {
            return chatSessions.Values;
        }
        public bool IsOnline {
            get {
                return sm.curState == State.在线;
            }
        }

        private IM() {
            loginHttp = new IMHttpClient(LOGIN_URL);
            update = Update;
            Updater.Instance.OnPreUpdate.Add(update);
            sm = new SimpleStateMachine<State>();
            sm.Register(State.离线, 离线_Enter, null, 离线_Update);
            sm.Register(State.登录, 登录_Enter, null, null);
            sm.Register(State.在线, null, null, 在线_Update);
            sm.ChangeState(State.离线);
        }

        void Update() {
            sm.Update();
        }

        void SaveSessions() {
            IMChatSession[] sessions = chatSessions.Values.Where(p => p.target is IMUser).ToArray();
            SaveData.SaveObjPersistent(me.id, "privateChatSessions", sessions);
        }

        void LoadSessions() {
            if(chatSessions != null) {
                return;
            }
            chatSessions = new CachedDictionary<string, IMChatSession>(5);
            try {
                IMChatSession[] sessions = SaveData.LoadObjPersistent<IMChatSession[]>(me.id, "privateChatSessions");
                if(sessions == null) {
                    return;
                }
                for(int i = sessions.Length; --i >= 0;) {
                    IMChatSession session = sessions[i];
                    chatSessions[session.id] = session;
                    IMUser user = session.target as IMUser;
                    if(user != null) {
                        allUsers[user.id.ToString()] = user;
                        user.detailGot = false;
                    }
                }
            } catch(Exception e) {
            }
        }

        private IMChatServerChannel GetChannelServer(string url) {
            IMChatServerChannel s;
            if(!channelServers.TryGetValue(url, out s)) {
                s = new IMChatServerChannel(this, url);
                channelServers[url] = s;
            }
            return s;
        }

        public IMUser GetUser(string id) {
            IMUser u;
            if(!allUsers.TryGetValue(id, out u)) {
                u = new IMUser();
                u.id = Convert.ToInt32(id);
                allUsers[id] = u;
            }
            return u;
        }

        public IMChannel GetChannel(string id) {
            IMChannel ch;
            if(myChannels.TryGetValue(id, out ch)) {
                return ch;
            }
            return null;
        }

        public IIMChatTarget GetChatTarget(IMChatTarget target, string id) {
            switch(target) {
                case IMChatTarget.用户: return GetUser(id);
                case IMChatTarget.频道: return GetChannel(id);
            }
            return null;
        }

        /// <summary>
        /// 设置成离线
        /// </summary>
        public void SetOffline() {
            sm.ChangeState(State.离线);
        }

        void 离线_Enter() {
            foreach(var chs in channelServers.Values) {
                chs.http.Destroy();
            }
            channelServers.Clear();
            if(privateServer != null) {
                privateServer.http.CancelAll();
            }
            IMChannel[] cs = myChannels.Values;
            for(int i = cs.Length; --i >= 0;) {
                IMChannel c = cs[i];
                c.SetOffline();
            }
            distributedMsgTime = 0;
        }

        float loginTime = 0;
        void 离线_Update() {
            if(me == null) {
                return;
            }
            if(Time.realtimeSinceStartup > loginTime) {
                loginTime = Time.realtimeSinceStartup + 5;
                sm.ChangeState(State.登录);
            }
        }

        void 登录_Enter() {
            UL_User_login ul = new UL_User_login();
            ul.name = me.name;
            ul.head = me.head;
            ul.pwd = me.pwd;
            loginHttp.Send(ul, ctx => {
                if(ctx.code != 0) {
                    Debug.Log("聊天服务器登录失败");
                    sm.ChangeState(State.离线);
                    return;
                }
                try {
                    BonUtil.ToObj(ctx.response["user"], me);
                    List<string> chatUrls = BonUtil.ToObj<List<string>>(ctx.response["privateServers"], null);
                    if(chatUrls.Count == 0) {
                        sm.ChangeState(State.离线);
                        return;
                    }
                    voiceUrlRoot = ctx.response["voiceUrlRoot"].AsString;
                    string privateUrl = chatUrls[UnityEngine.Random.Range(0, chatUrls.Count)];
                    if(privateServer == null) {
                        privateServer = new IMChatServerPrivate(this, privateUrl);
                    } else {
                        privateServer.http.CancelAll();
                        privateServer.http.url = privateUrl;
                    }
                    sm.ChangeState(State.在线);
                } catch(Exception e) {
                    Debug.Log(e);
                    sm.ChangeState(State.离线);
                    return;
                }
            });
        }

        float onlineUpdateTime;
        void 在线_Update() {
            if(Time.realtimeSinceStartup < onlineUpdateTime) {
                return;
            }
            onlineUpdateTime = Time.realtimeSinceStartup + 0.2f;
            进入频道();
            分发消息();
            privateServer.Update();
            var chss = channelServers.Values;
            for(int i = chss.Length; --i >= 0;) {
                chss[i].Update();
            }
            处理新消息();
            if(needSaveSessions) {
                needSaveSessions = false;
                SaveSessions();
            }
        }

        float distributedMsgTime;
        void 分发消息() {
            if(Time.realtimeSinceStartup < distributedMsgTime) {
                return;
            }
            distributedMsgTime = Time.realtimeSinceStartup + 1;
            if(sendingMsgs.Count == 0) {
                return;
            }
            for(int i = 0; i < sendingMsgs.Count; i++) {
                IMMsgForSend msg = sendingMsgs[i];
                if(msg.Sent || msg.RecTgt == IMChatTarget.频道 && !myChannels.ContainsKey(msg.recId)) {
                    sendingMsgs.RemoveAt(i);
                    i--;
                    continue;
                }
                if(!msg.Distributed) {
                    switch(msg.RecTgt) {
                        case IMChatTarget.用户: {
                            privateServer.AddMsg(msg);
                            break;
                        }
                        case IMChatTarget.频道: {
                            IMChannel ch = GetChannel(msg.recId);
                            if(!ch.IsOnline) {
                                continue;
                            }
                            GetChannelServer(ch.url).AddMsg(msg);
                            break;
                        }
                    }
                }
            }
        }

        public void AddNewMsgs(List<IMMsg> msgs) {
            newMsgs.AddRange(msgs);
        }
        List<int> toGetDetails = new List<int>(10);
        void 处理新消息() {
            if(newMsgs.Count == 0) {
                return;
            }
            bool busy = privateServer.http.IsBusy();
            toGetDetails.Clear();
            for(int i = 0; i < newMsgs.Count; i++) {
                IMMsg msg = newMsgs[i];
                IMUser sender = msg.Sender;
                if(!sender.detailGot) {
                    toGetDetails.Add(sender.id);
                }
                if(toGetDetails.Count > 0) {
                    continue;
                }
                newMsgs.RemoveAt(i);
                i--;
                Debug.Log("[" + sender.name + "]: " + msg.text);
                IMChatSession session = GetSession(msg);
                //PlayMsg(msg);
                session.AddMsg(msg);
                if(session.target is IMUser) {
                    needSaveSessions = true;
                }
                if(newMsgReceived != null) {
                    newMsgReceived(msg);
                }
            }
            if(busy || toGetDetails.Count == 0) {
                return;
            }
            UL_User_getDetails ul = new UL_User_getDetails();
            ul.uIds = toGetDetails;
            privateServer.http.Send(ul, ctx => {
                if(ctx.code != 0) {
                    Debug.Log("获取用户信息失败");
                    sm.ChangeState(State.离线);
                    return;
                }
                try {
                    BonUtil.ToObj(ctx.response["users"], allUsers);
                    foreach(BonElement be in ctx.response["users"].AsBonDocument) {
                        IMUser u = allUsers[be.name];
                        if(u.id == 0) {
                            u.id = Convert.ToInt32(be.name);
                        }
                        u.detailGot = true;
                    }
                    for(int i = toGetDetails.Count; --i >= 0;) {
                        if(!allUsers[toGetDetails[i].ToString()].detailGot) {
                            newMsgs.RemoveAll(p => p.sndId == toGetDetails[i]);
                        }
                    }
                } catch(Exception e) {
                    Debug.Log(e);
                    sm.ChangeState(State.离线);
                    return;
                }
            });
        }

        List<string> offlineChannels = new List<string>(10);
        void 进入频道() {
            if(!privateServer.http.IsBusy()) {
                offlineChannels.Clear();
                IMChannel[] cs = myChannels.Values;
                for(int i = cs.Length; --i >= 0;) {
                    IMChannel c = cs[i];
                    if(!c.IsOnline) {
                        offlineChannels.Add(c.id);
                    }
                }
                if(offlineChannels.Count > 0) {
                    UL_Chat_getChannels ul = new UL_Chat_getChannels();
                    ul.chIds = offlineChannels;
                    privateServer.http.Send(ul, ctx => {
                        distributedMsgTime = 0;
                        if(ctx.code != 0) {
                            Debug.Log("获取频道服务器失败");
                            sm.ChangeState(State.离线);
                            return;
                        }
                        try {
                            foreach(BonElement chd in ctx.response["channels"].AsBonDocument) {
                                IMChannel ch;
                                if(!myChannels.TryGetValue(chd.name, out ch)) {
                                    continue;
                                }
                                ch.url = chd.value["url"].AsString;
                                GetChannelServer(ch.url).channels.Add(ch.id);
                            }
                        } catch(Exception e) {
                            Debug.Log(e);
                            sm.ChangeState(State.离线);
                            return;
                        }
                    });
                }
            }
        }

        IMChatSession GetSession(string key) {
            IMChatSession session;
            if(!chatSessions.TryGetValue(key, out session)) {
                session = new IMChatSession();
                session.id = key;
                chatSessions[key] = session;
            }
            return session;
        }

        IMChatSession GetSession(IMMsg msg) {
            return GetSession(msg.Receiver == me ? msg.Sender : msg.Receiver);
        }

        public IMChatSession GetSession(IIMChatTarget chatTarget) {
            switch(chatTarget.Target) {
                case IMChatTarget.用户: {
                    return GetSession((IMUser)chatTarget);
                }
                case IMChatTarget.频道: {
                    return GetSession((IMChannel)chatTarget);
                }
            }
            return null;
        }

        public IMChatSession GetSession(IMChannel channel) {
            string key = "channel." + channel.id;
            var s = GetSession(key);
            s.target = channel;
            return s;
        }

        public IMChatSession GetSession(IMUser other) {
            string key = "private.";
            if(me.id.CompareTo(other.id) < 0) {
                key += me.id + "." + other.id;
            } else {
                key += other.id + "." + me.id;
            }
            var s = GetSession(key);
            s.target = other;
            return s;
        }

        /// <summary>
        /// 登录聊天服务器
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="head"></param>
        /// <param name="pwd"></param>
        public void Login(int id, string name, string head, string pwd) {
            if(me == null) {
                me = new IMMe();
            }
            me.id = id;
            me.name = name;
            me.head = head;
            me.pwd = pwd;
            me.auth = null;
            me.detailGot = true;
            allUsers[me.id.ToString()] = me;
            LoadSessions();
        }

        /// <summary>
        /// 更新我的信息
        /// </summary>
        /// <param name="name"></param>
        /// <param name="head"></param>
        public void UpdateInfo(string name, string head) {
            if(me == null) {
                return;
            }
            me.name = name;
            me.head = head;
            if(!IsOnline) {
                return;
            }

        }

        /// <summary>
        /// 进入频道，记得退出该退出的频道，例如世界频道
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public IMChannel EnterChannel(string channel) {
            IMChannel ch;
            if(!myChannels.TryGetValue(channel, out ch)) {
                ch = new IMChannel();
                ch.id = channel;
                myChannels[ch.id] = ch;
            }
            return ch;
        }

        /// <summary>
        /// 退出频道
        /// </summary>
        /// <param name="channel"></param>
        public void ExitChannel(string channel) {
            myChannels.Remove(channel);
        }

        private void SendMsg(IIMChatTarget target, IMMsgForSend msg) {
            msg.RecTgt = target.Target;
            msg.recId = target.Id;
            sendingMsgs.Add(msg);
            distributedMsgTime = 0;
        }

        /// <summary>
        /// 发送文字消息
        /// </summary>
        /// <param name="target"></param>
        /// <param name="text"></param>
        /// <param name="extra"></param>
        public void SendTextMsg(IIMChatTarget target, string text, string extra = null) {
            IMMsgForSend msg = new IMMsgForSend();
            msg.text = text;
            msg.extra = extra;
            SendMsg(target, msg);
        }

        /// <summary>
        /// 开始录音,最大30秒
        /// </summary>
        /// <param name="maxTime">秒</param>
        /// <returns>语音key</returns>
        public string StartTalk(float maxTime) {
            //if(maxTime > 30) {
            //    maxTime = 30;
            //}
            //string key = Guid.NewGuid().ToString("N");
            //string p = ExpireCache.GetPath(VoiceKeyToPath(key));
            //AmrRecorderPlayer.StartRecord(p, maxTime);
            //return key;
            return "N";
        }

        /// <summary>
        /// 结束录音
        /// </summary>
        /// <returns></returns>
        public bool StopTalk() {
            //return AmrRecorderPlayer.StopRecord();
            return true;
        }

        /// <summary>
        /// 发送语音消息
        /// </summary>
        /// <param name="target"></param>
        /// <param name="voiceKey"></param>
        /// <param name="voiceToText">是否将语音翻译成文字</param>
        /// <param name="extra"></param>
        public void SendVoiceMsg(IIMChatTarget target, string voiceKey, bool voiceToText, string extra = null) {
            IMMsgForSend msg = new IMMsgForSend();
            msg.voice = voiceKey;
            if(voiceKey != null) {
                //ExpireCache.TryLoad(VoiceKeyToPath(voiceKey), out msg.voiceData);
            }
            msg.extra = extra;
            if(voiceToText && msg.voiceData != null) {
                Updater.Instance.proxy.StartCoroutine(TranslateVoice(target, msg));
            } else {
                SendMsg(target, msg);
            }
        }

        float expiresTime;
        string access_token;
        IEnumerator TranslateVoice(IIMChatTarget target, IMMsgForSend msg) {
            if(access_token == null || Time.realtimeSinceStartup > expiresTime) {
                string url = "https://openapi.baidu.com/oauth/2.0/token?grant_type=client_credentials&client_id=5wjanYysoKk8Qn235ZPdAKuP&client_secret=364785708f2049fb1050eab308d06537";
                using(WWW www = new WWW(url)) {
                    yield return www;
                    if(www.error != null) {
                        SendMsg(target, msg);
                        yield break;
                    }
                    try {
                        BonDocument doc = BonDocument.FromJsonString(www.text);
                        expiresTime = Time.realtimeSinceStartup + doc["expires_in"].AsInt;
                        access_token = doc["access_token"].AsString;
                    } catch(Exception e) {
                        Debug.Log(e);
                        SendMsg(target, msg);
                        yield break;
                    }
                }
            }
            Dictionary<string, string> header = new Dictionary<string, string>();
            header["Content-Type"] = "audio/amr;rate=8000";
            header["Content-Length"] = msg.voiceData.Length.ToString();
            using(WWW www2 = new WWW("http://vop.baidu.com/server_api?cuid=" + SystemInfo.deviceUniqueIdentifier + "&token=" + access_token, msg.voiceData, header)) {
                yield return www2;
                if(www2.error != null) {
                    SendMsg(target, msg);
                    yield break;
                }
                try {
                    BonDocument doc = BonDocument.FromJsonString(www2.text);
                    if(doc.GetInt("err_no") != 0) {
                        Debug.Log(doc.GetString("err_msg"));
                        SendMsg(target, msg);
                        yield break;
                    }
                    BonArray results = doc.GetBonArray("result");
                    msg.text = results[UnityEngine.Random.Range(0, results.Count)].AsString.TrimEnd(',', '，');
                    //msg.text = DataManager_Sensitiveword.instance.替换敏感词(msg.text);
                    SendMsg(target, msg);
                    yield break;
                } catch(Exception e) {
                    Debug.Log(e);
                    SendMsg(target, msg);
                    yield break;
                }
            }
        }

        /// <summary>
        /// 获取语音key对应的缓存文件目录
        /// </summary>
        /// <param name="voiceKey"></param>
        /// <returns></returns>
        string VoiceKeyToPath(string voiceKey) {
            return "voice/" + voiceKey + ".amr";
        }

        HashSet<string> downloadingVoices = new HashSet<string>();
        string lastVoice;
        /// <summary>
        /// 播放语音，未下载的语音会先下载
        /// </summary>
        /// <param name="msg"></param>
        public void PlayMsg(IMMsg msg) {
            if(msg.voice == null) {
                return;
            }
            string path;
            bool isToday;
            //if(ExpireCache.TryGetPath(VoiceKeyToPath(msg.voice), out path, out isToday)) {
            //    AmrRecorderPlayer.Play(path);
            //    return;
            //}
            lastVoice = msg.voice;
            string url = string.Format("{0}{1}/{2}/{3}/{4}.amr", voiceUrlRoot, TimeUtil.unixTimeZero.AddSeconds(msg.time).ToString("yyyyMMdd"), msg.voice.Substring(0, 2), msg.voice.Substring(2, 2), msg.voice);
            if(downloadingVoices.Contains(msg.voice)) {
                return;
            }
            downloadingVoices.Add(msg.voice);
            Updater.Instance.proxy.StartCoroutine(DownloadVoice(msg.voice, url));
        }

        /// <summary>
        /// 语音是否正在下载
        /// </summary>
        /// <param name="voiceKey"></param>
        /// <returns></returns>
        public bool IsVoiceDownloading(string voiceKey) {
            return downloadingVoices.Contains(voiceKey);
        }
        /// <summary>
        /// 语音是否已下载
        /// </summary>
        /// <param name="voiceKey"></param>
        /// <returns></returns>
        public bool IsVoiceDownloaded(string voiceKey) {
            //string p;
            //bool t;
            //return ExpireCache.TryGetPath(VoiceKeyToPath(voiceKey), out p, out t);
            return false;
        }

        IEnumerator DownloadVoice(string voice, string url) {
            using(WWW www = new WWW(url)) {
                yield return www;
                downloadingVoices.Remove(voice);
                if(www.error != null) {
                    Debug.Log("下载语音出错：" + www.error);
                    yield break;
                }
                byte[] data = www.bytes;
                //string path = ExpireCache.Save(VoiceKeyToPath(voice), data);
                //if(voice == lastVoice) {
                //    AmrRecorderPlayer.Play(path);
                //}
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace go2play.lib.im {
    public abstract class IMChatServerBase {
        public IM im;
        public IMHttpClient http;
        protected Queue<IMMsgForSend> sendingMsgs = new Queue<IMMsgForSend>();
        const float minHeartbeatTimeSpan = 2;
        const float maxHeartbeatTimeSpan = 10;
        float heartbeatTimeSpan = minHeartbeatTimeSpan;
        public IMChatServerBase(IM im, string url) {
            this.im = im;
            http = new IMHttpClient(url);
        }

        public void AddMsg(IMMsgForSend msg) {
            msg.Distributed = true;
            sendingMsgs.Enqueue(msg);
        }

        float heatbeatTime;
        public void Update() {
            if(!http.IsBusy()) {
                while(sendingMsgs.Count > 0) {
                    IMMsgForSend msg = sendingMsgs.Dequeue();
                    if(!IsMsgValid(msg)) {
                        msg.Distributed = false;
                        continue;
                    }
                    Base_Upload ul = MakeUL(msg);
                    //Debug.Log(DateTime.Now + ": " + ul.GetType().Name);
                    http.Send(ul, ctx => {
                        heartbeatTimeSpan = minHeartbeatTimeSpan;
                        heatbeatTime = Time.realtimeSinceStartup + heartbeatTimeSpan;
                        OnSendDone(msg, ctx);
                    });
                    break;
                }
            }
            if(!http.IsBusy() && Time.realtimeSinceStartup > heatbeatTime) {
                Base_Upload ul = MakeHeatbeatUL();
                //Debug.Log(DateTime.Now + ": " + ul.GetType().Name);
                http.Send(ul, ctx => {
                    if(OnHeatbeatDone(ctx)) {
                        heartbeatTimeSpan = minHeartbeatTimeSpan;
                    } else {
                        heartbeatTimeSpan = Mathf.Min(heartbeatTimeSpan + 1, maxHeartbeatTimeSpan);
                    }
                    heatbeatTime = Time.realtimeSinceStartup + heartbeatTimeSpan;
                });
            }
        }

        protected abstract Base_Upload MakeUL(IMMsgForSend msg);
        protected abstract Base_Upload MakeHeatbeatUL();
        protected abstract void OnSendDone(IMMsgForSend msg, IMHttpContext ctx);
        protected abstract bool OnHeatbeatDone(IMHttpContext ctx);

        protected abstract bool IsMsgValid(IMMsgForSend msg);
    }

    public class IMChatServerChannel: IMChatServerBase {
        public List<string> channels = new List<string>();

        public IMChatServerChannel(IM im, string url) : base(im, url) {
        }

        void ClearInvalidChannels() {
            for(int i = channels.Count; --i >= 0;) {
                IMChannel ch = im.GetChannel(channels[i]);
                if(ch == null || ch.url != http.url) {
                    channels.RemoveAt(i);
                }
            }
        }

        protected override bool IsMsgValid(IMMsgForSend msg) {
            ClearInvalidChannels();
            return channels.Contains(msg.recId);
        }

        protected override Base_Upload MakeUL(IMMsgForSend msg) {
            UL_Chat_sendChannelMsg ul = new UL_Chat_sendChannelMsg();
            ul.lastId = im.GetSession(im.GetChannel(msg.recId)).lastId;
            ul.msg = msg;
            return ul;
        }

        void ClearAll() {
            foreach(var m in sendingMsgs) {
                m.Distributed = false;
            }
            sendingMsgs.Clear();
            for(int i = channels.Count; --i >= 0;) {
                IMChannel ch2 = im.GetChannel(channels[i]);
                ch2.SetOffline();
            }
            channels.Clear();
        }

        protected override void OnSendDone(IMMsgForSend msg, IMHttpContext ctx) {
            IMChannel ch = im.GetChannel(msg.recId);
            if(ch == null) {
                return;
            }
            ClearInvalidChannels();
            if(ctx.code < 0) {
                Debug.Log("发送聊天致命错误");
                msg.Distributed = false;
                ClearAll();
                return;
            }
            if(ctx.code > 0) {
                Debug.Log("发送聊天错误");
                msg.Distributed = false;
                ch.SetOffline();
                return;
            }
            msg.Sent = true;
            try {
                BonDocument chd = ctx.response.GetBonDocument("channel");
                if(chd == null) {
                    msg.Distributed = false;
                    ch.SetOffline();
                    return;
                }
                SyncChannelMsgs(chd, ch);
            } catch(Exception e) {
                Debug.Log(e);
                msg.Distributed = false;
                ch.SetOffline();
            }
        }

        protected override Base_Upload MakeHeatbeatUL() {
            ClearInvalidChannels();
            UL_Chat_getNewChannelMsgs ul = new UL_Chat_getNewChannelMsgs();
            ul.lastIds = new Dictionary<string, long>();
            for(int i = channels.Count; --i >= 0;) {
                IMChannel ch = im.GetChannel(channels[i]);
                ul.lastIds[ch.id] = im.GetSession(ch).lastId;
            }
            return ul;
        }

        protected override bool OnHeatbeatDone(IMHttpContext ctx) {
            ClearInvalidChannels();
            if(ctx.code != 0) {
                Debug.Log("获取频道新聊天失败");
                ClearAll();
                return false;
            }

            BonDocument chds = ctx.response.GetBonDocument("channels");
            if(chds == null) {
                ClearAll();
                return false;
            }
            bool haveMsg = false;
            for(int i = channels.Count; --i >= 0;) {
                IMChannel ch = im.GetChannel(channels[i]);
                try {
                    if(!chds.Contains(ch.id)) {
                        ch.SetOffline();
                        continue;
                    }
                    if(SyncChannelMsgs(chds[ch.id].AsBonDocument, ch) > 0) {
                        haveMsg = true;
                    }
                } catch(Exception e) {
                    Debug.Log(e);
                    ch.SetOffline();
                    continue;
                }
            }
            return haveMsg;
        }

        List<IMMsg> newMsgs = new List<IMMsg>(10);
        int SyncChannelMsgs(BonDocument chd, IMChannel ch) {
            IMChatSession session = im.GetSession(ch);
            session.lastId = chd["lastId"].AsLong;
            newMsgs.Clear();
            BonUtil.ToObj(chd["msgs"], newMsgs);
            for(int i = newMsgs.Count; --i >= 0;) {
                newMsgs[i].im = im;
                newMsgs[i].recTgt = IMChatTarget.频道;
            }
            im.AddNewMsgs(newMsgs);
            return newMsgs.Count;
        }
    }

    public class IMChatServerPrivate: IMChatServerBase {
        public long lastId = -1;

        public IMChatServerPrivate(IM im, string url) : base(im, url) {
        }

        protected override bool IsMsgValid(IMMsgForSend msg) {
            return true;
        }

        protected override Base_Upload MakeUL(IMMsgForSend msg) {
            UL_Chat_sendPrivateMsg ul = new UL_Chat_sendPrivateMsg();
            ul.lastId = lastId;
            ul.msg = msg;
            return ul;
        }

        protected override void OnSendDone(IMMsgForSend msg, IMHttpContext ctx) {
            if(ctx.code != 0) {
                Debug.Log("发送聊天致命错误");
                msg.Distributed = false;
                im.SetOffline();
                return;
            }
            msg.Sent = true;
            try {
                if(!ctx.response.Contains("private")) {
                    im.SetOffline();
                    return;
                }
                SyncPrivateMsgs(ctx.response["private"].AsBonDocument);
            } catch(Exception e) {
                Debug.Log(e);
                im.SetOffline();
            }
        }

        protected override Base_Upload MakeHeatbeatUL() {
            UL_Chat_getNewPrivateMsgs ul = new UL_Chat_getNewPrivateMsgs();
            ul.lastId = lastId;
            return ul;
        }

        protected override bool OnHeatbeatDone(IMHttpContext ctx) {
            if(ctx.code != 0) {
                Debug.Log("获取私聊失败");
                im.SetOffline();
                return false;
            }
            try {
                if(!ctx.response.Contains("private")) {
                    im.SetOffline();
                    return false;
                }
                return SyncPrivateMsgs(ctx.response["private"].AsBonDocument) > 0;
            } catch(Exception e) {
                Debug.Log(e);
                im.SetOffline();
            }
            return false;
        }

        List<IMMsg> newMsgs = new List<IMMsg>(10);
        int SyncPrivateMsgs(BonDocument chd) {
            lastId = chd["lastId"].AsLong;
            List<IMMsg> newMsgs = new List<IMMsg>();
            BonUtil.ToObj(chd["msgs"], newMsgs);
            for(int i = newMsgs.Count; --i >= 0;) {
                newMsgs[i].im = im;
                newMsgs[i].recTgt = IMChatTarget.用户;
            }
            im.AddNewMsgs(newMsgs);
            return newMsgs.Count;
        }
    }
}

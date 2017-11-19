using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace go2play.lib.im {
    public enum IMChatTarget {
        用户 = 0,
        频道 = 1,
    }
    public interface IIMChatTarget {
        IMChatTarget Target { get; }
        string Id { get; }
    }

    public class IMMsg {
        public long id;
        public long time;
        public string text;
        public string voice;
        public string extra;
        public int sndId;
        public IMChatTarget recTgt;
        public string recId;
        public IM im { get; set; }
        public IMUser Sender {
            get {
                return im.GetUser(sndId.ToString());
            }
        }
        public IIMChatTarget Receiver {
            get {
                return im.GetChatTarget(recTgt, recId);
            }
        }
    }

    public class IMMsgForSend {
        public string text;
        public string voice;
        public byte[] voiceData;
        public string extra;
        public string recId;
        public IMChatTarget RecTgt { get; set; }
        public bool Distributed { get; set; }
        public bool Sent { get; set; }
    }
}

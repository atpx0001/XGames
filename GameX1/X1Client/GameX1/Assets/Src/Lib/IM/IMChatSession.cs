using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace go2play.lib.im {
    public class IMChatSession {
        public string id;
        public long lastId = -1;
        public IIMChatTarget target;
        public List<IMMsg> msgs = new List<IMMsg>();
        const int MAX = 100;

        public void AddMsg(IMMsg msg) {
            msgs.Add(msg);
            if(msgs.Count > MAX) {
                msgs.RemoveRange(0, msgs.Count - MAX);
            }
        }
    }
}

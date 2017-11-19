using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace go2play.lib.im {
    public class IMUser: IIMChatTarget {
        public int id;
        public string name;
        public string head;
        public bool detailGot;

        public IMChatTarget Target {
            get {
                return IMChatTarget.用户;
            }
        }

        public string Id {
            get {
                return id.ToString();
            }
        }
    }

    public class IMMe: IMUser {
        public string auth;
        public string pwd;
    }
}

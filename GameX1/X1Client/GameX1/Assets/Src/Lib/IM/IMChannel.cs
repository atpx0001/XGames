using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace go2play.lib.im {
    public class IMChannel: IIMChatTarget {
        public string id;
        public string url;

        public bool IsOnline {
            get {
                return url != null;
            }
        }

        public void SetOffline() {
            url = null;
        }

        public IMChatTarget Target {
            get {
                return IMChatTarget.频道;
            }
        }

        public string Id {
            get {
                return id;
            }
        }
    }
}

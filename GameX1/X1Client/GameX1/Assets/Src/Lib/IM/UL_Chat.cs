using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace go2play.lib.im {
    public class UL_Chat_getChannels: Base_Upload {
        public List<string> chIds;
    }

    public class UL_Chat_sendChannelMsg: Base_Upload {
        public long lastId;
        public IMMsgForSend msg;
    }

    public class UL_Chat_sendPrivateMsg: Base_Upload {
        public long lastId;
        public IMMsgForSend msg;
    }

    public class UL_Chat_getNewChannelMsgs: Base_Upload {
        public Dictionary<string, long> lastIds;
    }

    public class UL_Chat_getNewPrivateMsgs: Base_Upload {
        public long lastId;
    }
}

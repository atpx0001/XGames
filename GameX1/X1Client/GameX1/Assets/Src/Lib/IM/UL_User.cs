using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace go2play.lib.im {
    public class UL_User_login: Base_Upload {
        public string name;
        public string head;
        public string pwd;
    }

    public class UL_User_getDetails: Base_Upload {
        public List<int> uIds;
    }

    /// <summary>
    /// 更新用户信息
    /// </summary>
    public class UL_User_updateInfo: Base_Upload {
        /// <summary>
        /// 名字
        /// </summary>
        public string name;
        /// <summary>
        /// 头像
        /// </summary>
        public string head;
    }
}

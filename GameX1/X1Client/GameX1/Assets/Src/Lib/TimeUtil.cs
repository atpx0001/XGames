using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static class TimeUtil {
    public static readonly DateTime unixTimeZero = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    /// <summary>
    /// 从1970年1月1日到现在经过的秒数
    /// </summary>
    /// <returns></returns>
    public static double UnixTimestampNow() {
        return (DateTime.UtcNow - unixTimeZero).TotalSeconds;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="timestamp"></param>
    /// <returns></returns>
    public static DateTime Timestamp2DateTime(int timestamp) {
        return unixTimeZero.AddSeconds(timestamp);
    }

    public static string GetTimeText_HHMMSS(int second) {
        TimeSpan ts = TimeSpan.FromSeconds(second);
        if(ts.Days > 0) {
            return string.Format("{0}:{1:D2}:{2:D2}", ts.Days * 24 + ts.Hours, ts.Minutes, ts.Seconds);
        } else {
            return string.Format("{0:D2}:{1:D2}:{2:D2}", ts.Hours, ts.Minutes, ts.Seconds);
        }
    }
    public static string GetTimeText_剩余n天(int second, string prefix = "") {
        TimeSpan ts = TimeSpan.FromSeconds(second);
        if(ts.Days > 0) {
            return prefix + ts.Days + "天";
        } else {
            return string.Format("{0:D2}:{1:D2}:{2:D2}", ts.Hours, ts.Minutes, ts.Seconds);
        }
    }
    public static string GetTimeText_剩余n天_活动专用(int second) {
        TimeSpan ts = TimeSpan.FromSeconds(second);
        if(ts.Days > 0) {
            return ts.Days + " " + "天";
        } else {
            return string.Format("{0:D2}:{1:D2}:{2:D2}", ts.Hours, ts.Minutes, ts.Seconds);
        }
    }
    public static string GetTimeText_活动耗时(int second) {
        TimeSpan ts = TimeSpan.FromSeconds(second);
        StringBuilder msg = new StringBuilder();


        if(ts.Minutes > 0) {
            msg.Append((int)ts.Minutes + ts.Hours * 60).Append("分钟");
        }
        if(ts.Seconds > 0) {
            msg.Append((int)ts.Seconds).Append("秒");
        }

        return msg.ToString();
    }

    public static string GetTimeText_n天以前(int second) {
        TimeSpan ts = TimeSpan.FromSeconds(second);
        StringBuilder msg = new StringBuilder();
        if(ts.Days > 0) {
            msg.Append((int)ts.Days).Append("天");
        } else {
            if(ts.Hours > 0) {
                msg.Append((int)ts.Hours).Append("小时");
            }
            msg.Append((int)ts.Minutes).Append("分钟");
        }
        msg.Append("以前");
        return msg.ToString();
    }
    public static string Get模糊时间(int second) {
        StringBuilder msg = new StringBuilder();
        if(second < 60) {
            msg.Append(second).Append("秒");
        } else if(second < 60 * 60) {
            msg.Append(second / 60).Append("分钟");
            int s = second % 60;
            if(s != 0) {
                msg.Append(s).Append("秒");
            }
        } else if(second < 3600 * 24) {
            msg.Append(second / 3600).Append("小时");
            int m = second % 3600 / 60;
            if(m != 0) {
                msg.Append(m).Append("分钟");
            }
        } else {
            msg.Append(second / (3600 * 24)).Append("天");
            int h = second % (3600 * 24) / 3600;
            if(h != 0) {
                msg.Append(h).Append("小时");
            }
        }
        return msg.ToString();
    }

    /// <summary>
    /// 返回2016-01-31格式
    /// </summary>
    /// <param name="timestamp"></param>
    /// <returns></returns>
    public static string GetDateText_YYMMDD(int timestamp) {
        return unixTimeZero.AddSeconds(timestamp).ToLocalTime().ToString("yyyy-MM-dd");
    }

    /// <summary>
    /// 返回2016-01格式
    /// </summary>
    /// <param name="timestamp"></param>
    /// <returns></returns>
    public static string GetDateText_YYMM(int timestamp) {
        return unixTimeZero.AddSeconds(timestamp).ToLocalTime().ToString("yyyy-MM");
    }

    /// <summary>
    /// 返回2016-01-31 12:00格式
    /// </summary>
    /// <param name="timestamp"></param>
    /// <returns></returns>
    public static string GetDateText_YYMMDD_HHMM(int timestamp) {
        return unixTimeZero.AddSeconds(timestamp).ToLocalTime().ToString("yyyy-MM-dd HH:mm");
    }
    public static string GetDateText_YYMMDD_HHMMSS(int timestamp) {
        return unixTimeZero.AddSeconds(timestamp).ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
    }

    public static string GetDateText_MM月DD日(int timestamp) {
        return unixTimeZero.AddSeconds(timestamp).ToLocalTime().ToString(string.Format("MM{0}dd{1}", "月", "日"));
    }
    public static string GetDateText_MM(int timestamp) {
        return unixTimeZero.AddSeconds(timestamp).ToLocalTime().ToString("MM");
    }
    public static string GetDateText_DD(int timestamp) {
        return unixTimeZero.AddSeconds(timestamp).ToLocalTime().ToString("dd");
    }

    public static string GetDateText_HHMM(int timestamp) {
        return unixTimeZero.AddSeconds(timestamp).ToLocalTime().ToString("HH:mm");
    }

    public static string GetDateText_HHMMSS(int timestamp) {
        return unixTimeZero.AddSeconds(timestamp).ToLocalTime().ToString("HH:mm:ss");
    }


}
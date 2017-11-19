
using go2play.lib.im;
using System;
using System.Linq;
using System.Collections.Generic;

public class IMHttpContext {
    public readonly static Action<IMHttpContext> dummyCallback = ctx => { };
    public string url;
    public Base_Upload ul;
    /// <summary>
    /// 唯一序列号
    /// </summary>
    public int sno;
    public BonDocument request;
    public int retry;
    public BonDocument response;
    public int code;
    public string error;
    /// <summary>
    /// 默认回调，优先调用回调，然后触发事件
    /// </summary>
    public Action<IMHttpContext> callback;
    /// <summary>
    /// 发送时的时间戳
    /// </summary>
    public double sendTime;
    /// <summary>
    /// 回调中如果将该值置为true，将不触发事件
    /// </summary>
    public bool done = false;
    /// <summary>
    /// 指令超时
    /// </summary>
    public double timeout;
}

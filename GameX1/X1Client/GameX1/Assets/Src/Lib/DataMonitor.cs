using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine.Events;

public class DataMonitor {
    public object data;
    public string field;
    public UnityAction callback;

    public DataMonitor(object data, string field, UnityAction callback) {
        this.data = data;
        this.field = field;
        this.callback = callback;
    }
}

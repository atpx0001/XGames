using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

public interface IBon {
    BonValue ToBon();
    void FromBon(BonValue bonValue);
}

public class BonUtil {
    static RoleStatic<Dictionary<object, Dictionary<string, List<DataMonitor>>>> _dataMonitor = new RoleStatic<Dictionary<object, Dictionary<string, List<DataMonitor>>>>();
    static RoleStatic<List<DataMonitor>> _dataChanged = new RoleStatic<List<DataMonitor>>();

    private static List<DataMonitor> dataChanged {
        get {
            if(_dataChanged.value == null) {
                _dataChanged.value = new List<DataMonitor>();
            }
            return _dataChanged.value;
        }
    }

    private static Dictionary<object, Dictionary<string, List<DataMonitor>>> dataMonitor {
        get {
            if(_dataMonitor.value == null) {
                _dataMonitor.value = new Dictionary<object, Dictionary<string, List<DataMonitor>>>();
            }
            return _dataMonitor.value;
        }
    }

    public static void StartMonitor() {
        dataChanged.Clear();
    }

    public static DataMonitor AddDataMonitor(DataMonitor dm) {
        Dictionary<string, List<DataMonitor>> ms;
        if(!dataMonitor.TryGetValue(dm.data, out ms)) {
            ms = new Dictionary<string, List<DataMonitor>>();
            dataMonitor[dm.data] = ms;
        }
        List<DataMonitor> ds;
        if(!ms.TryGetValue(dm.field, out ds)) {
            ds = new List<DataMonitor>(1);
            ms[dm.field] = ds;
        }
        ds.Add(dm);
        return dm;
    }

    public static DataMonitor AddDataMonitor(object tag, object data, string field, UnityAction callback) {
        List<DataMonitor> list = null;
        for(int i = list.Count - 1; i >= 0; i--) {
            if(list[i].data == data && list[i].field == field) {
                RemoveDataMonitor(list[i]);
                list.Remove(list[i]);
            }
        }
        DataMonitor dm = new DataMonitor(data, field, callback);
        AddDataMonitor(dm);
        list.Add(dm);
        return dm;
    }

    public static void RemoveDataMonitor(DataMonitor dm) {
        if(dm == null) {
            return;
        }
        Dictionary<string, List<DataMonitor>> ms;
        if(!dataMonitor.TryGetValue(dm.data, out ms)) {
            return;
        }
        List<DataMonitor> ds;
        if(!ms.TryGetValue(dm.field, out ds)) {
            return;
        }
        ds.Remove(dm);
        if(ds.Count == 0) {
            ms.Remove(dm.field);
            if(ms.Count == 0) {
                dataMonitor.Remove(dm.data);
            }
        }
    }

    public static void DebugShowDatamoniter() {
        Debug.LogError("Count:" + dataMonitor.Count);
        foreach(var kv in dataMonitor) {
            Debug.LogError("KEY: " + kv.Key + kv.Key.GetHashCode());
        }
    }

    public static void FireDataChanged() {
        if(dataChanged.Count == 0) {
            return;
        }
        foreach(var dm in dataChanged) {
            if(dm != null)
                dm.callback();
        }
    }


    public static T ToObj<T>(BonValue v, T old, HashSet<string> fields = null) {
        return (T)ToObj(v, typeof(T), old, fields);
    }

    public static object ToObj(BonValue v, Type t, object old, HashSet<string> fields = null) {
        if(v == null) return null;
        switch(t.Name) {
            case "Byte": return (byte)v.AsInt;
            case "SByte": return (sbyte)v.AsInt;
            case "Int16": return (short)v.AsInt;
            case "UInt16": return (ushort)v.AsInt;
            case "Int32": return v.AsInt;
            case "UInt32": return (uint)v.AsInt;
            case "Int64": return v.AsLong;
            case "UInt64": return (ulong)v.AsLong;
            case "Single": return v.AsFloat;
            case "Double": return v.AsDouble;
            case "Boolean": return v.AsBoolean;
            case "String": return v.AsString;
            case "Byte[]": return v.AsBinary;
            case "List`1": {
                BonArray arr = v.AsBonArray;
                if(arr == null) {
                    return null;
                }
                int num = arr.Count;
                IList l = null;
                if(old != null) {
                    l = (IList)old;
                } else {
                    l = (IList)Activator.CreateInstance(t, num);
                }
                Type t2 = t.GetGenericArguments()[0];
                l.Clear();
                for(int i = 0; i < num; i++) {
                    l.Add(ToObj(arr[i], t2, null, fields));
                }
                return l;
            }
            case "Dictionary`2": {
                BonDocument doc = v.AsBonDocument;
                if(doc == null) {
                    return null;
                }
                int num = doc.Count;
                IDictionary d = null;
                if(old != null) {
                    d = (IDictionary)old;
                } else {
                    d = (IDictionary)Activator.CreateInstance(t, num);
                }
                Type[] t2s = t.GetGenericArguments();
                Type tk = t2s[0];
                Type t2 = t2s[1];
                for(int i = 0; i < num; i++) {
                    BonElement el = doc[i];
                    object key = null;
                    switch(tk.Name) {
                        case "Int32": key = Convert.ToInt32(el.name); break;
                        case "Int64": key = Convert.ToInt64(el.name); break;
                        case "String": key = el.name; break;
                        default: {
                            if(tk.IsEnum) {
                                key = Enum.ToObject(tk, Convert.ToInt32(el.name));
                            }
                            break;
                        }
                    }
                    if(key != null) {
                        BonValue v2 = el.value;
                        object obj = null;
                        if(d.Contains(key)) {
                            obj = ToObj(v2, t2, d[key], fields);
                        } else {
                            obj = ToObj(v2, t2, null, fields);
                        }
                        if(obj == null) {
                            d.Remove(key);
                        } else {
                            d[key] = obj;
                        }
                    }
                }
                return d;
            }
            default: {
                if(t.IsEnum) {
                    return Enum.ToObject(t, v.AsInt);
                }
                if(t.IsArray) {
                    BonArray arr = v.AsBonArray;
                    if(arr == null) {
                        return null;
                    }
                    int num = arr.Count;
                    Type t2 = t.GetElementType();
                    var obj = Array.CreateInstance(t2, num);
                    for(int i = 0; i < num; i++) {
                        obj.SetValue(ToObj(arr[i], t2, null, fields), i);
                    }
                    return obj;
                }
                if(!v.IsBonDocument) {
                    return null;
                }
                {
                    BonDocument doc = v.AsBonDocument;
                    string _t_ = doc.GetString("_t_");
                    if(_t_ != null) {
                        try {
                            t = Type.GetType(_t_);
                        } catch(Exception) {
                            Debug.LogWarning("Î´ÕÒµ½ÀàÐÍ: " + doc["_t_"].AsString);
                            return null;
                        }
                    }
                    ClassInfo ci = ClassInfo.Get(t);
                    object obj = old;
                    Dictionary<string, List<DataMonitor>> dataMonitors = null;
                    bool monitorObj = false;
                    if(old != null) {
                        monitorObj = dataMonitor.TryGetValue(old, out dataMonitors);
                    }
                    if(obj == null) {
                        obj = Activator.CreateInstance(t);
                    }
                    if(obj is IBon) {
                        ((IBon)obj).FromBon(doc);
                        return obj;
                    }
                    CachedDictionary<string, FldInfo> fis = ci.fields;
                    bool isValueType = t.IsValueType;
                    int num = doc.Count;
                    for(int i = 0; i < num; i++) {
                        BonElement el = doc[i];
                        if(fields != null && !fields.Contains(el.name)) {
                            continue;
                        }
                        FldInfo fi;
                        if(fis.TryGetValue(el.name, out fi)) {
                            List<DataMonitor> fieldMonitors = null;
                            bool monitorField = monitorObj && dataMonitors.TryGetValue(fi.name, out fieldMonitors) && fieldMonitors.Count > 0;
                            if((fi.type.IsValueType || fi.type == typeof(string) || fi.type.IsEnum) && !monitorField) {
                                fi.SetValue(obj, ToObj(el.value, fi.type, null, fi.subFields));
                            } else {
                                object oldv = fi.GetValue(obj);
                                object newv = ToObj(el.value, fi.type, oldv, fi.subFields);
                                fi.SetValue(obj, newv);
                                if(monitorField && (fi.type.IsClass || oldv != newv)) {
                                    dataChanged.AddRange(fieldMonitors);
                                }
                            }
                        }
                    }
                    return obj;
                }
            }
        }
    }

    public static BonValue ToBon(object obj, HashSet<string> fields = null, Type declareType = null) {
        if(obj == null) {
            return BonNull.value;
        }
        Type t = obj.GetType();
        switch(t.Name) {
            case "Byte": return (int)(byte)obj;
            case "SByte": return (int)(sbyte)obj;
            case "Int16": return (int)(short)obj;
            case "UInt16": return (int)(ushort)obj;
            case "Int32": return (int)obj;
            case "UInt32": return (int)(uint)obj;
            case "Int64": return (long)obj;
            case "UInt64": return (long)(ulong)obj;
            case "Single": return (float)obj;
            case "Double": return (double)obj;
            case "Boolean": return (bool)obj;
            case "String": return (string)obj;
            case "Byte[]": return (byte[])obj;
            default: {
                if(t.IsEnum) {
                    return (int)obj;
                }
                break;
            }
        }

        switch(t.Name) {
            case "List`1": {
                Type et = t.GetGenericArguments()[0];
                BonArray arr = null;
                arr = new BonArray();
                IList list = (IList)obj;
                int num = list.Count;
                for(int i = 0; i < num; i++) {
                    arr.Add(ToBon(list[i], fields, et));
                }
                return arr;
            }
            case "Dictionary`2": {
                Type et = t.GetGenericArguments()[1];
                BonDocument doc = null;
                doc = new BonDocument();
                foreach(DictionaryEntry kv in (IDictionary)obj) {
                    if(kv.Key.GetType().IsEnum) {
                        doc[((int)kv.Key).ToString()] = ToBon(kv.Value, fields, et);
                    } else {
                        doc[kv.Key.ToString()] = ToBon(kv.Value, fields, et);
                    }
                }
                return doc;
            }
            default: {
                if(t.IsArray) {
                    Type et = t.GetElementType();
                    BonArray arr = null;
                    arr = new BonArray();
                    Array list = (Array)obj;
                    int num = list.Length;
                    for(int i = 0; i < num; i++) {
                        arr.Add(ToBon(list.GetValue(i), fields, et));
                    }
                    return arr;
                }
                {
                    if(obj is IBon) {
                        return ((IBon)obj).ToBon();
                    }
                    ClassInfo ci = ClassInfo.Get(t);
                    BonDocument doc = new BonDocument();
                    if(declareType != null && declareType != t) {
                        doc["_t_"] = t.FullName;
                    }
                    FldInfo[] fis = ci.fields.Values;
                    for(int i = fis.Length; --i >= 0;) {
                        FldInfo fi = fis[i];
                        if(fields != null && !fields.Contains(fi.name)) {
                            continue;
                        }
                        doc[fi.name] = ToBon(fi.GetValue(obj), fi.subFields);
                    }
                    return doc;
                }
            }
        }
    }

    //public static byte[] ToBonData(object obj, HashSet<string> fields = null, Type declareType = null) {
    //    using(DataWriter dw = new DataWriter()) {
    //        _ToBonData(dw, obj, fields, declareType);
    //        return dw.ToArray();
    //    }
    //}

    //static void _ToBonData(DataWriter dw, object obj, HashSet<string> fields, Type declareType) {
    //    if(obj == null) {
    //        dw.Write((byte)BonValueType.Null);
    //        return;
    //    }
    //    Type t = obj.GetType();
    //    switch(t.Name) {
    //        case "Byte":
    //        case "SByte":
    //        case "Int16":
    //        case "UInt16":
    //        case "Int32":
    //        case "UInt32": {
    //            dw.Write((byte)BonValueType.Int);
    //            dw.Write(Convert.ToInt32(obj));
    //            return;
    //        }

    //        case "Int64":
    //        case "UInt64": {
    //            dw.Write((byte)BonValueType.Long);
    //            dw.Write(Convert.ToInt64(obj));
    //            return;
    //        }
    //        case "Single": {
    //            dw.Write((byte)BonValueType.Float);
    //            dw.Write((float)obj);
    //            return;
    //        }
    //        case "Double": {
    //            dw.Write((byte)BonValueType.Double);
    //            dw.Write((double)obj);
    //            return;
    //        }
    //        case "Boolean": {
    //            dw.Write((byte)BonValueType.Boolean);
    //            dw.Write((bool)obj);
    //            return;
    //        }
    //        case "String": {
    //            dw.Write((byte)BonValueType.String);
    //            dw.Write((string)obj);
    //            return;
    //        }
    //        case "Byte[]": {
    //            dw.Write((byte)BonValueType.Binary);
    //            byte[] data = (byte[])obj;
    //            dw.Write7BitEncodedInt(data.Length);
    //            dw.Write(data);
    //            return;
    //        }
    //        default: {
    //            if(t.IsEnum) {
    //                dw.Write((byte)BonValueType.Int);
    //                dw.Write((int)obj);
    //                return;
    //            }
    //            break;
    //        }
    //    }

    //    switch(t.Name) {
    //        case "List`1": {
    //            dw.Write((byte)BonValueType.Array);
    //            Type et = t.GetGenericArguments()[0];
    //            IList list = (IList)obj;
    //            dw.Write7BitEncodedInt(list.Count);
    //            int n = list.Count;
    //            for(int i = 0; i < n; i++) {
    //                _ToBonData(dw, list[i], fields, et);
    //            }
    //            return;
    //        }
    //        case "Dictionary`2": {
    //            dw.Write((byte)BonValueType.Document);
    //            Type kt = t.GetGenericArguments()[0];
    //            Type et = t.GetGenericArguments()[1];
    //            IDictionary map = (IDictionary)obj;
    //            dw.Write7BitEncodedInt(map.Count);
    //            foreach(DictionaryEntry kv in map) {
    //                if(kt.IsEnum) {
    //                    dw.Write(((int)kv.Key).ToString());
    //                } else {
    //                    dw.Write(kv.Key.ToString());
    //                }
    //                _ToBonData(dw, kv.Value, fields, et);
    //            }
    //            return;
    //        }
    //        default: {
    //            dw.Write((byte)BonValueType.Document);
    //            ClassInfo ci = ClassInfo.Get(t);
    //            int num = 0;
    //            bool wt = declareType != null && declareType != t;
    //            if(wt) {
    //                num++;
    //            }
    //            if(fields != null) {
    //                num += ci.fields.Keys.Count(k => fields.Contains(k));
    //            }
    //            dw.Write7BitEncodedInt(num);
    //            if(wt) {
    //                dw.Write("_t_");
    //                dw.Write(t.FullName);
    //            }
    //            foreach(KeyValuePair<string, FldInfo> kv in ci.fields) {
    //                if(fields != null && !fields.Contains(kv.Key)) {
    //                    continue;
    //                }
    //                dw.Write(kv.Key);
    //                _ToBonData(dw, kv.Value.GetValue(obj), kv.Value.subFields, null);
    //            }
    //            return;
    //        }
    //}
    //    }
}
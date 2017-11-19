using System;
using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Linq;
using System.Reflection;

/// <summary>
/// 精简的二进制序列化类
/// </summary>
public class BinarySerialize {
    private static readonly Type[] basicTypes = new Type[] {
        //0~9
        null,//for null
        null,//for ref
        typeof(object),
        typeof(int),
        typeof(long),
        typeof(float),
        typeof(double),
        typeof(string),
        typeof(Vector2),
        typeof(Vector3),
        //10~19
        typeof(Quaternion),
        typeof(uint),
        typeof(ulong),
        typeof(byte),
        typeof(sbyte),
        typeof(DateTime),
        typeof(short),
        typeof(ushort),
        typeof(bool),
        null,
        //20~29
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        //30~39
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        //40~49
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        null,
    };
    const byte TYPE_NULL = 0;
    const byte TYPE_REF = 1;

    private class WriteArg {
        public DataWriter bw;
        private Dictionary<Type, int> typeMap = new Dictionary<Type, int>();
        public List<Type> typeRef;
        public Dictionary<object, int> objRef = new Dictionary<object, int>();
        public WriteArg(DataWriter bw) {
            this.bw = bw;
            typeRef = new List<Type>(basicTypes);
            for(int i = 0; i < basicTypes.Length; i++) {
                if(basicTypes[i] == null) {
                    continue;
                }
                typeMap[basicTypes[i]] = i;
            }
        }

        public int AddType(Type t) {
            int r;
            if(!typeMap.TryGetValue(t, out r)) {
                r = typeRef.Count;
                typeMap[t] = r;
                typeRef.Add(t);
            }
            return r;
        }
    }
    public static byte[] Serialize(object obj) {
        MemoryStream ms = new MemoryStream();
        using(DataWriter bw = new DataWriter(ms)) {
            WriteArg arg = new WriteArg(bw);
            bw.Write(0);
            WriteObj(obj, true, arg);
            if(arg.typeRef.Count > basicTypes.Length) {
                bw.Seek(0, SeekOrigin.Begin);
                bw.Write((int)bw.BaseStream.Length - sizeof(int));
                bw.Seek(0, SeekOrigin.End);
                bw.Write(arg.typeRef.Count - basicTypes.Length);
                for(int i = basicTypes.Length; i < arg.typeRef.Count; i++) {
                    bw.Write(arg.typeRef[i].FullName);
                }
            }
            return ms.ToArray();
        }
    }

    private static void WriteObj(object obj, bool writeType, WriteArg arg) {
        DataWriter bw = arg.bw;
        if(obj == null) {
            bw.Write(TYPE_NULL);
            return;
        }
        Type t = obj.GetType();
        if(t.IsClass) {
            int r;
            if(arg.objRef.TryGetValue(obj, out r)) {
                bw.Write(TYPE_REF);
                bw.Write7BitEncodedInt(r);
                return;
            } else {
                arg.objRef[obj] = arg.objRef.Count;
            }
        }
        if(writeType) {
            bw.Write7BitEncodedInt(arg.AddType(t));
        }
        switch(t.Name) {
            case "Int32": bw.Write((int)obj); break;
            case "Int64": bw.Write((long)obj); break;
            case "Single": bw.Write((float)obj); break;
            case "Double": bw.Write((double)obj); break;
            case "String": bw.Write((string)obj); break;
            case "UInt32": bw.Write((uint)obj); break;
            case "UInt64": bw.Write((ulong)obj); break;
            case "Byte": bw.Write((byte)obj); break;
            case "SByte": bw.Write((sbyte)obj); break;
            case "Int16": bw.Write((short)obj); break;
            case "UInt16": bw.Write((ushort)obj); break;
            case "Boolean": bw.Write((bool)obj); break;
            case "DateTime": bw.Write(((DateTime)obj).ToBinary()); break;
            case "Vector2": {
                Vector2 v2 = (Vector2)obj;
                bw.Write(v2.x);
                bw.Write(v2.y);
                break;
            }
            case "Vector3": {
                Vector3 v3 = (Vector3)obj;
                bw.Write(v3.x);
                bw.Write(v3.y);
                bw.Write(v3.z);
                break;
            }
            default: {
                if(t.IsEnum) {
                    bw.Write((int)obj);
                    return;
                }
                if(t.IsArray) {
                    Array arr = (Array)obj;
                    Type t2 = t.GetElementType();
                    bw.Write7BitEncodedInt(arr.Length);
                    foreach(object obj2 in arr) {
                        WriteObj(obj2, t2.IsClass, arg);
                    }
                    return;
                }
                if(typeof(IList).IsAssignableFrom(t)) {
                    IList list = (IList)obj;
                    Type t2 = null;
                    if(t.IsGenericType) {
                        t2 = t.GetGenericArguments()[0];
                    }
                    bw.Write7BitEncodedInt(list.Count);
                    bool wt = t2 == null || t2.IsClass;
                    foreach(object obj2 in list) {
                        WriteObj(obj2, wt, arg);
                    }
                    return;
                }
                if(typeof(IDictionary).IsAssignableFrom(t)) {
                    Type tk = null;
                    Type tv = null;
                    if(t.IsGenericType) {
                        Type[] ts = t.GetGenericArguments();
                        tk = ts[0];
                        tv = ts[1];
                    }
                    IDictionary dic = (IDictionary)obj;
                    bw.Write7BitEncodedInt(dic.Count);
                    bool wtk = tk == null || tk.IsClass;
                    bool wtv = tv == null || tv.IsClass;
                    foreach(DictionaryEntry kv in dic) {
                        WriteObj(kv.Key, wtk, arg);
                        WriteObj(kv.Value, wtv, arg);
                    }
                    return;
                }
                {
                    ClassInfo ci = ClassInfo.Get(t);
                    bw.Write7BitEncodedInt(ci.fields.Count);
                    foreach(FldInfo fi in ci.fields.Values) {
                        WriteObj(fi.name, true, arg);
                        WriteObj(fi.GetValue(obj), true, arg);
                    }
                }
                break;
            }
        }
    }

    private class ReadArg {
        public DataReader br;
        public List<Type> typeRef;
        public List<object> objRef = new List<object>();
        public ReadArg(DataReader br) {
            this.br = br;
            typeRef = new List<Type>(basicTypes);
        }
    }

    public static object Deserialize(byte[] data) {
        MemoryStream ms = new MemoryStream(data);
        using(DataReader br = new DataReader(ms)) {
            ReadArg arg = new ReadArg(br);
            int offset = br.ReadInt32();
            if(offset > 0) {
                br.BaseStream.Seek(offset, SeekOrigin.Current);
                int num = br.ReadInt32();
                arg.typeRef.Capacity = arg.typeRef.Count + num;
                while(num > 0) {
                    string ts = br.ReadString();
                    Type t = Type.GetType(ts);
                    if(t == null) {
                        Debug.LogError("未找到类型" + ts);
                        return null;
                    }
                    arg.typeRef.Add(t);
                    num--;
                }
                br.BaseStream.Seek(sizeof(int), SeekOrigin.Begin);
            }
            try {
                return ReadObj(null, arg);
            } catch(Exception e) {
                Debug.LogError("反序列化出错" + e);
                return null;
            }
        }
    }

    private static object ReadObj(Type t, ReadArg arg) {
        DataReader br = arg.br;
        if(t == null) {
            int ti = br.Read7BitEncodedInt();
            switch(ti) {
                case TYPE_NULL: return null;
                case TYPE_REF: return arg.objRef[br.Read7BitEncodedInt()];
            }
            t = arg.typeRef[ti];
        }
        switch(t.Name) {
            case "Int32": return br.ReadInt32();
            case "Int64": return br.ReadInt64();
            case "Single": return br.ReadSingle();
            case "Double": return br.ReadDouble();
            case "String": string s = br.ReadString(); arg.objRef.Add(s); return s;
            case "UInt32": return br.ReadUInt32();
            case "UInt64": return br.ReadUInt64();
            case "Byte": return br.ReadByte();
            case "SByte": return br.ReadSByte();
            case "Int16": return br.ReadInt16();
            case "UInt16": return br.ReadUInt16();
            case "Boolean": return br.ReadBoolean();
            case "DateTime": return DateTime.FromBinary(br.ReadInt64());
            case "Vector2": return new Vector2(br.ReadSingle(), br.ReadSingle());
            case "Vector3": return new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
            default: {
                if(t.IsEnum) {
                    return Enum.ToObject(t, br.ReadInt32());
                }
                if(t.IsArray) {
                    Type t2 = t.GetElementType();
                    int num = br.Read7BitEncodedInt();
                    Array arr = Array.CreateInstance(t2, num);
                    arg.objRef.Add(arr);
                    Type rt = t2.IsClass ? null : t2;
                    for(int i = 0; i < num; i++) {
                        arr.SetValue(ReadObj(rt, arg), i);
                    }
                    return arr;
                }
                if(typeof(IList).IsAssignableFrom(t)) {
                    IList list = (IList)Activator.CreateInstance(t);
                    arg.objRef.Add(list);
                    Type t2 = null;
                    if(t.IsGenericType) {
                        t2 = t.GetGenericArguments()[0];
                        if(t2.IsClass) {
                            t2 = null;
                        }
                    }
                    int num = br.Read7BitEncodedInt();
                    for(int i = 0; i < num; i++) {
                        list.Add(ReadObj(t2, arg));
                    }
                    return list;
                }
                if(typeof(IDictionary).IsAssignableFrom(t)) {
                    IDictionary dic = (IDictionary)Activator.CreateInstance(t);
                    arg.objRef.Add(dic);
                    Type tk = null;
                    Type tv = null;
                    if(t.IsGenericType) {
                        Type[] ts = t.GetGenericArguments();
                        tk = ts[0];
                        tv = ts[1];
                        if(tk.IsClass) {
                            tk = null;
                        }
                        if(tv.IsClass) {
                            tv = null;
                        }
                    }
                    int num = br.Read7BitEncodedInt();
                    for(int i = 0; i < num; i++) {
                        object key = ReadObj(tk, arg);
                        object value = ReadObj(tv, arg);
                        dic[key] = value;
                    }
                    return dic;
                }
                {
                    object obj = Activator.CreateInstance(t);
                    if(t.IsClass) {
                        arg.objRef.Add(obj);
                    }
                    ClassInfo ci = ClassInfo.Get(t);
                    int num = br.Read7BitEncodedInt();
                    for(int i = 0; i < num; i++) {
                        string name = (string)ReadObj(null, arg);
                        object v = ReadObj(null, arg);
                        FldInfo fi;
                        if(ci.fields.TryGetValue(name, out fi)) {
                            if(v == null) {
                                if(fi.type.IsClass) {
                                    fi.SetValue(obj, null);
                                }
                            } else {
                                Type vt = v.GetType();
                                if(fi.type.IsEnum && vt == typeof(int) || fi.type.IsAssignableFrom(vt)) {
                                    fi.SetValue(obj, v);
                                }
                            }
                        }
                    }
                    return obj;
                }
            }
        }
    }
}

/// <summary>
/// 系统默认的序列化
/// </summary>
public class BinarySerialize2 {
    class Vector3Surrogate: ISerializationSurrogate {
        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context) {
            Vector3 v3 = (Vector3)obj;
            info.AddValue("x", v3.x);
            info.AddValue("y", v3.y);
            info.AddValue("z", v3.z);
        }

        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector) {
            Vector3 v3 = (Vector3)obj;
            v3.x = info.GetSingle("x");
            v3.y = info.GetSingle("y");
            v3.z = info.GetSingle("z");
            return v3;
        }
    }

    private static BinaryFormatter _bf;
    private static BinaryFormatter bf {
        get {
            if(_bf == null) {
                SurrogateSelector ss = new SurrogateSelector();
                ss.AddSurrogate(typeof(Vector3), new StreamingContext(StreamingContextStates.All), new Vector3Surrogate());
                _bf = new BinaryFormatter();
                _bf.SurrogateSelector = ss;
            }
            return _bf;
        }
    }

    public static byte[] Serialize(object obj) {
        using(MemoryStream ms = new MemoryStream()) {
            bf.Serialize(ms, obj);
            return ms.ToArray();
        }
    }

    public static object Deserialize(byte[] data) {
        using(MemoryStream ms = new MemoryStream(data)) {
            return bf.Deserialize(ms);
        }
    }
}
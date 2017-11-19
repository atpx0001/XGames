using System;
using System.Collections.Generic;
using System.Reflection;
public interface IFastGetSet {
    void FastSetValue(string name, object value);
    object FastGetValue(string name);
}
public class ClassInfo {
    private static Dictionary<Type, ClassInfo> classes = new Dictionary<Type, ClassInfo>();

    public Type type;
    public CachedDictionary<string, FldInfo> fields = new CachedDictionary<string, FldInfo>();
    private MethodInfo readKeyFromBonMethod;

    public MethodInfo ReadKeyFromBonMethod {
        get {
            if(readKeyFromBonMethod == null) {
                readKeyFromBonMethod = type.GetMethod("ReadKeyFromBon", BindingFlags.Static | BindingFlags.Public);
            }
            return readKeyFromBonMethod;
        }
    }

    private ClassInfo(Type t) {
        this.type = t;
        FieldInfo[] fis = t.GetFields(BindingFlags.Public | BindingFlags.Instance);
        for(int i = fis.Length; --i >= 0;) {
            FldInfo fi = new FldInfo(fis[i]);
            fields[fi.name] = fi;
        }
    }

    public static ClassInfo Get(Type t) {
        ClassInfo ci;
        if(!classes.TryGetValue(t, out ci)) {
            ci = new ClassInfo(t);
            classes[t] = ci;
        }
        return ci;
    }
}

public class FldInfo {
    private System.Reflection.FieldInfo fi;
    public string name;
    public Type type;
    public Type[] geneicTypes;
    public HashSet<string> subFields;
    bool isFast;
    public FldInfo(System.Reflection.FieldInfo fi) {
        this.fi = fi;
        this.name = fi.Name;
        type = fi.FieldType;
        if(type.IsGenericType) {
            geneicTypes = type.GetGenericArguments();
        }
        object[] atts = fi.GetCustomAttributes(typeof(JsonFields), true);
        if(atts.Length > 0) {
            subFields = new HashSet<string>(((JsonFields)atts[0]).fields);
        }
        isFast = typeof(IFastGetSet).IsAssignableFrom(fi.DeclaringType);
    }

    public object GetValue(object obj) {
        if(isFast) {
            ((IFastGetSet)obj).FastGetValue(name);
        }
        return fi.GetValue(obj);
    }

    public void SetValue(object obj, object value) {
        if(isFast) {
            ((IFastGetSet)obj).FastSetValue(name, value);
            return;
        }
        fi.SetValue(obj, value);
    }
}

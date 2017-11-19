using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static class RoleStaticHolder {
    public static List<IRoleStatic> objs = new List<IRoleStatic>();
    public static void Reset() {
        foreach(var obj in objs) {
            obj.Reset();
        }
    }
}
public interface IRoleStatic {
    void Reset();
}
public class RoleStatic<T>: IRoleStatic {
    public T value;

    public void Reset() {
        this.value = default(T);
    }
}

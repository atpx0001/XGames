using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// 可被对象池缓存的类
/// </summary>
public interface IPoolableObj {
    /// <summary>
    /// 被对象池回收时清理工作
    /// </summary>
    void Release();
}

/// <summary>
/// 对象池
/// </summary>
public class ObjPool {
    public bool disposed = false;
    ~ObjPool() {
        disposed = true;
    }

#if !RELEASE
    struct Pinfo {
        public Type type;
        public int GetCount;
        public int NewCount;
        public int ReleaseCount;
        public int FreeObjCount;
    }
    static Dictionary<Type, Pinfo> countDict = new Dictionary<Type, Pinfo>();

    bool countPool = false;
#endif
    public void StartCountPool() {
#if !RELEASE
        countPool = true;
        countDict.Clear();
#endif
    }
    public void EndCountPool() {
#if !RELEASE
        countPool = false;
#endif
    }


    public void DebugLogPoolInfo() {
#if !RELEASE
        foreach(Pinfo p in countDict.Values) {
            Debug.Log(p.type.Name + " => GET:" + p.GetCount + " NEW:" + p.NewCount + " RELEASE:" + p.ReleaseCount + " FREEOBJ:" + p.FreeObjCount);
        }
#endif
    }
#if !RELEASE

    Pinfo GetPinfo(Type key) {
        Pinfo ret;
        if(!countDict.TryGetValue(key, out ret)) {
            ret = new Pinfo();
            ret.type = key;
            ret.GetCount = 0;
            ret.NewCount = 0;
            ret.ReleaseCount = 0;
            ret.FreeObjCount = 0;
            countDict[key] = ret;
        }
        return ret;
    }
    void AddGetCount(Type key, int val) {
        if(countPool) {
            Pinfo ret = GetPinfo(key);
            ret.GetCount += val;
            countDict[key] = ret;
        }
    }
    void AddNewCount(Type key, int val) {
        if(countPool) {
            Pinfo ret = GetPinfo(key);
            ret.NewCount += val;
            countDict[key] = ret;
        }
    }
    void AddReleaseCount(Type key, int val) {
        if(countPool) {
            Pinfo ret = GetPinfo(key);
            ret.ReleaseCount += val;
            countDict[key] = ret;
        }
    }
    void AddFreeObjCount(Type key, int val) {
        if(countPool) {
            Pinfo ret = GetPinfo(key);
            ret.FreeObjCount += val;
            countDict[key] = ret;
        }
    }
#endif

    public Dictionary<Type, Queue<IPoolableObj>> pools = new Dictionary<Type, Queue<IPoolableObj>>(100);
    /// <summary>
    /// 清空某种类型的对象池
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public void Clear<T>() {
        EnsurePool(typeof(T)).Clear();
    }
    /// <summary>
    /// 清空全部对象池
    /// </summary>
    public void ClearAll() {
        pools.Clear();
    }


    /// <summary>
    /// 获取一个新的对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T Get<T>() where T : IPoolableObj, new() {
#if !RELEASE
        AddGetCount(typeof(T), 1);
#endif
        Queue<IPoolableObj> pool = EnsurePool(typeof(T));
        if(pool.Count > 0) {
#if !RELEASE
            AddFreeObjCount(typeof(T), -1);
#endif
            return (T)pool.Dequeue();
        }
#if !RELEASE
        AddNewCount(typeof(T), 1);
#endif
        return new T();
        //return (T)Get(typeof(T));
    }

    //public IPoolableObj Get(Type t) {
    //    Queue<IPoolableObj> pool = EnsurePool(t);
    //    if(pool.Count > 0) {
    //        return pool.Dequeue();
    //    }
    //    return (IPoolableObj)Activator.CreateInstance(t);
    //}
    ///
    private Queue<IPoolableObj> EnsurePool(Type t) {
        Queue<IPoolableObj> pool;
        if(!pools.TryGetValue(t, out pool)) {
            pool = new Queue<IPoolableObj>(1000);
            pools[t] = pool;
        }
        return pool;
    }
    /// <summary>
    /// 释放一个对象
    /// </summary>
    /// <param name="v"></param>
    public void Release(IPoolableObj v) {
        v.Release();
        EnsurePool(v.GetType()).Enqueue(v);
#if !RELEASE
        AddReleaseCount(v.GetType(), 1);
        AddFreeObjCount(v.GetType(), 1);
#endif
    }
}


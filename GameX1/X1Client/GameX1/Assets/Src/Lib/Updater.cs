using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

public class PriorityEvent {
    private class Item {
        public Action action;
        public int priority;
        public Item(Action value, int priority) {
            this.priority = priority;
            this.action = value;
        }
    }

    private CachedList<Item> handles;
    private bool dirty = true;
    /// <summary>
    /// 
    /// </summary>
    /// <param name="handle"></param>
    /// <param name="priority">越大越先执行</param>
    public void Add(Action handle, int priority) {
        if(handles == null) {
            handles = new CachedList<Item>();
        }
        handles.Add(new Item(handle, priority));
        dirty = true;
    }
    public void Add(Action handle) {
        Add(handle, 0);
    }

    /// <summary>
    /// 粗暴删除，不匹配原始优先级
    /// </summary>
    /// <param name="handle"></param>
    public void Remove(Action handle) {
        if(handles != null) {
            int c = handles.Count;
            for(int i = 0; i < c; i++) {
                if(handles[i].action == handle || handles[i].action.Target == handle.Target && handles[i].action.Method == handle.Method) {
                    handles.RemoveAt(i);
                    break;
                }
            }
        }
    }

    /// <summary>
    /// 精确删除，匹配原始优先级
    /// </summary>
    /// <param name="handle"></param>
    /// <param name="priority"></param>
    public void Remove(Action handle, int priority) {
        if(handles != null) {
            int c = handles.Count;
            for(int i = 0; i < c; i++) {
                if(handles[i].action == handle || handles[i].action.Target == handle.Target && handles[i].action.Method == handle.Method && handles[i].priority == priority) {
                    handles.RemoveAt(i);
                    break;
                }
            }
        }
    }

    public void Clear() {
        if(handles != null) {
            handles.Clear();
        }
    }

    public void Invoke() {
        if(handles != null && handles.Count > 0) {
            if(dirty) {
                handles.Sort((x, y) => y.priority - x.priority);
                dirty = false;
            }
            Item[] cachedHandles = handles.ToArray();
            for(int i = 0; i < cachedHandles.Length; i++) {
                cachedHandles[i].action();
            }
        }
    }
}

/// <summary>
/// 供全局使用的更新调用器，可以在这里注册回调，一般用于需要每帧update的单例类
/// </summary>
public class Updater {
    private static Updater instance;
    /// <summary>
    /// 在所有update之前调用
    /// </summary>
    public PriorityEvent OnPreUpdate = new PriorityEvent();
    /// <summary>
    /// 在所有update之后调用
    /// </summary>
    public PriorityEvent OnPostUpdate = new PriorityEvent();
    /// <summary>
    /// 在所有OnDestroy之前调用，一般不用
    /// </summary>
    public PriorityEvent OnPreDestroy = new PriorityEvent();
    /// <summary>
    /// 在所有OnDestroy之后调用
    /// </summary>
    public PriorityEvent OnPostDestroy = new PriorityEvent();
    /// <summary>
    /// 在所有FixedUpdate之前调用
    /// </summary>
    public PriorityEvent OnPreFixedUpdate = new PriorityEvent();
    /// <summary>
    /// 在所有FixedUpdate之后调用
    /// </summary>
    public PriorityEvent OnPostFixedUpdate = new PriorityEvent();

    public SafedQueue<UnityAction> actions = new SafedQueue<UnityAction>();

    public static Updater Instance {
        get {
            if(instance == null) {
#if UNITY_EDITOR
                if(!UnityEditor.EditorApplication.isPlaying) {
                    return null;
                }
#endif
                instance = new Updater();
                GameObject go = GameObject.Find("updater");
                PreUpdater pre = null;
                PostUpdater post = null;
                if(go == null) {
                    go = new GameObject("updater");
                    pre = go.AddComponent<PreUpdater>();
                    post = go.AddComponent<PostUpdater>();
                } else {
                    pre = go.GetComponent<PreUpdater>();
                    post = go.GetComponent<PostUpdater>();
                }
                pre.updater = instance;
                post.updater = instance;
                instance.proxy = post;
                GameObject.DontDestroyOnLoad(go);
                //if(StaticRoot.Instance.root != null) {
                //    go.transform.parent = StaticRoot.Instance.root;
                //}
            }
            return instance;
        }
    }

    void Update() {
        UnityAction action;
        while(actions.TryDequeue(out action)) {
            action();
        }
    }

    /// <summary>
    /// 一个可以用来启动协程的代理，如果已被销毁，该值为null，使用前注意判断
    /// </summary>
    public PostUpdater proxy;

    public void PreDestroy() {
        OnPreDestroy.Invoke();
        OnPreUpdate.Clear();
        OnPreDestroy.Clear();
    }

    public void PostDestroy() {
        OnPostDestroy.Invoke();
        OnPostUpdate.Clear();
        OnPostDestroy.Clear();
    }

    public void PreUpdate() {
        OnPreUpdate.Invoke();
    }

    public void PostUpdate() {
        OnPostUpdate.Invoke();
        Update();
    }

    public void PreFixedUpdate() {
        OnPreFixedUpdate.Invoke();
    }

    public void PostFixedUpdate() {
        OnPostFixedUpdate.Invoke();
    }
}
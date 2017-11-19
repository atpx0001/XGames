using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// 可以在遍历中增减元素的字典
/// </summary>
/// <typeparam name="TK"></typeparam>
/// <typeparam name="TV"></typeparam>
public class CachedDictionary<TK, TV>: IEnumerable<KeyValuePair<TK, TV>> {
    private Dictionary<TK, TV> dic;
    private KeyValuePair<TK,TV>[] cache;
    private TV[] valueCache;

    public CachedDictionary() {
        dic = new Dictionary<TK, TV>();
    }

    public CachedDictionary(int c) {
        dic = new Dictionary<TK, TV>(c);
    }

    public int Count {
        get {
            return dic.Count;
        }
    }

    public TV this[TK key] {
        get {
            return dic[key];
        }
        set {
            dic[key] = value;
            cache = null;
            valueCache = null;
        }
    }

    public void Add(TK key, TV value) {
        dic.Add(key, value);
        cache = null;
        valueCache = null;
    }
    public void Clear() {
        dic.Clear();
        cache = null;
        valueCache = null;
    }
    public bool ContainsKey(TK key) {
        return dic.ContainsKey(key);
    }
    public bool ContainsValue(TV value) {
        return dic.ContainsValue(value);
    }

    public bool Remove(TK key) {
        if(dic.Remove(key)) {
            cache = null;
            valueCache = null;
            return true;
        }
        return false;
    }
    public bool TryGetValue(TK key, out TV value) {
        return dic.TryGetValue(key, out value);
    }

    public KeyValuePair<TK, TV>[] ToArray() {
        if(cache == null) {
            cache = dic.ToArray();
        }
        return cache;
    }

    public TV[] Values {
        get {
            if(valueCache == null) {
                valueCache = dic.Values.ToArray();
            }
            return valueCache;
        }
    }

    public IEnumerator<KeyValuePair<TK, TV>> GetEnumerator() {
        return (IEnumerator<KeyValuePair<TK, TV>>)ToArray().GetEnumerator();
    }


    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
        return ToArray().GetEnumerator();
    }

}

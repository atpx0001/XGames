using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// 可以在遍历中增减元素的列表
/// </summary>
/// <typeparam name="T"></typeparam>
public class CachedList<T> : IEnumerable<T> {
    private List<T> list = new List<T>();
    private T[] cache;

    public int Count {
        get {
            return list.Count;
        }
    }

    public T this[int index] {
        get {
            return list[index];
        }
        set {
            list[index] = value;
            cache = null;
        }
    }

    public void Add(T item) {
        list.Add(item);
        cache = null;
    }

    public bool Remove(T item) {
        if(list.Remove(item)) {
            cache = null;
            return true;
        }
        return false;
    }

    public void RemoveAt(int index) {
        list.RemoveAt(index);
        cache = null;
    }

    public void Clear() {
        list.Clear();
        cache = null;
    }

    public void Sort(Comparison<T> comparison) {
        list.Sort(comparison);
        cache = null;
    }

    public T[] ToArray() {
        if(cache == null) {
            cache = list.ToArray();
        }
        return cache;
    }

    public IEnumerator<T> GetEnumerator() {
        return (IEnumerator<T>)ToArray().GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
        return ToArray().GetEnumerator();
    }
}

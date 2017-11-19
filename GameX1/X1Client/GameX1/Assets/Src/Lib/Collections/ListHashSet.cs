using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;

/// <summary>
/// 保持列表内元素唯一,Add/AddRange操作速度较慢
/// </summary>
/// <typeparam name="T"></typeparam>
public class ListHashSet<T>: IEnumerable<T> {
    List<T> _list = new List<T>();
    public bool Add(T item) {
        if(_list.Contains(item))
            return false;
        _list.Add(item);
        return true;
    }

    public bool Remove(T item) {
        return _list.Remove(item);
    }
    public void Clear() {
        _list.Clear();
    }
    public void RemoveAt(int index) {
        _list.RemoveAt(index);
    }


    public void AddRange(IEnumerable<T> collection) {
        foreach(T item in collection) {
            Add(item);
        }
    }

    public IEnumerator<T> GetEnumerator() {
        return _list.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return _list.GetEnumerator();
    }

    public T this[int index] {
        get {
            return _list[index];
        }
        set {
            _list[index] = value;
        }
    }

    public int Count {
        get {
            return _list.Count;
        }
    }
}

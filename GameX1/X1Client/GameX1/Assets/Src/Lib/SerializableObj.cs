using UnityEngine;
using System.Collections;
using System;

public class SerializableObj: ScriptableObject, ISerializationCallbackReceiver {
    public byte[] data;
    public object obj;

    public T As<T>() where T: class {
        return obj as T;
    }

    public void OnAfterDeserialize() {
        if(data == null || data.Length == 0) {
            obj = null;
        } else {
            try {
                obj = BinarySerialize2.Deserialize(data);
            } catch(Exception e) {
                Debug.Log(e);
                obj = null;
            }
        }
        data = null;
    }

    public void OnBeforeSerialize() {
        if(obj == null) {
            data = null;
        } else {
            data = BinarySerialize2.Serialize(obj);
        }
    }
}

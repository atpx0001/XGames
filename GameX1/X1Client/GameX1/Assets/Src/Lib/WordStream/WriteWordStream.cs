#if UNITY_EDITOR

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class WriteWordStream {
    bool withNote;

    List<string> words = new List<string>();

    public int level;

    public WriteWordStream(bool withNote) {
        this.withNote = withNote;
    }

    public void Save(string path) {
        File.WriteAllLines(path, words.ToArray());
    }

    public void Write(string value, string note) {
        words.Add(WithNote(value, note));
    }

    public void Write(int value, string note) {
        words.Add(WithNote(value.ToString(), note));
    }

    public void Write(float value, string note) {
        words.Add(WithNote(value.ToString(), note));
    }

    public void Write(bool value, string note) {
        words.Add(WithNote(value.ToString(), note));
    }

    public string WithNote(string value, string note) {
        if(!withNote) {
            return value;
        }
        while(true) {
            string tempNote = note.Replace("//", "");
            if(tempNote == note) {
                break;
            }
            note = tempNote;
        };
        if(note != "") {
            return new string(' ', level * 2) + value + " //" + note.Trim();
        }
        return new string(' ', level * 2) + value;
    }
}

#endif
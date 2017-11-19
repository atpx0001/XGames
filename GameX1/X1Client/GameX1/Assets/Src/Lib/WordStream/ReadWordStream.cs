using UnityEngine;
using System.Collections;
using System.IO;

public class ReadWordStream {
    string[] words;
    int pos;

    public ReadWordStream(string path) {
        Open(path);
    }
    public void Open(string path) {
        TextAsset txt = Res2.Load<TextAsset>(path);
        if(txt != null) {
            string aw = txt.ToString();
            words = aw.Split('\n');
        } else {
            words = new string[0];
        }
     
        //删除注释
        for(int i = 0; i < words.Length; i++) {
            int tempIndedx = words[i].LastIndexOf("//");
            if(tempIndedx >= 0) {
                words[i] = words[i].Substring(0, tempIndedx);
            }
            words[i] = words[i].Trim();
        }

        pos = 0;
    }

    public string ReadString() {
        return pos >= words.Length ? null : words[pos++];
    }

    public int ReadInt() {
        return pos >= words.Length ? 0 : int.Parse(words[pos++]);
    }

    public float ReadFloat() {
        return pos >= words.Length ? 0 : float.Parse(words[pos++]);
    }

    public bool ReadBool() {
        return pos >= words.Length ? false : bool.Parse(words[pos++]);
    }

    public bool IsEnd() {
        return pos >= words.Length;
    }
}
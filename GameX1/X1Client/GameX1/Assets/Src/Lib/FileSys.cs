using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System;


public enum DLCType {
    Static,//登陆时更新
    Dynamic,//动态更新
}

public class PackageDesc {
    public string name;
    public DLCType dlcType = DLCType.Static;
    public Dictionary<string, FileDesc> files = new Dictionary<string, FileDesc>();
    public string[] dependences;

    public void FromJson(BonDocument json) {
        foreach(BonElement p in json) {
            switch(p.name) {
                case "dlcType": {
                    dlcType = (DLCType)p.value.AsInt;
                    break;
                }
                case "files": {
                    foreach(BonElement pp in p.value.AsBonDocument) {
                        FileDesc fd = new FileDesc();
                        fd.name = pp.name;
                        fd.pak = this;
                        files[fd.name] = fd;
                        fd.FromJson(pp.value.AsBonDocument);
                    }
                    break;
                }
                case "dependences": {
                    BonArray arr = p.value.AsBonArray;
                    dependences = new string[arr.Count];
                    for(int i = dependences.Length; --i >= 0; ) {
                        dependences[i] = arr[i].AsString;
                    }
                    break;
                }
            }
        }
    }

    public BonDocument ToJson() {
        BonDocument json = new BonDocument();
        json["dlcType"] = (int)dlcType;
        BonDocument files = new BonDocument();
        json["files"] = files;
        foreach(KeyValuePair<string, FileDesc> p in this.files) {
            files[p.Key] = p.Value.ToJson();
        }
        if(dependences != null) {
            BonArray dps = new BonArray();
            foreach(string d in dependences) {
                dps.Add(d);
            }
            json["dependences"] = dps;
        }
        return json;
    }
}

public class FileDesc {
    public PackageDesc pak;
    public string name;
    public int crc;

    public void FromJson(BonDocument json) {
        foreach(BonElement p in json) {
            switch(p.name) {
                case "crc": {
                    crc = p.value.AsInt;
                    break;
                }
            }
        }
    }

    public BonDocument ToJson() {
        BonDocument json = new BonDocument();
        json["crc"] = crc;
        return json;
    }
}

public class FileSys {
    public Dictionary<string, PackageDesc> packages = new Dictionary<string, PackageDesc>();

    private Dictionary<string, FileDesc> fileIndex;

    public FileDesc GetFileDesc(string resName) {
        if(fileIndex == null) {
            fileIndex = new Dictionary<string, FileDesc>();
        }
        FileDesc file;
        if(fileIndex.TryGetValue(resName, out file)) {
            return file;
        }
        foreach(PackageDesc pak in packages.Values) {
            if(pak.files.TryGetValue(resName, out file)) {
                fileIndex[resName] = file;
                return file;
            }
        }
        fileIndex[resName] = null;
        return null;
    }

    public void FromJson(BonDocument json) {
        foreach(BonElement p in json) {
            switch(p.name) {
                case "packages": {
                    foreach(BonElement pp in p.value.AsBonDocument) {
                        PackageDesc pd = new PackageDesc();
                        pd.name = pp.name;
                        packages[pd.name] = pd;
                        pd.FromJson(pp.value.AsBonDocument);
                    }
                    break;
                }
            }
        }
    }

    public BonDocument ToJson() {
        BonDocument json = new BonDocument();
        BonDocument packages = new BonDocument();
        json["packages"] = packages;
        foreach(KeyValuePair<string, PackageDesc> p in this.packages) {
            packages[p.Key] = p.Value.ToJson();
        }
        return json;
    }


    public HashSet<string> GetDiff(FileSys other) {
        HashSet<string> diff = new HashSet<string>();
        foreach(PackageDesc pak in packages.Values) {
            PackageDesc pak2;
            if(!other.packages.TryGetValue(pak.name, out pak2) || pak.files.Count != pak2.files.Count) {
                diff.Add(pak.name);
                continue;
            }
            foreach(FileDesc file in pak.files.Values) {
                FileDesc file2;
                if(!pak2.files.TryGetValue(file.name, out file2) || file.crc != file2.crc) {
                    diff.Add(pak.name);
                    break;
                }
            }
        }
        return diff;
    }
}
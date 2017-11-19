using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseData: IFastGetSet {
    public virtual string Key() {
        return "";
    }

    public BonDocument ToBon() {
        return ToBon(null);
    }

    public BonDocument ToBon(HashSet<string> fields) {
        return BonUtil.ToBon(this, fields).AsBonDocument;
    }


    public void FromBon(BonValue jv) {
        FromBon(jv, null);
    }

    public void FromBon(BonValue jv, HashSet<string> fields) {
        BonUtil.ToObj(jv, this.GetType(), this, fields);//, this as IBonMonitor);
    }

    public virtual void FastSetValue(string name, object value) {
    }

    public virtual object FastGetValue(string name) {
        return null;
    }
}
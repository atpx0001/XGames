using System.Collections;
using System.Collections.Generic;
using System.Text;
public class BonDocument: BonValue, IEnumerable<BonElement> {
    private List<string> keys = new List<string>();
    private Dictionary<string, BonElement> elements = new Dictionary<string, BonElement>();
    public int Count {
        get {
            return keys.Count;
        }
    }

    public BonDocument() {
    }

    public static BonDocument FromBonBytes(byte[] data) {
        using(DataReader br = new DataReader(data)) {
            return new BonTokener(br).Read().AsBonDocument;
        }
    }

    public static BonDocument FromJsonString(string json) {
        return new BonJsonTokener(json).NextValue().AsBonDocument;
    }

    public BonElement this[int index] {
        get {
            return elements[keys[index]];
        }
    }

    public override BonValue this[string name] {
        get {
            BonElement el;
            if(elements.TryGetValue(name, out el)) {
                return el.value;
            }
            return null;
        }
        set {
            BonElement e;
            if(elements.TryGetValue(name, out e)) {
                keys.Remove(name);
                e.value = value == null ? BonNull.value : value;
            } else {
                e = new BonElement(name, value);
            }
            elements[name] = e;
            keys.Add(name);
        }
    }

    public int GetInt(string name, int defaultValue = 0) {
        BonElement el;
        if(elements.TryGetValue(name, out el)) {
            return el.value.AsInt;
        }
        return defaultValue;
    }

    public long GetLong(string name, long defaultValue = 0L) {
        BonElement el;
        if(elements.TryGetValue(name, out el)) {
            return el.value.AsLong;
        }
        return defaultValue;
    }

    public float GetFloat(string name, float defaultValue = 0f) {
        BonElement el;
        if(elements.TryGetValue(name, out el)) {
            return el.value.AsFloat;
        }
        return defaultValue;
    }

    public double GetDouble(string name, double defaultValue = 0.0) {
        BonElement el;
        if(elements.TryGetValue(name, out el)) {
            return el.value.AsDouble;
        }
        return defaultValue;
    }

    public string GetString(string name, string defaultValue = null) {
        BonElement el;
        if(elements.TryGetValue(name, out el)) {
            return el.value.AsString;
        }
        return defaultValue;
    }

    public bool GetBoolean(string name, bool defaultValue = false) {
        BonElement el;
        if(elements.TryGetValue(name, out el)) {
            return el.value.AsBoolean;
        }
        return defaultValue;
    }

    public byte[] GetBinary(string name, byte[] defaultValue = null) {
        BonElement el;
        if(elements.TryGetValue(name, out el)) {
            return el.value.AsBinary;
        }
        return defaultValue;
    }

    public BonArray GetBonArray(string name, BonArray defaultValue = null) {
        BonElement el;
        if(elements.TryGetValue(name, out el)) {
            return el.value.AsBonArray;
        }
        return defaultValue;
    }

    public BonDocument GetBonDocument(string name, BonDocument defaultValue = null) {
        BonElement el;
        if(elements.TryGetValue(name, out el)) {
            return el.value.AsBonDocument;
        }
        return defaultValue;
    }

    public bool Contains(string key) {
        return elements.ContainsKey(key);
    }

    public void Add(BonElement e) {
        BonElement e2;
        if(elements.TryGetValue(e.name, out e2)) {
            keys.Remove(e2.name);
        }
        elements[e.name] = e;
        keys.Add(e.name);
    }

    public void Remove(string name) {
        BonElement e;
        if(elements.TryGetValue(name, out e)) {
            elements.Remove(name);
            keys.Remove(name);
        }
    }

    public override BonDocument AsBonDocument {
        get { return this; }
    }

    public override bool IsBonDocument {
        get { return true; }
    }

    public override string AsString {
        get { return ToJsonString(); }
    }

    protected internal override void ToJsonStringUnformated(StringBuilder sb) {
        sb.Append('{');
        int c = Count;
        string name = null;
        for(int index = 0; index < c; index++) {
            name = keys[index];
            if(index > 0) {
                sb.Append(',');
            }
            sb.Append('\"');
            sb.Append(name);
            sb.Append("\":");
            elements[name].value.ToJsonStringUnformated(sb);
        }
        sb.Append('}');
    }

    protected internal override void ToJsonStringFormated(StringBuilder sb, int indent) {
        sb.Append('{');
        if(this.Count > 0) {
            sb.Append('\n');
            int i2 = indent + 2;
            int c = Count;
            string name = null;
            for(int index = 0; index < c; index++) {
                name = keys[index];
                if(index > 0) {
                    sb.Append(",\n");
                }
                AddIndent(sb, i2);
                sb.Append('\"');
                sb.Append(name);
                sb.Append("\":");
                elements[name].value.ToJsonStringFormated(sb, i2);
            }
            sb.Append('\n');
            AddIndent(sb, indent);
        }
        sb.Append('}');
    }

    protected internal override void ToBonBytes(DataWriter bw) {
        bw.Write((byte)BonValueType.Document);
        bw.Write7BitEncodedInt(Count);
        int c = Count;
        string name = null;
        for(int index = 0; index < c; index++) {
            name = keys[index];
            bw.Write(name);
            elements[name].value.ToBonBytes(bw);
        }
    }

    private struct Enumerator: IEnumerator<BonElement> {
        private Dictionary<string, BonElement> map;
        private IEnumerator<string> keys;
        private BonElement current;
        public Enumerator(BonDocument doc) {
            this.map = doc.elements;
            this.keys = doc.keys.GetEnumerator();
            current = BonElement.Empty;
        }

        #region IEnumerator<BonElement> 成员

        public BonElement Current {
            get { return current; }
        }

        #endregion

        #region IEnumerator 成员

        object IEnumerator.Current {
            get { return current; }
        }

        public bool MoveNext() {
            if(keys.MoveNext()) {
                current = map[keys.Current];
                return true;
            }
            current = BonElement.Empty;
            return false;
        }

        public void Reset() {
            keys.Reset();
            current = BonElement.Empty;
        }

        #endregion

        #region IDisposable 成员

        public void Dispose() {
            keys.Dispose();
        }

        #endregion
    }

    #region IEnumerable<BonElement> 成员

    public IEnumerator<BonElement> GetEnumerator() {
        return new Enumerator(this);
    }

    #endregion

    #region IEnumerable 成员

    IEnumerator IEnumerable.GetEnumerator() {
        return new Enumerator(this);
    }

    #endregion

}

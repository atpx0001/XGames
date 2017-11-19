using System.Collections.Generic;
using System.IO;
using System.Text;

public class BonArray: BonValue, IEnumerable<BonValue> {
    private List<BonValue> values = new List<BonValue>();

    public BonArray() {
    }


    public static BonArray FromBonBytes(byte[] data) {
        using(DataReader br = new DataReader(data)) {
            return new BonTokener(br).Read().AsBonArray;
        }
    }

    public static BonArray FromJsonString(string json) {
        return new BonJsonTokener(json).NextValue().AsBonArray;
    }

    public BonValue this[int index] {
        get {
            return values[index];
        }
        set {
            for(int i = values.Count; i <= index; i++) {
                values.Add(BonNull.value);
            }
            values[index] = value == null ? BonNull.value : value;
        }
    }

    public int Count {
        get {
            return values.Count;
        }
    }

    public void Add(BonValue v) {
        values.Add(v == null ? BonNull.value : v);
    }

    public void RemoveAt(int index) {
        values.RemoveAt(index);
    }

    public override bool IsBonArray {
        get { return true; }
    }

    public override BonArray AsBonArray {
        get { return this; }
    }

    public override string AsString {
        get { return ToJsonString(); }
    }

    protected internal override void ToJsonStringUnformated(StringBuilder sb) {
        sb.Append("[");
        int c = Count;
        for(int index = 0; index < c; index++) {
            if(index > 0) {
                sb.Append(",");
            }
            values[index].ToJsonStringUnformated(sb);
        }
        sb.Append("]");
    }

    protected internal override void ToJsonStringFormated(StringBuilder sb, int indent) {
        sb.Append('[');
        if(values.Count > 0) {
            sb.Append('\n');
            int i2 = indent + 2;
            int c = Count;
            for(int index = 0; index < c; index++) {
                if(index > 0) {
                    sb.Append(",\n");
                }
                AddIndent(sb, i2);
                values[index].ToJsonStringFormated(sb, i2);
            }
            sb.Append('\n');
            AddIndent(sb, indent);
        }
        sb.Append(']');
    }

    protected internal override void ToBonBytes(DataWriter bw) {
        bw.Write((byte)BonValueType.Array);
        bw.Write7BitEncodedInt(Count);
        int c = Count;
        for(int i = 0; i < c; i++) {
            values[i].ToBonBytes(bw);
        }
    }

    #region IEnumerable<BonValue> 成员

    public IEnumerator<BonValue> GetEnumerator() {
        return values.GetEnumerator();
    }

    #endregion

    #region IEnumerable 成员

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
        return values.GetEnumerator();
    }

    #endregion
    
}

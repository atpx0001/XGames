using System;
using System.IO;
using System.Text;

public class BonBinary: BonValue {
    public byte[] value;

    public BonBinary() {
    }

    public BonBinary(byte[] v) {
        value = v;
    }

    public override byte[] AsBinary {
        get { return value; }
    }

    public override bool IsBinary {
        get { return true; }
    }

    public override string AsString {
        get { return Convert.ToBase64String(value); }
    }

    protected internal override void ToJsonStringUnformated(StringBuilder sb) {
        sb.Append("\"").Append(AsString).Append("\"");
    }

    protected internal override void ToJsonStringFormated(StringBuilder sb, int indent) {
        sb.Append("\"").Append(AsString).Append("\"");
    }

    protected internal override void ToBonBytes(DataWriter bw) {
        bw.Write((byte)BonValueType.Binary);
        bw.Write7BitEncodedInt(value.Length);
        bw.Write(value);
    }

    public override string ToString() {
        return "Binary: " + value.Length;
    }
    
}

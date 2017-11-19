using System;
using System.IO;
using System.Text;

public class BonInt: BonValue {
    public int value;
    public BonInt() {
    }
    public BonInt(int v) {
        value = v;
    }

    public override bool AsBoolean {
        get { return value != 0; }
    }

    public override double AsDouble {
        get { return value; }
    }

    public override int AsInt {
        get { return value; }
    }

    public override float AsFloat {
        get { return value; }
    }

    public override long AsLong {
        get { return value; }
    }

    public override string AsString {
        get { return value.ToString(); }
    }

    public override bool IsInt {
        get { return true; }
    }

    protected internal override void ToJsonStringUnformated(StringBuilder sb) {
        sb.Append(value.ToString());
    }

    protected internal override void ToJsonStringFormated(StringBuilder sb, int indent) {
        sb.Append(value.ToString());
    }

    protected internal override void ToBonBytes(DataWriter bw) {
        bw.Write((byte)BonValueType.Int);
        bw.Write(value);
    }
    
}

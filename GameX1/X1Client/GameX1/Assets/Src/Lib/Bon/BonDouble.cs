using System;
using System.IO;
using System.Text;

public class BonDouble: BonValue {
    public double value;
    public BonDouble() {
    }
    public BonDouble(double v) {
        value = v;
    }

    public override bool AsBoolean {
        get { return value != 0.0; }
    }

    public override double AsDouble {
        get { return value; }
    }

    public override int AsInt {
        get { return (int)value; }
    }

    public override float AsFloat {
        get { return (float)value; }
    }

    public override long AsLong {
        get { return (long)value; }
    }

    public override string AsString {
        get { return value.ToString(); }
    }

    public override bool IsDouble {
        get { return true; }
    }

    protected internal override void ToJsonStringUnformated(StringBuilder sb) {
        sb.Append(value.ToString());
    }

    protected internal override void ToJsonStringFormated(StringBuilder sb, int indent) {
        sb.Append(value.ToString());
    }

    protected internal override void ToBonBytes(DataWriter bw) {
        bw.Write((byte)BonValueType.Double);
        bw.Write(value);
    }
}

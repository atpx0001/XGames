using System;
using System.IO;
using System.Text;

public class BonBoolean: BonValue {
    public bool value;
    public BonBoolean() {
    }

    public BonBoolean(bool v) {
        value = v;
    }

    public override bool AsBoolean {
        get { return value; }
    }

    public override double AsDouble {
        get { return value ? 1.0 : 0.0; }
    }

    public override int AsInt {
        get { return value ? 1 : 0; }
    }

    public override float AsFloat {
        get { return value ? 1f : 0f; }
    }

    public override long AsLong {
        get { return value ? 1L : 0L; }
    }

    public override string AsString {
        get { return value ? "true" : "false"; }
    }

    public override bool IsBoolean {
        get { return true; }
    }

    protected internal override void ToJsonStringUnformated(StringBuilder sb) {
        sb.Append(AsString);
    }

    protected internal override void ToJsonStringFormated(StringBuilder sb, int indent) {
        sb.Append(AsString);
    }

    protected internal override void ToBonBytes(DataWriter bw) {
        bw.Write((byte)BonValueType.Boolean);
        bw.Write(value ? (byte)1 : (byte)0);
    }
}

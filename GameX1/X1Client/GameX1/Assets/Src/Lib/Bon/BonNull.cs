using System;
using System.IO;
using System.Text;

public class BonNull: BonValue {
    public static readonly BonNull value = new BonNull();
    private BonNull() {
    }
    public override bool IsNull {
        get { return true; }
    }

    protected internal override void ToJsonStringUnformated(StringBuilder sb) {
        sb.Append("null");
    }

    protected internal override void ToJsonStringFormated(StringBuilder sb, int indent) {
        sb.Append("null");
    }

    protected internal override void ToBonBytes(DataWriter bw) {
        bw.Write((byte)BonValueType.Null);
    }
    
}

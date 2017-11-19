using System;
using System.IO;
using System.Text;
using UnityEngine;
public class BonString: BonValue {
    public string value;
    public BonString() {
    }
    public BonString(string v) {
        value = v;
    }
    public override bool AsBoolean {
        get { return !(value.ToLower() == "false" || value == "0" || string.IsNullOrEmpty(value)); }
    }

    public override double AsDouble {
        get {
            double v;
            return double.TryParse(value, out v) ? v : 0.0;
        }
    }

    public override int AsInt {
        get {
            int v;
            return int.TryParse(value, out v) ? v : 0;
        }
    }

    public override float AsFloat {
        get {
            float v;
            return float.TryParse(value, out v) ? v : 0f;
        }
    }

    public override long AsLong {
        get {
            long v;
            return long.TryParse(value, out v) ? v : 0L;
        }
    }

    public override byte[] AsBinary {
        get {
            try {
                return Convert.FromBase64String(value);
            } catch(Exception e) {
                Debug.LogError(e);
                return null;
            }
        }
    }

    public override string AsString {
        get { return value; }
    }

    public override bool IsString {
        get { return true; }
    }

    protected internal override void ToJsonStringUnformated(StringBuilder sb) {
        sb.Append("\"");
        char c = '\0';
        for(int i = 0; i < value.Length; i++) {
            c = value[i];
            switch(c) {
                case '"': sb.Append(@"\"""); continue;
                case '\\': sb.Append(@"\\"); continue;
                case '\b': sb.Append(@"\b"); continue;
                case '\f': sb.Append(@"\f"); continue;
                case '\n': sb.Append(@"\n"); continue;
                case '\r': sb.Append(@"\r"); continue;
                case '\t': sb.Append(@"\t"); continue;
                default: {
                    int ci = (int)c;
                    if(ci < 32 || ci > 126 && ci < 256) {
                        sb.AppendFormat(@"\u{0:x4}", ci);
                    } else {
                        sb.Append(c);
                    }
                    continue;
                }
            }
        }
        sb.Append("\"");
    }

    protected internal override void ToJsonStringFormated(StringBuilder sb, int indent) {
        ToJsonStringUnformated(sb);
    }

    protected internal override void ToBonBytes(DataWriter bw) {
        bw.Write((byte)BonValueType.String);
        bw.Write(value);
    }

}

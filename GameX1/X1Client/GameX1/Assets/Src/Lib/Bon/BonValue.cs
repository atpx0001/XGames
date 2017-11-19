using System;
using System.IO;
using System.Text;

public enum BonValueType {
    Double = 0x01,
    String = 0x02,
    Document = 0x03,
    Array = 0x04,
    Binary = 0x05,
    //ObjectId = 0x07,
    Boolean = 0x08,
    //DateTime = 0x09,
    Null = 0x0A,
    //RegExp = 0x0B,
    //JavaScript = 0x0D,
    //Symbol = 0x0E,
    //JavaScriptWithScope = 0x0F,
    Int = 0x10,
    //Timestamp = 0x11,
    Long = 0x12,
    Float = 0x20,
}
public abstract class BonValue {
    public virtual BonValue this[string name] {
        get {
            return null;
        }
        set {
        }
    }

    public virtual bool AsBoolean {
        get { return false; }
    }
    public virtual double AsDouble {
        get { return 0.0; }
    }
    public virtual int AsInt {
        get { return 0; }
    }
    public virtual float AsFloat {
        get { return 0f; }
    }
    public virtual long AsLong {
        get { return 0L; }
    }
    public virtual BonDocument AsBonDocument {
        get { return null; }
    }
    public virtual BonArray AsBonArray {
        get { return null; }
    }
    public virtual string AsString {
        get { return null; }
    }

    public override string ToString() {
        return AsString;
    }

    public virtual byte[] AsBinary {
        get { return null; }
    }

    public virtual bool IsBoolean {
        get { return false; }
    }
    public virtual bool IsDouble {
        get { return false; }
    }
    public virtual bool IsFloat {
        get { return false; }
    }
    public virtual bool IsInt {
        get { return false; }
    }
    public virtual bool IsLong {
        get { return false; }
    }
    public virtual bool IsString {
        get { return false; }
    }
    public virtual bool IsBonDocument {
        get { return false; }
    }
    public virtual bool IsBonArray {
        get { return false; }
    }
    public virtual bool IsNull {
        get { return false; }
    }
    public virtual bool IsBinary {
        get { return false; }
    }

    public string ToJsonString(bool formated = false) {
        StringBuilder sb = new StringBuilder();
        if(formated) {
            ToJsonStringFormated(sb, 0);
        } else {
            ToJsonStringUnformated(sb);
        }
        return sb.ToString();
    }

    protected internal abstract void ToJsonStringUnformated(StringBuilder sb);

    /// <summary>
    /// 带格式化
    /// </summary>
    /// <param name="sb"></param>
    /// <param name="indent"></param>
    protected internal abstract void ToJsonStringFormated(StringBuilder sb, int indent);

    protected void AddIndent(StringBuilder sb, int indent) {
        while(indent > 0) {
            sb.Append(' ');
            indent--;
        }
    }

    public byte[] ToBonBytes() {
        MemoryStream ms = new MemoryStream();
        using(DataWriter bw = new DataWriter(ms)) {
            ToBonBytes(bw);
            return ms.ToArray();
        }
    }

    protected internal abstract void ToBonBytes(DataWriter bw);

    public static implicit operator BonValue(string value) {
        if(value == null) {
            return BonNull.value;
        }
        return new BonString(value);
    }

    public static implicit operator BonValue(byte[] value) {
        if(value == null) {
            return BonNull.value;
        }
        return new BonBinary(value);
    }

    public static implicit operator BonValue(int value) {
        return new BonInt(value);
    }

    public static implicit operator BonValue(long value) {
        return new BonLong(value);
    }

    public static implicit operator BonValue(float value) {
        return new BonFloat(value);
    }

    public static implicit operator BonValue(double value) {
        return new BonDouble(value);
    }

    public static implicit operator BonValue(bool value) {
        return new BonBoolean(value);
    }

}

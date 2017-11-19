using System;

public class BonTokener {
    private DataReader br;

    public BonTokener(DataReader br) {
        this.br = br;
    }

    private BonException SyntaxError(String message) {
        return new BonException(message + ToString());
    }

    public override String ToString() {
        return " at index " + br.BaseStream.Position;
    }

    public BonValue Read() {
        try {
            BonValueType type = (BonValueType)br.ReadByte();
            switch(type) {
                case BonValueType.Array: return ReadArray();
                case BonValueType.Document: return ReadDoc();
                case BonValueType.Binary: return ReadBinary();
                case BonValueType.Boolean:
                {
                    BonBoolean v = new BonBoolean(br.ReadByte() != 0);
                    return v;
                }
                case BonValueType.Double:
                {
                    BonDouble v = new BonDouble(br.ReadDouble());
                    return v;
                }
                case BonValueType.Float:
                {
                    BonFloat v = new BonFloat(br.ReadSingle());
                    return v;
                }
                case BonValueType.Int:
                {
                    BonInt v = new BonInt(br.ReadInt32());
                    return v;
                }
                case BonValueType.Long:
                {
                    BonLong v = new BonLong(br.ReadInt64());
                    return v;
                }
                case BonValueType.Null: return BonNull.value;
                case BonValueType.String:
                {
                    BonString v = new BonString(br.ReadString());
                    return v;
                }
                default: return null;
            }
        } catch(Exception e) {
            throw SyntaxError(e.Message);
        }
    }

    private BonArray ReadArray() {
        BonArray arr = new BonArray();
        int count = br.Read7BitEncodedInt();
        for(int i = 0; i < count; i++) {
            arr.Add(Read());
        }
        return arr;
    }

    private BonDocument ReadDoc() {
        BonDocument doc = new BonDocument();
        int count = br.Read7BitEncodedInt();
        for(int i = 0; i < count; i++) {
            string name = br.ReadString();
            doc[name] = Read();
        }
        return doc;
    }

    private BonBinary ReadBinary() {
        int l = br.Read7BitEncodedInt();
        BonBinary v = new BonBinary();
        v.value = br.ReadBytes(l);
        return v;
    }

}

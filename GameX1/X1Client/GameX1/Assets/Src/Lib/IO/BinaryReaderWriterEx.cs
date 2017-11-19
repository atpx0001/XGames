//using System;
//using System.IO;
//using System.Text;


//public class BinaryReaderEx: BinaryReader {
//    public BinaryReaderEx(byte[] data) : base(new MemoryStream(data)) {

//    }
//    public BinaryReaderEx(Stream input)
//        : base(input) {
//    }

//    public new int Read7BitEncodedInt() {
//        return base.Read7BitEncodedInt();
//    }
//}

//public class BinaryWriterEx: BinaryWriter {
//    public BinaryWriterEx() : base(new MemoryStream()) {

//    }

//    public BinaryWriterEx(Stream output)
//        : base(output) {
//    }

//    public new void Write7BitEncodedInt(int v) {
//        base.Write7BitEncodedInt(v);
//    }
//}

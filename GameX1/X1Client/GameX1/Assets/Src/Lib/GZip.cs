using System.IO;

public static class GZip {
    public static byte[] Compress(byte[] data) {
        MemoryStream ms = new MemoryStream();
        using(GZipOutputStream gz = new GZipOutputStream(ms, 9, false)) {
            gz.Write(data, 0, data.Length);
            byte[] data2 = ms.ToArray();
            return data2;
        }
    }

    public static byte[] Decompress(byte[] data) {
        using(MemoryStream ms = new MemoryStream()) {
            using(GZipInputStream gz = new GZipInputStream(data)) {
                byte[] buff = new byte[1024 * 10];
                while(true) {
                    int len = gz.Read(buff, 0, buff.Length);
                    if(len == 0) {
                        break;
                    }
                    ms.Write(buff, 0, len);
                }
                byte[] data2 = ms.ToArray();
                return data2;
            }
        }
    }

    //public static void Compress(Stream ins, Stream outs) {
    //    byte[] buff = new byte[1024 * 1024];
    //    GZipStream gz = new GZipStream(outs, CompressionMode.Compress, CompressionLevel.BestCompression, true);
    //    int len = 0;
    //    while(ins.Position < ins.Length) {
    //        len = ins.Read(buff, 0, Mathf.Min(buff.Length, (int)(ins.Length - ins.Position)));
    //        gz.Write(buff, 0, len);
    //    }
    //    outs.Flush();
    //}

    //public static void Decompress(Stream ins, Stream outs) {
    //    byte[] buff = new byte[1024 * 1024];
    //    GZipStream gz = new GZipStream(ins, CompressionMode.Decompress, true);
    //    int len = 0;
    //    while(ins.Position < ins.Length) {
    //        len = gz.Read(buff, 0, Mathf.Min(buff.Length, (int)(ins.Length - ins.Position)));
    //        outs.Write(buff, 0, len);
    //    }
    //    outs.Flush();
    //}
}


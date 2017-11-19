using System.IO;
using UnityEngine;

public static class Lzma {
    public static void Decompress(Stream ins, Stream outs) {
        byte[] head = new byte[13];
        ins.Read(head, 0, 13);
        using(LzmaInputStream lzma = new LzmaInputStream(head, ins)) {
            byte[] buff = new byte[1024 * 10];
            while(lzma.Position < lzma.Length) {
                int len = lzma.Read(buff, 0, Mathf.Min(buff.Length, (int)(lzma.Length - lzma.Position)));
                outs.Write(buff, 0, len);
            }
        }
    }

    public static byte[] Decompress(byte[] data) {
        byte[] outb = null;
        using(MemoryStream outms = new MemoryStream()) {
            using(MemoryStream inms = new MemoryStream(data)) {
                Decompress(inms, outms);
            }
            outb = outms.ToArray();
        }
        return outb;
    }
}





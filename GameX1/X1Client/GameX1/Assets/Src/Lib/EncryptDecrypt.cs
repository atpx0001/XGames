using System;
public class EncryptDecrypt {
    /// <summary>
    /// Encrypt the specified data.
    /// </summary>
    /// <param name="data">Data.</param>
    public static byte[] Encrypt(byte[] data) {
        data = GZip.Compress(data);
        byte key = 0;
        Random rnd = new Random();
        key = (byte)rnd.Next(2, 250);
        for(int i = Math.Min(data.Length, 100); --i >= 0; ) {
            data[i] ^= key;
        }
        data[0] = (byte)'p';
        data[1] = (byte)'k';
        data[4] = (byte)((data[4] & 0xee) | (((key >> 7) & 0x1) << 4) | (((key >> 6) & 0x1) << 0));
        data[5] = (byte)((data[5] & 0xee) | (((key >> 5) & 0x1) << 4) | (((key >> 4) & 0x1) << 0));
        data[6] = (byte)((data[6] & 0xee) | (((key >> 3) & 0x1) << 4) | (((key >> 2) & 0x1) << 0));
        data[7] = (byte)((data[7] & 0xee) | (((key >> 1) & 0x1) << 4) | (((key >> 0) & 0x1) << 0));
        return data;
    }

    /// <summary>
    /// Decrypt the specified data.
    /// </summary>
    /// <param name="data">Data.</param>
    public static byte[] Decrypt(byte[] data) {
        int len = data.Length;
        int time = (data[4] << 24) | (data[5] << 16) | (data[6] << 8) | (data[7]);
        byte key = 0;
        for(int i = 0; i < 8; i++) {
            key |= (byte)(((time >> (i << 2)) & 0x1) << i);
        }
        for(int i = Math.Min(100, len); --i >= 2; ) {
            data[i] ^= key;
        }
        data[0] = 0x1f;
        data[1] = 0x8b;
        data = GZip.Decompress(data);
        return data;
    }

    public static byte[] EncryptPak(byte[] data) {
        byte key = 0;
        Random rnd = new Random();
        key = (byte)rnd.Next(2, 250);
        for(int i = Math.Min(data.Length, 100); --i >= 5; ) {
            data[i] ^= key;
        }
        data[0] = (byte)'p';
        data[1] = (byte)'k';
        data[2] = (byte)rnd.Next(2, 250);
        data[3] = key;
        data[4] = (byte)rnd.Next(2, 250);
        return data;
    }

    /// <summary>
    /// Decrypt the specified data.
    /// </summary>
    /// <param name="data">Data.</param>
    public static byte[] DecryptPak(byte[] data) {
        int len = data.Length;
        byte key = data[3];
        for(int i = Math.Min(100, len); --i >= 5; ) {
            data[i] ^= key;
        }
        data[0] = 0x5d;
        data[1] = 0x00;
        data[2] = 0x00;
        data[3] = 0x40;
        data[4] = 0x00;
        return data;
    }
}
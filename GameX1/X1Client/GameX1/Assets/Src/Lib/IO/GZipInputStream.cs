using UnityEngine;
using System.Collections;
using System;
using System.IO;
using SevenZip.Compression.LZMA;
using SevenZip;
using System.Runtime.InteropServices;
using System.Collections.Generic;


public class GZipInputStream: Stream {
    const int MAX_IN_BUFF_SIZE = 512 * 1024;
    bool disposed = false;
    IntPtr gz;
    Stream inStream;
    bool leaveOpen;
    byte[] inBuff;
    int inBuffLen;
    int inBuffIdx;
    bool inIsStream;

    public override bool CanRead {
        get {
            return true;
        }
    }

    public override bool CanSeek {
        get {
            return false;
        }
    }

    public override bool CanWrite {
        get {
            return false;
        }
    }

    public override long Length {
        get {
            return inIsStream ? inStream.Length : inBuffLen;
        }
    }

    public override long Position {
        get {
            return inIsStream ? inStream.Position : inBuffIdx;
        }
        set {
            throw new NotImplementedException();
        }
    }

    public GZipInputStream(byte[] data) {
        gz = GZip_Alloc(1, 0);
        if(gz == IntPtr.Zero) {
            throw new Exception("gz创建出错");
        }
        inBuff = data;
        inBuffIdx = 0;
        inBuffLen = data.Length;
        inIsStream = false;
    }

    public GZipInputStream(Stream stream, bool leaveOpen = true) {
        gz = GZip_Alloc(1, 0);
        if(gz == IntPtr.Zero) {
            throw new Exception("gz创建出错");
        }
        inStream = stream;
        inBuff = new byte[Mathf.Min((int)(stream.Length - stream.Position), MAX_IN_BUFF_SIZE)];
        inBuffIdx = 0;
        inBuffLen = 0;
        this.leaveOpen = leaveOpen;
        inIsStream = true;
    }

    protected override void Dispose(bool disposing) {
        if(disposed) {
            return;
        }
        disposed = true;
        if(disposing) {
            inBuff = null;
            if(!leaveOpen && inStream != null) {
                inStream.Close();
            }
            inStream = null;
        }
        if(gz != IntPtr.Zero) {
            GZip_Free(gz, 1);
            gz = IntPtr.Zero;
        }
        base.Dispose(disposing);
    }

    public override void Flush() {
        throw new NotImplementedException();
    }

    public override int Read(byte[] buffer, int offset, int count) {
        int tolen = 0;
        while(count > 0) {
            if(inBuffIdx >= inBuffLen) {
                if(inIsStream) {
                    inBuffLen = Mathf.Min((int)(inStream.Length - inStream.Position), inBuff.Length);
                    if(inBuffLen <= 0) {
                        break;
                    }
                    inStream.Read(inBuff, 0, inBuffLen);
                    inBuffIdx = 0;
                } else {
                    break;
                }
            }
            int ilen = inBuffLen - inBuffIdx;
            int olen = count;
            int b = GZip_Decode(gz, inBuff, inBuffIdx, ref ilen, buffer, offset, ref olen);
            if(b != 0 && b != 1) {
                throw new Exception("gz解压数据错误");
            }
            offset += olen;
            count -= olen;
            tolen += olen;
            inBuffIdx += ilen;
            if(b == 1 || olen == 0) {
                break;
            }
        }
        return tolen;
    }

    public override long Seek(long offset, SeekOrigin origin) {
        throw new NotImplementedException();
    }

    public override void SetLength(long value) {
        throw new NotImplementedException();
    }

    public override void Write(byte[] buffer, int offset, int count) {
        throw new NotImplementedException();
    }


#if UNITY_EDITOR
    [DllImport("Lzma.x64")]
#elif UNITY_STANDALONE_WIN
    [DllImport("Lzma.x86")]
#elif UNITY_IOS
    [DllImport("__Internal")]
#elif UNITY_ANDROID
    [DllImport("Lzma")]
#endif
    static extern IntPtr GZip_Alloc(int mode, int level);
#if UNITY_EDITOR
    [DllImport("Lzma.x64")]
#elif UNITY_STANDALONE_WIN
    [DllImport("Lzma.x86")]
#elif UNITY_IOS
    [DllImport("__Internal")]
#elif UNITY_ANDROID
    [DllImport("Lzma")]
#endif
    static extern void GZip_Free(IntPtr gz, int mode);
#if UNITY_EDITOR
    [DllImport("Lzma.x64")]
#elif UNITY_STANDALONE_WIN
    [DllImport("Lzma.x86")]
#elif UNITY_IOS
    [DllImport("__Internal")]
#elif UNITY_ANDROID
    [DllImport("Lzma")]
#endif
    static extern int GZip_Decode(IntPtr gz, byte[] ibuff, int ioffset, ref int ilen, byte[] obuff, int ooffset, ref int olen);
    
}



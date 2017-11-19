using UnityEngine;
using System.Collections;
using System;
using System.IO;
using SevenZip.Compression.LZMA;
using SevenZip;
using System.Runtime.InteropServices;
using System.Collections.Generic;


public class GZipOutputStream: Stream {
    const int MAX_OUT_BUFF_SIZE = 512 * 1024;
    bool disposed = false;
    IntPtr gz;
    Stream outStream;
    bool leaveOpen;
    byte[] oBuff;

    public override bool CanRead {
        get {
            return false;
        }
    }

    public override bool CanSeek {
        get {
            return false;
        }
    }

    public override bool CanWrite {
        get {
            return true;
        }
    }

    public override long Length {
        get {
            return outStream.Length;
        }
    }

    public override long Position {
        get {
            return outStream.Position;
        }
        set {
            throw new NotImplementedException();
        }
    }

    public GZipOutputStream(Stream stream, int level = 9, bool leaveOpen = true) {
        gz = GZip_Alloc(0, level);
        if(gz == IntPtr.Zero) {
            throw new Exception("gz创建出错");
        }
        this.outStream = stream;
        this.leaveOpen = leaveOpen;
    }

    protected override void Dispose(bool disposing) {
        if(disposed) {
            return;
        }
        disposed = true;
        if(disposing) {
            if(!leaveOpen && outStream != null) {
                outStream.Close();
            }
            outStream = null;
        }
        if(gz != IntPtr.Zero) {
            GZip_Free(gz, 0);
            gz = IntPtr.Zero;
        }
        base.Dispose(disposing);
    }

    public override void Flush() {
        throw new NotImplementedException();
    }

    public override int Read(byte[] buffer, int offset, int count) {
        throw new NotImplementedException();
    }

    public override long Seek(long offset, SeekOrigin origin) {
        throw new NotImplementedException();
    }

    public override void SetLength(long value) {
        throw new NotImplementedException();
    }

    public override void Write(byte[] buffer, int offset, int count) {
        if(oBuff == null) {
            oBuff = new byte[MAX_OUT_BUFF_SIZE];
        }
        int idx = 0;
        int iLen = 0;
        int oLen = 0;
        while(idx < count) {
            iLen = count - idx;
            oLen = oBuff.Length;
            int err = GZip_Encode0(gz, buffer, offset, ref iLen, oBuff, 0, ref oLen);
            if(err != 0) {
                throw new Exception("gz压缩错误");
            }
            outStream.Write(oBuff, 0, oLen);
            idx += iLen;
            offset += iLen;
        }
        while(true) {
            oLen = oBuff.Length;
            int err = GZip_Encode1(gz, oBuff, 0, ref oLen);
            if(err != 0 && err != 1) {
                throw new Exception("gz压缩错误");
            }
            outStream.Write(oBuff, 0, oLen);
            if(err == 1 || oLen == 0) {
                break;
            }
        }
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
    static extern int GZip_Encode0(IntPtr gz, byte[] ibuff, int ioffset, ref int ilen, byte[] obuff, int ooffset, ref int olen);

#if UNITY_EDITOR
    [DllImport("Lzma.x64")]
#elif UNITY_STANDALONE_WIN
    [DllImport("Lzma.x86")]
#elif UNITY_IOS
    [DllImport("__Internal")]
#elif UNITY_ANDROID
    [DllImport("Lzma")]
#endif
    static extern int GZip_Encode1(IntPtr gz, byte[] obuff, int ooffset, ref int olen);


}



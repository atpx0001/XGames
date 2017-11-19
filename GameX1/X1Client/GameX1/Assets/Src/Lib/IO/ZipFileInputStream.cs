using UnityEngine;
using System.Collections;
using System;
using System.IO;
using SevenZip.Compression.LZMA;
using SevenZip;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;

public class ZipFileInputStream: Stream {
    bool disposed = false;
    IntPtr zf;
    int oLen;
    int oPos;

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
            return oLen;
        }
    }

    public override long Position {
        get {
            return oPos;
        }
        set {
            throw new NotImplementedException();
        }
    }

    public ZipFileInputStream(string zipFilename, string filename) {
        zf = ZipFile_Alloc(zipFilename, filename, ref oLen);
        if(zf == IntPtr.Zero) {
            throw new Exception("打开zip文件出错");
        }
    }
    
    protected override void Dispose(bool disposing) {
        if(disposed) {
            return;
        }
        disposed = true;
        if(disposing) {
        }
        if(zf != IntPtr.Zero) {
            ZipFile_Free(zf);
            zf = IntPtr.Zero;
        }
        base.Dispose(disposing);
    }

    public override void Flush() {
        throw new NotImplementedException();
    }

    public override int Read(byte[] buffer, int offset, int count) {
        int len = ZipFile_Decode(zf, buffer, offset, Mathf.Min(count, oLen - oPos));
        if(len < 0) {
            throw new Exception("解压zip文件出错");
        }
        oPos += len;
        return len;
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
    static extern IntPtr ZipFile_Alloc(string zFilename, string filename, ref int olen);
#if UNITY_EDITOR
    [DllImport("Lzma.x64")]
#elif UNITY_STANDALONE_WIN
    [DllImport("Lzma.x86")]
#elif UNITY_IOS
    [DllImport("__Internal")]
#elif UNITY_ANDROID
    [DllImport("Lzma")]
#endif
    static extern void ZipFile_Free(IntPtr uf);
#if UNITY_EDITOR
    [DllImport("Lzma.x64")]
#elif UNITY_STANDALONE_WIN
    [DllImport("Lzma.x86")]
#elif UNITY_IOS
    [DllImport("__Internal")]
#elif UNITY_ANDROID
    [DllImport("Lzma")]
#endif
    static extern int ZipFile_Decode(IntPtr uf, byte[] obuff, int ooffset, int olen);


}



using UnityEngine;
using System.Collections;
using System;
using System.IO;
using SevenZip.Compression.LZMA;
using SevenZip;
using System.Runtime.InteropServices;
using System.Collections.Generic;


public class LzmaInputStream: Stream {
    const int MAX_IN_BUFF_SIZE = 512 * 1024;
    bool disposed = false;
    IntPtr lzma;
    Stream inStream;
    bool inIsStream;
    bool leaveOpen;
    byte[] inBuff;
    int inBuffLen;
    int inBuffIdx;

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

    public LzmaInputStream(byte[] data) {
        if(data.Length < 13) {
            throw new Exception("数据长度不够");
        }
        lzma = Lzma_Alloc(data, ref oLen);
        if(lzma == IntPtr.Zero) {
            throw new Exception("数据头错误");
        }
        inBuff = data;
        inBuffIdx = 13;
        inBuffLen = data.Length;
        inIsStream = false;
    }

    public LzmaInputStream(Stream stream, bool leaveOpen = true) {
        int left = (int)(stream.Length - stream.Position);
        if(left < 13) {
            throw new Exception("数据长度不够");
        }
        inBuff = new byte[Mathf.Max(13, Mathf.Min(left - 13, MAX_IN_BUFF_SIZE))];
        stream.Read(inBuff, 0, 13);
        lzma = Lzma_Alloc(inBuff, ref oLen);
        if(lzma == IntPtr.Zero) {
            throw new Exception("数据头错误");
        }
        inStream = stream;
        inBuffIdx = 0;
        inBuffLen = 0;
        inIsStream = true;
        this.leaveOpen = leaveOpen;
    }

    public LzmaInputStream(byte[] head, Stream stream, bool leaveOpen = true) {
        lzma = Lzma_Alloc(head, ref oLen);
        if(lzma == IntPtr.Zero) {
            throw new Exception("数据头错误");
        }
        inBuff = new byte[Mathf.Min((int)(stream.Length - stream.Position), MAX_IN_BUFF_SIZE)];
        inStream = stream;
        inBuffIdx = 0;
        inBuffLen = 0;
        inIsStream = true;
        this.leaveOpen = leaveOpen;
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
        if(lzma != IntPtr.Zero) {
            Lzma_Free(lzma);
            lzma = IntPtr.Zero;
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
            int olen = Mathf.Min(count, oLen - oPos);
            int b = Lzma_Decode(lzma, inBuff, inBuffIdx, ref ilen, buffer, offset, ref olen);
            if(b != 0) {
                throw new Exception("解压数据错误");
            }
            oPos += olen;
            offset += olen;
            count -= olen;
            tolen += olen;
            inBuffIdx += ilen;
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
    static extern IntPtr Lzma_Alloc(byte[] ibuff, ref int olen);
#if UNITY_EDITOR
    [DllImport("Lzma.x64")]
#elif UNITY_STANDALONE_WIN
    [DllImport("Lzma.x86")]
#elif UNITY_IOS
    [DllImport("__Internal")]
#elif UNITY_ANDROID
    [DllImport("Lzma")]
#endif
    static extern void Lzma_Free(IntPtr lzma);
#if UNITY_EDITOR
    [DllImport("Lzma.x64")]
#elif UNITY_STANDALONE_WIN
    [DllImport("Lzma.x86")]
#elif UNITY_IOS
    [DllImport("__Internal")]
#elif UNITY_ANDROID
    [DllImport("Lzma")]
#endif
    static extern int Lzma_Decode(IntPtr lzma, byte[] ibuff, int ioffset, ref int ilen, byte[] obuff, int ooffset, ref int olen);


}



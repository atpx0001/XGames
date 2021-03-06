﻿using System;
using System.IO;
using System.Text;

/// <summary>
/// 低字节优先
/// </summary>
public class DataWriter: BinaryWriter {
    //private byte[] buff = new byte[8];

    public DataWriter(MemoryStream output) :
        base(output) {
    }

    public DataWriter()
        : base(new MemoryStream()) {
    }

    public byte[] ToArray() {
        return ((MemoryStream)BaseStream).ToArray();
    }

    //public override void Write(int value) {
    //    buff[0] = (byte)(value >> 24);
    //    buff[1] = (byte)(value >> 16);
    //    buff[2] = (byte)(value >> 8);
    //    buff[3] = (byte)(value >> 0);
    //    Write(buff, 0, 4);
    //}

    //public override void Write(long value) {
    //    buff[0] = (byte)(value >> 56);
    //    buff[1] = (byte)(value >> 48);
    //    buff[2] = (byte)(value >> 40);
    //    buff[3] = (byte)(value >> 32);
    //    buff[4] = (byte)(value >> 24);
    //    buff[5] = (byte)(value >> 16);
    //    buff[6] = (byte)(value >> 8);
    //    buff[7] = (byte)(value >> 0);
    //    Write(buff, 0, 8);
    //}

    //public override void Write(short value) {
    //    buff[0] = (byte)(value >> 8);
    //    buff[1] = (byte)(value >> 0);
    //    Write(buff, 0, 2);
    //}

    //public override void Write(uint value) {
    //    Write((int)value);
    //}

    //public override void Write(ulong value) {
    //    Write((long)value);
    //}

    //public override void Write(ushort value) {
    //    Write((short)value);
    //}

    //public override void Write(float value) {
    //    Write(BitConverterEx.SingleToInt32Bits(value));
    //}

    //public override void Write(double value) {
    //    Write(BitConverter.DoubleToInt64Bits(value));
    //}

    public void WriteUTF(string s) {
        Write(Encoding.UTF8.GetBytes(s));
    }

    public new void Write7BitEncodedInt(int value) {
        base.Write7BitEncodedInt(value);
    }
}

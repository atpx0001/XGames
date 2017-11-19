using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static class MD5 {
    static char[] hex_chr = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f' };

    private static string rhex(int num) {
        string str = "";
        for(int j = 0; j <= 3; j++) {
            str = str + hex_chr[(num >> ((j << 3) + 4)) & 0xf] + hex_chr[(num >> (j << 3)) & 0xf];
        }
        return str;
    }

    /*
     * Convert a string to a sequence of 16-word blocks, stored as an array.
     * Append padding bits and the length, as described in the MD5 standard.
     */
    private static int[] str2blks_MD5(string str) {
        int nblk = ((str.Length + 8) >> 6) + 1;
        int[] blks = new int[nblk << 4];
        int i = 0;
        for(i = 0; i < blks.Length; i++) {
            blks[i] = 0;
        }
        for(i = 0; i < str.Length; i++) {
            blks[i >> 2] |= str[i] << ((i & 0x3) << 3);
        }
        blks[i >> 2] |= 0x80 << ((i & 0x3) << 3);
        blks[(nblk << 4) - 2] = str.Length << 3;

        return blks;
    }

    /*
     * Add integers, wrapping at 2^32
     */
    private static int add(int x, int y) {
        return (int)(((x & 0x7FFFFFFF) + (y & 0x7FFFFFFF)) ^ (x & 0x80000000) ^ (y & 0x80000000));
    }

    /*
     * Bitwise rotate a 32-bit number to the left
     */
    private static int rol(int num, int cnt) {
        return (num << cnt) | (int)((uint)num >> (32 - cnt));
    }

    /*
     * These functions implement the basic operation for each round of the
     * algorithm.
     */
    private static int cmn(int q, int a, int b, int x, int s, int t) {
        return add(rol(add(add(a, q), add(x, t)), s), b);
    }

    private static int ff(int a, int b, int c, int d, int x, int s, int t) {
        return cmn((b & c) | ((~b) & d), a, b, x, s, t);
    }

    private static int gg(int a, int b, int c, int d, int x, int s, int t) {
        return cmn((b & d) | (c & (~d)), a, b, x, s, t);
    }

    private static int hh(int a, int b, int c, int d, int x, int s, int t) {
        return cmn(b ^ c ^ d, a, b, x, s, t);
    }

    private static int ii(int a, int b, int c, int d, int x, int s, int t) {
        return cmn(c ^ (b | (~d)), a, b, x, s, t);
    }

    /*
     * Take a string and return the hex representation of its MD5.
     */
    public static string CalcMD5(string str) {
        int[] x = str2blks_MD5(str);
        int a = 0x67452301;
        int b = (0xEFCD << 16) | 0xAB89;
        int c = (0x98BA << 16) | 0xDCFE;
        int d = 0x10325476;

        for(int i = 0; i < x.Length; i += 16) {
            int olda = a;
            int oldb = b;
            int oldc = c;
            int oldd = d;

            a = ff(a, b, c, d, x[i + 0], 7, (0xD76A << 16) | 0xA478);
            d = ff(d, a, b, c, x[i + 1], 12, (0xE8C7 << 16) | 0xB756);
            c = ff(c, d, a, b, x[i + 2], 17, (0x2420 << 16) | 0x70DB);
            b = ff(b, c, d, a, x[i + 3], 22, (0xC1BD << 16) | 0xCEEE);
            a = ff(a, b, c, d, x[i + 4], 7, (0xF57C << 16) | 0x0FAF);
            d = ff(d, a, b, c, x[i + 5], 12, (0x4787 << 16) | 0xC62A);
            c = ff(c, d, a, b, x[i + 6], 17, (0xA830 << 16) | 0x4613);
            b = ff(b, c, d, a, x[i + 7], 22, (0xFD46 << 16) | 0x9501);
            a = ff(a, b, c, d, x[i + 8], 7, (0x6980 << 16) | 0x98D8);
            d = ff(d, a, b, c, x[i + 9], 12, (0x8B44 << 16) | 0xF7AF);
            c = ff(c, d, a, b, x[i + 10], 17, (0xFFFF << 16) | 0x5BB1);
            b = ff(b, c, d, a, x[i + 11], 22, (0x895C << 16) | 0xD7BE);
            a = ff(a, b, c, d, x[i + 12], 7, (0x6B90 << 16) | 0x1122);
            d = ff(d, a, b, c, x[i + 13], 12, (0xFD98 << 16) | 0x7193);
            c = ff(c, d, a, b, x[i + 14], 17, (0xA679 << 16) | 0x438E);
            b = ff(b, c, d, a, x[i + 15], 22, (0x49B4 << 16) | 0x0821);

            a = gg(a, b, c, d, x[i + 1], 5, (0xF61E << 16) | 0x2562);
            d = gg(d, a, b, c, x[i + 6], 9, (0xC040 << 16) | 0xB340);
            c = gg(c, d, a, b, x[i + 11], 14, (0x265E << 16) | 0x5A51);
            b = gg(b, c, d, a, x[i + 0], 20, (0xE9B6 << 16) | 0xC7AA);
            a = gg(a, b, c, d, x[i + 5], 5, (0xD62F << 16) | 0x105D);
            d = gg(d, a, b, c, x[i + 10], 9, (0x0244 << 16) | 0x1453);
            c = gg(c, d, a, b, x[i + 15], 14, (0xD8A1 << 16) | 0xE681);
            b = gg(b, c, d, a, x[i + 4], 20, (0xE7D3 << 16) | 0xFBC8);
            a = gg(a, b, c, d, x[i + 9], 5, (0x21E1 << 16) | 0xCDE6);
            d = gg(d, a, b, c, x[i + 14], 9, (0xC337 << 16) | 0x07D6);
            c = gg(c, d, a, b, x[i + 3], 14, (0xF4D5 << 16) | 0x0D87);
            b = gg(b, c, d, a, x[i + 8], 20, (0x455A << 16) | 0x14ED);
            a = gg(a, b, c, d, x[i + 13], 5, (0xA9E3 << 16) | 0xE905);
            d = gg(d, a, b, c, x[i + 2], 9, (0xFCEF << 16) | 0xA3F8);
            c = gg(c, d, a, b, x[i + 7], 14, (0x676F << 16) | 0x02D9);
            b = gg(b, c, d, a, x[i + 12], 20, (0x8D2A << 16) | 0x4C8A);

            a = hh(a, b, c, d, x[i + 5], 4, (0xFFFA << 16) | 0x3942);
            d = hh(d, a, b, c, x[i + 8], 11, (0x8771 << 16) | 0xF681);
            c = hh(c, d, a, b, x[i + 11], 16, (0x6D9D << 16) | 0x6122);
            b = hh(b, c, d, a, x[i + 14], 23, (0xFDE5 << 16) | 0x380C);
            a = hh(a, b, c, d, x[i + 1], 4, (0xA4BE << 16) | 0xEA44);
            d = hh(d, a, b, c, x[i + 4], 11, (0x4BDE << 16) | 0xCFA9);
            c = hh(c, d, a, b, x[i + 7], 16, (0xF6BB << 16) | 0x4B60);
            b = hh(b, c, d, a, x[i + 10], 23, (0xBEBF << 16) | 0xBC70);
            a = hh(a, b, c, d, x[i + 13], 4, (0x289B << 16) | 0x7EC6);
            d = hh(d, a, b, c, x[i + 0], 11, (0xEAA1 << 16) | 0x27FA);
            c = hh(c, d, a, b, x[i + 3], 16, (0xD4EF << 16) | 0x3085);
            b = hh(b, c, d, a, x[i + 6], 23, (0x0488 << 16) | 0x1D05);
            a = hh(a, b, c, d, x[i + 9], 4, (0xD9D4 << 16) | 0xD039);
            d = hh(d, a, b, c, x[i + 12], 11, (0xE6DB << 16) | 0x99E5);
            c = hh(c, d, a, b, x[i + 15], 16, (0x1FA2 << 16) | 0x7CF8);
            b = hh(b, c, d, a, x[i + 2], 23, (0xC4AC << 16) | 0x5665);

            a = ii(a, b, c, d, x[i + 0], 6, (0xF429 << 16) | 0x2244);
            d = ii(d, a, b, c, x[i + 7], 10, (0x432A << 16) | 0xFF97);
            c = ii(c, d, a, b, x[i + 14], 15, (0xAB94 << 16) | 0x23A7);
            b = ii(b, c, d, a, x[i + 5], 21, (0xFC93 << 16) | 0xA039);
            a = ii(a, b, c, d, x[i + 12], 6, (0x655B << 16) | 0x59C3);
            d = ii(d, a, b, c, x[i + 3], 10, (0x8F0C << 16) | 0xCC92);
            c = ii(c, d, a, b, x[i + 10], 15, (0xFFEF << 16) | 0xF47D);
            b = ii(b, c, d, a, x[i + 1], 21, (0x8584 << 16) | 0x5DD1);
            a = ii(a, b, c, d, x[i + 8], 6, (0x6FA8 << 16) | 0x7E4F);
            d = ii(d, a, b, c, x[i + 15], 10, (0xFE2C << 16) | 0xE6E0);
            c = ii(c, d, a, b, x[i + 6], 15, (0xA301 << 16) | 0x4314);
            b = ii(b, c, d, a, x[i + 13], 21, (0x4E08 << 16) | 0x11A1);
            a = ii(a, b, c, d, x[i + 4], 6, (0xF753 << 16) | 0x7E82);
            d = ii(d, a, b, c, x[i + 11], 10, (0xBD3A << 16) | 0xF235);
            c = ii(c, d, a, b, x[i + 2], 15, (0x2AD7 << 16) | 0xD2BB);
            b = ii(b, c, d, a, x[i + 9], 21, (0xEB86 << 16) | 0xD391);

            a = add(a, olda);
            b = add(b, oldb);
            c = add(c, oldc);
            d = add(d, oldd);
        }
        return rhex(a) + rhex(b) + rhex(c) + rhex(d);
    }
}

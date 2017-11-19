//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;

//public class HLRandom {
//    int seed;
//    public int Seed { get { return seed; } }
//    System.Random random;

//    public HLRandom(int seed) {
//        this.seed = seed;
//        random = new System.Random(seed);
//    }

//    public HLRandom Clone() {
//        return new HLRandom(seed);
//    }

//    public int Next() {
//        return Next(int.MinValue, int.MaxValue);
//    }

//    public int Next(int minValue, int maxValue) {
//        return random.Next(minValue, maxValue);
//    }

//    public int GetIndex(int[] rates) {
//        int allRate = 0;

//        for(int i = 0; i < rates.Length; i++) {
//            allRate += rates[i];
//        }

//        if(allRate > 0) {
//            int randomRate = Next(0, allRate);

//            for(int i = 0; i < rates.Length; i++) {
//                int rate = rates[i];
//                if(randomRate < rate) { return i; }
//                randomRate -= rate;
//            }
//        }

//        return -1;
//    }

//    public int GetIndex(List<int> rates) {
//        return GetIndex(rates.ToArray());
//    }
//}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HLRandom {
    protected const byte ULongToIntShift = 33;

    public const ulong SeedX = 521288629U << 32;
    public const ulong SeedY = 4101842887655102017UL;
    protected const double IntToDoubleMultiplier = 1.0 / (int.MaxValue + 1.0);
    private ulong _x;
    private ulong _y;
    public int Seed { get; private set; }
    public int num = 0;

    public HLRandom(int seed) {
        this.Seed = seed;
        _x = SeedX + (uint)seed;
        _y = SeedY;
    }

    public HLRandom Clone() {
        return new HLRandom(Seed);
    }

    /// <summary>
    /// [0,1)
    /// </summary>
    /// <returns></returns>
    public double NextDouble() {
        return (int)(NextULong() << ULongToIntShift >> ULongToIntShift) * IntToDoubleMultiplier;
    }

    public ulong NextULong() {
        num++;
        var tx = _x;
        var ty = _y;
        _x = ty;
        tx ^= tx << 23;
        tx ^= tx >> 17;
        tx ^= ty ^ (ty >> 26);
        _y = tx;
        
        return tx + ty;
    }

    public int Next() {
        return (int)NextULong();
    }

    public int Next(int minValue, int maxValue) {
        return (int)(NextULong() % (ulong)(maxValue - minValue)) + minValue;
    }

    public int GetIndex(int[] rates) {
        int allRate = 0;

        for(int i = 0; i < rates.Length; i++) {
            allRate += rates[i];
        }

        if(allRate > 0) {
            int randomRate = Next(0, allRate);

            for(int i = 0; i < rates.Length; i++) {
                int rate = rates[i];
                if(randomRate < rate) { return i; }
                randomRate -= rate;
            }
        }

        return -1;
    }

    public int GetIndex(List<int> rates) {
        return GetIndex(rates.ToArray());
    }
}
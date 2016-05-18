using UnityEngine;
using System.Collections;

public struct Tile {

    public int X { get; private set; }
    public int Y { get; private set; }
    
    public Tile(int x, int y) {
        X = x;
        Y = y;
    }

    public long GetID() {
        return ((long)X << 32) + Y;
    }

    public static long GetIDbyXY(int x, int y) {
        return ((long)x << 32) + y; ;
    }

}

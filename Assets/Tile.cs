using UnityEngine;
using System.Collections;

public enum TileType {
    Sky,
    Ground,
    Clear
}

public class Tile {

    public int X { get; private set; }
    public int Y { get; private set; }
    public TileType Type { get; private set; }

    public Tile(int x, int y, TileType type) {
        X = x;
        Y = y;
        Type = type;
    }

    public long GetID() {
        return ((long)X << 32) + Y;
    }

    public static long GetIDbyXY(int x, int y) {
        return ((long)x << 32) + y; ;
    }

}

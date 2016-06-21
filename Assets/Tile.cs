using UnityEngine;
using System.Collections;

// TODO: this should be a string or int identifier, that can be directly set from the excel document.
public enum TileType {
    Clear = 0,
    Sky = 1,
    Ground = 2
}

public class Tile {

    public int X { get; private set; }
    public int Y { get; private set; }
    public TileType Type { get; private set; }
    public int EdgeIndex { get; set; }
    
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

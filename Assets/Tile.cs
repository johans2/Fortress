using UnityEngine;
using System.Collections;

public class Tile {

    public int X { get; private set; }
    public int Y { get; private set; }
    
    public Tile(int x, int y) {
        X = x;
        Y = y;
    }

    public string GetID() {
        return string.Format("{0}:{1}", X.ToString(), Y.ToString());
    }

    public static string GetIDbyXY(int x, int y) {
        return string.Format("{0}:{1}", x.ToString(), y.ToString());
    }

}

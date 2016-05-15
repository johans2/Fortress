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
        return string.Concat(X.ToString(), ":",Y.ToString());
    }

}

using UnityEngine;
using System.Collections.Generic;
using System;

public class TileManager {

    private Dictionary<long, Tile> tiles;
    private string filePath = "world_parsed.csv";

    public TileManager() {
        tiles = new Dictionary<long, Tile>();
    }

    public Tile GetTileById(long id) {
        Tile tile;
        if (!tiles.TryGetValue(id, out tile))
        {
            return null;
        }
        return tile;
    }

    private void ParseWorld() {
        try
        {
            string[] lines = System.IO.File.ReadAllLines(filePath);
        }
        catch (System.Exception)
        {


            throw;
        }
    }

    private Tile CreateTile(string type) {
        switch (type)
        {
            case "SKY":
                return null;
                break;
            case "GROUND":
                return null;
                break;

            default:
                throw new ArgumentException("Unable to create tile with type " + type);;
        }
    }
}

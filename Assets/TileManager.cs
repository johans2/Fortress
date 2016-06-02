using UnityEngine;
using System.Collections.Generic;
using System;

public class TileManager {

    private Dictionary<long, Tile> tiles;
    private string filePath = "Assets/Resources/world_parsed.csv";

    public TileManager() {
        tiles = new Dictionary<long, Tile>();
        ParseWorld();
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
            for (int y = 0; y < lines.Length; y++)
            {
                string[] rows = lines[y].Split(',');
                for (int x = 0; x < rows.Length; x++)
                {
                    Tile tile = CreateTile(x, -y, rows[x]);
                    if (tile != null)
                    {
                        tiles.Add(tile.GetID(), tile);
                    }
                }

            }

        }
        catch (System.Exception)
        {


            throw;
        }
    }

    private Tile CreateTile(int x, int y, string type) {
        switch (type)
        {
            case "SKY":
                return new Tile(x, y, TileType.Sky);
            case "GROUND":
                return new Tile(x, y, TileType.Ground);
            case "CLEAR":
                return null;
            default:
                throw new ArgumentException("Unable to create tile with type " + type);;
        }
    }

    public Vector3 GetWorldPositionByTileIndex(int x, int y, float tileSize)
    {
        return new Vector3(x * tileSize, y * tileSize, 0);
    }

    public Coordinate GetCoordByWorldPosition(Vector3 worldPosition, float tileSize)
    {
        int x;
        int y;

        if (worldPosition.x > 0f)
        {
            x = Mathf.FloorToInt(worldPosition.x / tileSize);
        }
        else
        {
            x = Mathf.CeilToInt(Mathf.Abs(worldPosition.x) / tileSize) * -1;
        }

        if (worldPosition.y > 0f)
        {
            y = Mathf.FloorToInt(worldPosition.y / tileSize);
        }
        else
        {
            y = Mathf.CeilToInt(Mathf.Abs(worldPosition.y) / tileSize) * -1;
        }

        return new Coordinate(x, y);
    }
}

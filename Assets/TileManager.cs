using UnityEngine;
using System.Collections.Generic;
using System;

public class TileManager {

    private Dictionary<long, Tile> tiles;
    private string filePath = "Assets/Resources/world_parsed.csv";

    public TileManager() {
        tiles = new Dictionary<long, Tile>();
        ParseWorld();
        SetTileEdgeIndexes();
    }

    public Tile GetTileById(long id) {
        Tile tile;
        if (!tiles.TryGetValue(id, out tile))
        {
            return null;
        }
        return tile;
    }
    
    public Tile GetTileByIndex(int x, int y) {
        return GetTileById(Tile.GetIDbyXY(x, y));
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
        catch (Exception ex)
        {
            throw new Exception("Unable to parse world" + ex.ToString());
        }
    }

    private Tile CreateTile(int x, int y, string type) {
        TileType tileType = TileType.Clear;

        switch (type)
        {
            case "SKY":
                tileType = TileType.Sky;
                break;
            case "GROUND":
                tileType = TileType.Ground;
                break;
            case "CLEAR":
                tileType = TileType.Clear;
                break;
            default:
                throw new ArgumentException("Unable to create tile with type " + type);;
        }
        
        return new Tile(x, y, tileType);
    }

    private void SetTileEdgeIndexes() {
        foreach (KeyValuePair<long, Tile> kvPair in tiles)
        {
            Tile tile = kvPair.Value;

            int x = tile.X;
            int y = tile.Y;

            int sum = 0;
            Tile above = GetTileByIndex(x, y + 1);
            Tile left = GetTileByIndex(x - 1, y + 1);
            Tile below = GetTileByIndex(x, y - 1);
            Tile right = GetTileByIndex(x + 1, y);

            if (above != null && above.Type == tile.Type)
                sum += 1;
            if (left != null && left.Type == tile.Type)
                sum += 2;
            if (below != null && below.Type == tile.Type)
                sum += 4;
            if (right != null && right.Type == tile.Type)
                sum += 8;

            kvPair.Value.EdgeIndex = sum;
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

    public Tile GetTileByWorldPosition(Vector3 worldPosition, float tileSize)
    {
        Coordinate coord = GetCoordByWorldPosition(worldPosition, tileSize);
        long id = Tile.GetIDbyXY(coord.X, coord.Y);

        Tile tile;
        if (!tiles.TryGetValue(id, out tile))
        {
            return null;
        }

        return tile;
    }
}

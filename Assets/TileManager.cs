using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

public class TileManager : MonoBehaviour {
    
    public GameObject CenterObject;
    public GameObject tileParent;

    public GameObject tilePrefab;
    public int poolSize = 2000;

    private float tileSize = 0.5f;
    private Tile currentTile;
    private Dictionary<string, GameObject> tileGameObjects;
    private List<Tile> currentTiles;
    private List<GameObject> tilePool;

    void Start() {
        currentTiles = new List<Tile>();
        tilePool = new List<GameObject>();
        tileGameObjects = new Dictionary<string, GameObject>();
        FillTilePool();
        Vector3 topRight = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, -Camera.main.transform.position.z));
        currentTile = GetTileByWorldPosition(topRight);
        OnEnterNewTile(currentTile, currentTile);
    }
    
	void Update () {
        CheckForNewTile();
    }

    void FillTilePool() {
        for (int i = 0; i <= poolSize; i++)
        {
            GameObject tile = (GameObject)Instantiate(tilePrefab, new Vector3(999f, 999f, 999f), Quaternion.identity);
            tile.transform.parent = tileParent.transform;
            tile.SetActive(false);
            tilePool.Add(tile);
        }
    }

    GameObject ActivateTile(Vector3 position) {
        for (int i = 0; i < tilePool.Count; i++)
        {
            if (!tilePool[i].activeInHierarchy)
            {
                tilePool[i].transform.position = position;

                tilePool[i].GetComponent<Renderer>().material.color = new Color(position.x / 20f, position.y / 20f, 0.5f);
                
                tilePool[i].SetActive(true);

                return tilePool[i];
            }
        }
        throw new System.Exception("TilePool comsumed.");
    }

    void DeactivateTile(GameObject tile) {
        tile.SetActive(false);
        tilePool.Add(tile);
    }

    void CheckForNewTile() {
        float dist = -Camera.main.transform.position.z;
        Vector3 topRight = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, dist));
        Tile calculatedTile = GetTileByWorldPosition(topRight);

        if (calculatedTile.X != currentTile.X || calculatedTile.Y != currentTile.Y)
        {
            OnEnterNewTile(calculatedTile, currentTile);
            currentTile = calculatedTile;
        }
    }

    private void OnEnterNewTile(Tile newTile, Tile oldTile) {

        int buffer = 2;

        //Debug.Log("New tile: " + newTile.X + " : " + newTile.Y);

        float dist = -Camera.main.transform.position.z;
        Vector3 bottomLeft = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, dist));
        Vector3 topLeft = Camera.main.ViewportToWorldPoint(new Vector3(0, 1, dist));
        Vector3 topRight = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, dist));
        Vector3 bottomRight = Camera.main.ViewportToWorldPoint(new Vector3(1, 0, dist));

        Tile topLeftTile = GetTileByWorldPosition(topLeft);
        Tile topRightTile = GetTileByWorldPosition(topRight);
        Tile bottomLeftTile = GetTileByWorldPosition(bottomLeft);
        Tile bottomRightTile = GetTileByWorldPosition(bottomRight);
        
        List<Tile> newTiles = new List<Tile>();
        List<Tile> deleteList = new List<Tile>();
        
        // Get new tiles
        for (int i = topLeftTile.X - buffer; i < topRightTile.X + buffer; i++)
        {
            for (int j = topLeftTile.Y + buffer; j > bottomLeftTile.Y - buffer; j--)
            {
                newTiles.Add(new Tile(i, j));
            }
        }

        // Check old tiles if in current bounds
        for (int i = 0; i < currentTiles.Count; i++)
        {
            Tile tile = currentTiles[i];
            if (tile.X <= topLeftTile.X || tile.X >= topRightTile.X)
            {
                deleteList.Add(tile);
            }
            if (tile.Y >= topLeftTile.Y || tile.Y <= bottomLeftTile.Y)
            {
                deleteList.Add(tile);
            }

        }

        // Delete tiles in deletelist
        for (int i = 0; i < deleteList.Count; i++)
        {
            string id = deleteList[i].GetID();
            if (tileGameObjects.ContainsKey(id))
            {
                //Destroy(tileGameObjects[id]);
                DeactivateTile(tileGameObjects[id]);
                tileGameObjects.Remove(id);
            }
        }
        currentTiles = newTiles;

        // Create new tiles if not present in currenTiles
        for (int i = 0; i < newTiles.Count; i++)
        {
            GameObject tileGameObject;
            if (!tileGameObjects.TryGetValue(newTiles[i].GetID(), out tileGameObject))
            {


                Vector3 position = GetWorldPositionByTileIndex(newTiles[i].X, newTiles[i].Y);


                GameObject newTileGO = ActivateTile(position); 
                //GameObject newTileGO = (GameObject)Instantiate(tilePrefab, position, Quaternion.identity);
                tileGameObjects.Add(newTiles[i].GetID(), newTileGO);


            }
        }
        
    }
    
    private Vector3 GetWorldPositionByTileIndex(int x, int y) {
        return new Vector3(x * tileSize, y * tileSize, 0);
    }

    public Tile GetTileByWorldPosition(Vector3 worldPosition) {
        int tileX;
        int tileY;

        if (worldPosition.x > 0f)
        {
            tileX = Mathf.FloorToInt(worldPosition.x / tileSize);
        }
        else
        {
            tileX = Mathf.CeilToInt(Mathf.Abs(worldPosition.x) / tileSize) * -1;
        }

        if (worldPosition.y > 0f)
        {
            tileY = Mathf.FloorToInt(worldPosition.y / tileSize);
        }
        else
        {
            tileY = Mathf.CeilToInt(Mathf.Abs(worldPosition.y) / tileSize) * -1;
        }

        return new Tile(tileX, tileY);


    }
    
}

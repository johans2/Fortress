using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

public class TileManager : MonoBehaviour {

    private class ViewBounds {
        public int xMax;
        public int xMin;
        public int yMax;
        public int yMin;

        public ViewBounds() {}

        public ViewBounds(Vector3 topRight, Vector3 bottomLeft) {
            xMax = GetTileByWorldPosition(topRight).X;
            xMin = GetTileByWorldPosition(bottomLeft).X;
            yMax = GetTileByWorldPosition(topRight).Y;
            yMin = GetTileByWorldPosition(bottomLeft).Y;
        }

        public override bool Equals(object obj)
        {
            ViewBounds other = obj as ViewBounds;
            if (other == null)
            {
                return false;
            }

            return xMax == other.xMax && xMin == other.xMin && yMax == other.yMax && yMin == other.yMin;
        }
    }

    public GameObject CenterObject;
    public GameObject tileParent;

    public GameObject tilePrefab;
    public int poolSize = 2000;

    private static float tileSize = 0.5f;
    private ViewBounds currentBounds;
    private Dictionary<string, GameObject> tileGameObjects;
    private List<Tile> currentTiles;
    private List<GameObject> tilePool;

    void Start() {
        currentTiles = new List<Tile>();
        tilePool = new List<GameObject>();
        tileGameObjects = new Dictionary<string, GameObject>();
        FillTilePool();
        Vector3 topRight = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, -Camera.main.transform.position.z));
        Vector3 bottomLeft = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, -Camera.main.transform.position.z));
        currentBounds = new ViewBounds(topRight, bottomLeft);

        FillView(currentBounds);
    }
    
	void Update () {
        CheckBounds();
    }

    void FillView(ViewBounds bounds) {
        for (int y = bounds.yMin; y <= bounds.yMax; y++)
        {
            for (int x = bounds.xMin; x <= bounds.xMax; x++)
            {
                Vector3 tilePosition = GetWorldPositionByTileIndex(x, y);
                //ActivateTile(tilePosition);
                
                Tile tile = new Tile(x, y);
                GameObject tileGameObject = (GameObject)Instantiate(tilePrefab, tilePosition, Quaternion.identity);
                tileGameObjects.Add(tile.GetID(), tileGameObject);
            }
        }
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

    void CheckBounds() {
        float dist = -Camera.main.transform.position.z;
        Vector3 topRight = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, dist));
        Vector3 bottomLeft = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, dist));

        ViewBounds calculatedBounds = new ViewBounds(topRight, bottomLeft);

        if (!calculatedBounds.Equals(currentBounds))
        {
            OnUpdatedBounds(calculatedBounds, currentBounds);
        }
        
    }

    private void OnUpdatedBounds(ViewBounds newBounds, ViewBounds oldBounds) {
        int deltaX = newBounds.xMax - oldBounds.xMax;
        int deltaY = newBounds.yMax - oldBounds.yMax;
           
        // Y tiles top to bot
        for (int y = newBounds.yMax; y > (newBounds.yMax - deltaY); y--)
        {
            // X tiles left to right
            for (int x = newBounds.xMin; x <= newBounds.xMax; x++)
            {
                // Create tile downwards
                Vector3 tilePosition = GetWorldPositionByTileIndex(x, y);
                Tile tile = new Tile(x, y);
                GameObject tileGameObject = (GameObject)Instantiate(tilePrefab, tilePosition, Quaternion.identity);
                tileGameObjects.Add(tile.GetID(), tileGameObject);
                
            }
        }

        // Create tiles for x diff
        for (int y = newBounds.yMax - deltaY; y >= newBounds.yMin; y--)
        {
            for (int x = (newBounds.xMax - deltaX + 1); x < newBounds.xMax + 1; x++)
            {
                Vector3 tilePosition = GetWorldPositionByTileIndex(x, y);
                Tile tile = new Tile(x, y);
                GameObject tileGameObject = (GameObject)Instantiate(tilePrefab, tilePosition, Quaternion.identity);
                tileGameObjects.Add(tile.GetID(), tileGameObject);

                // Remove tiles on other side
                string tileToRemoveId = new Tile(x - (oldBounds.xMax - oldBounds.xMin), y).GetID();
                GameObject tileToDestroy;

                if (tileGameObjects.TryGetValue(tileToRemoveId, out tileToDestroy))
                {
                    Destroy(tileToDestroy);
                    tileGameObjects.Remove(tileToRemoveId);
                }
                /*
                Destroy(tileGameObjects[tileToRemove.GetID()]);
                tileGameObjects.Remove(tileToRemove.GetID());*/
            }
        }
        currentBounds = newBounds;

    }
    
    private Vector3 GetWorldPositionByTileIndex(int x, int y) {
        return new Vector3(x * tileSize, y * tileSize, 0);
    }

    public static Tile GetTileByWorldPosition(Vector3 worldPosition) {
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

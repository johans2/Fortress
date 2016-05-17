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

    private static float tileSize = 1f;
    private ViewBounds currentBounds;
    private Dictionary<string, GameObject> tileGameObjects;
    private List<Tile> currentTiles;
    private List<GameObject> tilePool;

    private Tile currentCenter;

    private int viewWidth;
    private int viewHeight;

    void Start() {
        currentTiles = new List<Tile>();
        tilePool = new List<GameObject>();
        tileGameObjects = new Dictionary<string, GameObject>();
        FillTilePool();

        Vector3 topRight = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, -Camera.main.transform.position.z));
        Vector3 bottomLeft = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, -Camera.main.transform.position.z));
        Vector3 center = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, -Camera.main.transform.position.z));

        //currentBounds = new ViewBounds(topRight, bottomLeft);
        currentCenter = GetTileByWorldPosition(center);
        viewWidth = (GetTileByWorldPosition(center).X - GetTileByWorldPosition(bottomLeft).X) * 2;
        viewHeight = (GetTileByWorldPosition(center).Y - GetTileByWorldPosition(bottomLeft).Y) * 2;

        FillView(currentCenter, viewWidth, viewHeight);
    }
    
	void Update () {
        CheckTile();
        UpdateDebugLines();
    }

    void UpdateDebugLines() {
        Vector3 topLeft = Camera.main.ViewportToWorldPoint(new Vector3(0, 1, -Camera.main.transform.position.z + 0.01f));
        Vector3 topRight = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, -Camera.main.transform.position.z + 0.01f));
        Vector3 bottomLeft = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, -Camera.main.transform.position.z + 0.01f));
        Vector3 bottomRight = Camera.main.ViewportToWorldPoint(new Vector3(1, 0, -Camera.main.transform.position.z + 0.01f));

        Debug.DrawLine(topLeft, topRight, Color.red);
        Debug.DrawLine(topRight, bottomRight, Color.red);
        Debug.DrawLine(bottomRight, bottomLeft, Color.red);
        Debug.DrawLine(bottomLeft, topLeft, Color.red);
    }

    void FillView(Tile center, int viewWidth, int viewHeight) {
        Color color = Random.ColorHSV();
        for (int y = center.Y + (viewHeight / 2); y >= (center.Y - viewHeight / 2); y--) 
        {
            color = Random.ColorHSV();
            for(int x = center.X - (viewWidth / 2) ; x <= center.X + (viewWidth / 2); x++) 
            {
                Vector3 tilePosition = GetWorldPositionByTileIndex(x, y);
                Tile tile = new Tile(x, y);
                GameObject tileGameObject = (GameObject)Instantiate(tilePrefab, tilePosition, Quaternion.identity);
                tileGameObject.GetComponent<Renderer>().material.color = color;
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

    void CheckTile() {
        float dist = -Camera.main.transform.position.z;
        
        Vector3 newCenterPos = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, dist));
        Tile newCenter = GetTileByWorldPosition(newCenterPos);

        if (currentCenter.X != newCenter.X || currentCenter.Y != newCenter.Y)
        {
            OnEnterNewTile(currentCenter, newCenter);
        }
        
    }

    private void OnEnterNewTile(Tile oldCenter, Tile newCenter) {
        int deltaX = newCenter.X - oldCenter.X;
        int deltaY = newCenter.Y - oldCenter.Y;
        //Debug.Log("oldCenter: " + oldCenter.X + ":" + oldCenter.Y  + "  ----  " + "newCenter: " + newCenter.X + ":" + newCenter.Y + " ---- " + "DeltaY: " + deltaY);
        Color color = Color.black;

        int passOneYStart;
        int passOneYStop;

        if(deltaY >= 0) {
            passOneYStart = oldCenter.Y + (viewHeight / 2) + 1;
            passOneYStop = oldCenter.Y + (viewHeight / 2) + deltaY;
        }
        else{
            passOneYStart = oldCenter.Y - (viewHeight / 2) + deltaY;
            passOneYStop = oldCenter.Y - (viewHeight / 2) - 1;
        }
        
        int passOneXStart = newCenter.X - (viewWidth / 2);
        int passOneXStop = newCenter.X + (viewWidth / 2);
        
        for (int y = passOneYStart; y <= passOneYStop; y++)
        {
            for (int x = passOneXStart; x <= passOneXStop; x++)
            {
                // Create tile downwards
                Vector3 tilePosition = GetWorldPositionByTileIndex(x, y);
                Tile tile = new Tile(x, y);
                GameObject tileGameObject = (GameObject)Instantiate(tilePrefab, tilePosition, Quaternion.identity);
                tileGameObject.GetComponent<Renderer>().material.color = color;
                tileGameObjects.Add(tile.GetID(), tileGameObject);

                // Remove tiles on bottom
                
                int removeOffset = deltaY >= 0 ? (-viewHeight - 1) : viewHeight + 1;
                string tileToRemoveId = new Tile(x - deltaX,y + removeOffset).GetID();
                if (tileGameObjects.ContainsKey(tileToRemoveId))
                {
                    Destroy(tileGameObjects[tileToRemoveId]);
                    tileGameObjects.Remove(tileToRemoveId);
                }
            }
        }

        color = Color.red;    

        int passTwoYStart = newCenter.Y - (viewHeight / 2);
        int passTwoYStop = newCenter.Y + (viewHeight / 2) - deltaY;
        int passTwoXStart;
        int passTwoXStop;

        if(deltaX >= 0) {
            passTwoXStart = newCenter.X + (viewWidth / 2) - deltaX + 1;
            passTwoXStop = newCenter.X + (viewWidth / 2);
        }
        else {
            passTwoXStart = oldCenter.X - (viewWidth / 2) + deltaX;
            passTwoXStop = oldCenter.X - (viewWidth / 2) - 1;
        }

        for (int y = passTwoYStart; y <= passTwoYStop; y++)
        {
            for (int x = passTwoXStart; x <= passTwoXStop; x++)
            {
                try {
                    // Create tile downwards
                    Vector3 tilePosition = GetWorldPositionByTileIndex(x, y);
                    Tile tile = new Tile(x, y);
                    GameObject tileGameObject = (GameObject)Instantiate(tilePrefab, tilePosition, Quaternion.identity);
                    tileGameObject.GetComponent<Renderer>().material.color = color;
                    tileGameObjects.Add(tile.GetID(), tileGameObject);

                }
                catch(System.Exception) {

                    throw;
                }
                
                // Remove tiles on bottom
                int removeOffset = deltaX >= 0 ? (-viewWidth - 1) : viewWidth + 1;
                string tileToRemoveId = new Tile(x + removeOffset, y).GetID();
                if (tileGameObjects.ContainsKey(tileToRemoveId))
                {
                    Destroy(tileGameObjects[tileToRemoveId]);
                    tileGameObjects.Remove(tileToRemoveId);
                }
            }
        }

        currentCenter = newCenter;
        
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

/*
// Remove tiles on other side
string tileToRemoveId = new Tile(x - deltaX, y - (boardHeight + 1)).GetID();
                if(tileGameObjects.ContainsKey(tileToRemoveId)) {
                    Destroy(tileGameObjects[tileToRemoveId]);
tileGameObjects.Remove(tileToRemoveId);
                }
*/

/*
// Remove tiles on other side
string tileToRemoveId = new Tile(x - (boardWidth + 1), y).GetID();
                if(tileGameObjects.ContainsKey(tileToRemoveId)) {
                    Destroy(tileGameObjects[tileToRemoveId]);
tileGameObjects.Remove(tileToRemoveId);
                }
*/
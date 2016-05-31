using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using CakewalkIoC.Injection;


public class TileVisualizer : MonoBehaviour {

    public GameObject tileParent;
    public GameObject tilePrefab;
    public int poolSize = 2000;

    [Dependency]
    private TileManager tileManager { get; set; }

    private static float tileSize = 1f;
    private Dictionary<long, GameObject> tileGameObjects;
    private List<GameObject> tilePool;

    private Tile currentCenter;

    private int viewWidth;
    private int viewHeight;

    void Start() {
        this.Inject();

        tilePool = new List<GameObject>();
        tileGameObjects = new Dictionary<long, GameObject>(poolSize);
        FillTilePool();
        
        Vector3 bottomLeft = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, -Camera.main.transform.position.z));
        Vector3 center = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, -Camera.main.transform.position.z));
        
        currentCenter = GetTileByWorldPosition(center);
        viewWidth = (GetTileByWorldPosition(center).X - GetTileByWorldPosition(bottomLeft).X) * 2;
        viewHeight = (GetTileByWorldPosition(center).Y - GetTileByWorldPosition(bottomLeft).Y) * 2;

        FillView(currentCenter, viewWidth, viewHeight);
    }

    void Update() {
        CheckTile();
        //UpdateDebugLines();
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
        for(int y = center.Y + (viewHeight / 2); y >= (center.Y - viewHeight / 2); y--) {
            color = Random.ColorHSV();
            for(int x = center.X - (viewWidth / 2); x <= center.X + (viewWidth / 2); x++) {
                Vector3 tilePosition = GetWorldPositionByTileIndex(x, y);
                Tile tile = new Tile(x, y);
                GameObject tileGameObject = (GameObject)Instantiate(tilePrefab, tilePosition, Quaternion.identity);
                tileGameObject.GetComponent<Renderer>().material.color = color;
                tileGameObjects.Add(tile.GetID(), tileGameObject);
            }
        }
    }

    void FillTilePool() {
        for(int i = 0; i <= poolSize; i++) {
            GameObject tile = (GameObject)Instantiate(tilePrefab, new Vector3(999f, 999f, 999f), Quaternion.identity);
            tile.transform.parent = tileParent.transform;
            tile.SetActive(false);
            tilePool.Add(tile);
        }
    }

    GameObject ActivateTile(Vector3 position) {
        for(int i = 0; i < tilePool.Count; i++) {
            if(!tilePool[i].activeInHierarchy) {
                tilePool[i].transform.position = position;
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
        Vector3 newCenterPos = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, -Camera.main.transform.position.z));
        Tile newCenter = GetTileByWorldPosition(newCenterPos);

        if(currentCenter.X != newCenter.X || currentCenter.Y != newCenter.Y) {
            OnEnterNewTile(currentCenter, newCenter);
        }

    }

    private void OnEnterNewTile(Tile oldCenter, Tile newCenter) {
        int yStartNew = newCenter.Y - viewHeight / 2;
        int yStopNew = newCenter.Y + viewHeight / 2;
        int xStartNew = newCenter.X - viewWidth / 2;
        int xStopNew = newCenter.X + viewWidth / 2;
        
        // Add new tiles
        for(int y = yStartNew; y <= yStopNew; y++) {
            for(int x = xStartNew; x <= xStopNew; x++) {
                GameObject existing;
                Vector3 tilePosition = GetWorldPositionByTileIndex(x, y);
                long newTileId = Tile.GetIDbyXY(x, y);

                if(!tileGameObjects.TryGetValue(newTileId, out existing)) {
                    GameObject newTileGameObject = ActivateTile(tilePosition);
                    tileGameObjects.Add(newTileId, newTileGameObject);
                }
            }
        }

        int yStartOld = oldCenter.Y - viewHeight / 2;
        int yStopOld = oldCenter.Y + viewHeight / 2;
        int xStartOld = oldCenter.X - viewWidth / 2;
        int xStopOld = oldCenter.X + viewWidth / 2;

        // Remove old tiles
        for(int y = yStartOld; y <= yStopOld; y++) {
            for(int x = xStartOld; x <= xStopOld; x++) {
                                
                bool isOutsideView = y < yStartNew || y > yStopNew || x < xStartNew || x > xStopNew;
                if(!isOutsideView) {
                    continue;
                }

                GameObject tileToRemove;
                long idToRemove = Tile.GetIDbyXY(x, y);
                if(tileGameObjects.TryGetValue(idToRemove, out tileToRemove)) {
                    tileGameObjects.Remove(idToRemove);
                    DeactivateTile(tileToRemove);
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

        if(worldPosition.x > 0f) {
            tileX = Mathf.FloorToInt(worldPosition.x / tileSize);
        }
        else {
            tileX = Mathf.CeilToInt(Mathf.Abs(worldPosition.x) / tileSize) * -1;
        }

        if(worldPosition.y > 0f) {
            tileY = Mathf.FloorToInt(worldPosition.y / tileSize);
        }
        else {
            tileY = Mathf.CeilToInt(Mathf.Abs(worldPosition.y) / tileSize) * -1;
        }

        return new Tile(tileX, tileY);


    }

}


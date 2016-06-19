using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using CakewalkIoC.Injection;
using System;

public class TileVisualizer : MonoBehaviour {
    [Serializable]
    public class TextureData
    {
        public TileType tileType;
        public Texture texture;
    }

    public GameObject tileParent;
    public GameObject tilePrefab;
    public int poolSize = 2000;
    public TextureData[] textures;

    private Dictionary<TileType, Texture> textureDict;

    [Dependency]
    private TileManager tileManager { get; set; }

    private static float tileSize = 1f;
    private Dictionary<long, GameObject> activeTiles;
    private List<GameObject> tilePool;
    private Coordinate currentCenter;
    private int viewWidth;
    private int viewHeight;

    void Start() {
        this.Inject();
        tilePool = new List<GameObject>();
        activeTiles = new Dictionary<long, GameObject>(poolSize);
        textureDict = new Dictionary<TileType, Texture>();
        ParseTextures();
        FillTilePool();
        
        Vector3 bottomLeft = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, -Camera.main.transform.position.z));
        Vector3 center = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, -Camera.main.transform.position.z));

        currentCenter = tileManager.GetCoordByWorldPosition(center, tileSize);
        viewWidth = (currentCenter.X - tileManager.GetCoordByWorldPosition(bottomLeft, tileSize).X) * 2;
        viewHeight = (currentCenter.Y - tileManager.GetCoordByWorldPosition(bottomLeft, tileSize).Y) * 2;

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

    void ParseTextures() {
        for (int i = 0; i < textures.Length; i++)
        {
            textureDict.Add(textures[i].tileType, textures[i].texture);
        }
    }

    void FillView(Coordinate center, int viewWidth, int viewHeight) {
        Color color = UnityEngine.Random.ColorHSV();
        for(int y = center.Y + (viewHeight / 2); y >= (center.Y - viewHeight / 2); y--) {
            color = UnityEngine.Random.ColorHSV();
            for(int x = center.X - (viewWidth / 2); x <= center.X + (viewWidth / 2); x++) {
                Vector3 tilePosition = tileManager.GetWorldPositionByTileIndex(x, y, tileSize);
                Tile tile = new Tile(x, y, TileType.Clear);
                GameObject tileGameObject = (GameObject)Instantiate(tilePrefab, tilePosition, Quaternion.identity);
                tileGameObject.GetComponent<Renderer>().material.color = color;
                activeTiles.Add(tile.GetID(), tileGameObject);
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

    GameObject ActivateTile(Tile tile, Vector3 position) {
        for(int i = 0; i < tilePool.Count; i++) {
            if(!tilePool[i].activeInHierarchy) {
                tilePool[i].transform.position = position;
                tilePool[i].GetComponent<MeshRenderer>().material.mainTexture = GetTexture(tile.Type);
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
        Coordinate newCenter = tileManager.GetCoordByWorldPosition(newCenterPos, tileSize);// GetTileByWorldPosition(newCenterPos);

        if(currentCenter.X != newCenter.X || currentCenter.Y != newCenter.Y) {
            OnEnterNewTile(currentCenter, newCenter);
        }

    }

    private Texture GetTexture(TileType tileType) {
        Texture tex;
        if (!textureDict.TryGetValue(tileType, out tex))
        {
            throw new ArgumentException("No texture registered for TileType " + tileType.ToString());
        }
        return tex;
    }

    private void OnEnterNewTile(Coordinate oldCenter, Coordinate newCenter) {
        int yStartNew = newCenter.Y - viewHeight / 2;
        int yStopNew = newCenter.Y + viewHeight / 2;
        int xStartNew = newCenter.X - viewWidth / 2;
        int xStopNew = newCenter.X + viewWidth / 2;
        
        // Add new tiles
        for(int y = yStartNew; y <= yStopNew; y++) {
            for(int x = xStartNew; x <= xStopNew; x++) {
                GameObject existing;

                long newTileId = Tile.GetIDbyXY(x, y);

                if(!activeTiles.TryGetValue(newTileId, out existing)) {
                    Vector3 tilePosition = tileManager.GetWorldPositionByTileIndex(x, y, tileSize);
                    Tile tile = tileManager.GetTileByWorldPosition(tilePosition, tileSize);
                    if (tile != null) // Outside world.
                    {
                        GameObject newTileGameObject = ActivateTile(tile, tilePosition);
                        activeTiles.Add(newTileId, newTileGameObject);
                    }
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
                if(activeTiles.TryGetValue(idToRemove, out tileToRemove)) {
                    activeTiles.Remove(idToRemove);
                    DeactivateTile(tileToRemove);
                }
            }
        }

        currentCenter = newCenter;
    }

}


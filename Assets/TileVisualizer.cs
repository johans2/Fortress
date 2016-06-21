using UnityEngine;
using System.Collections.Generic;
using CakewalkIoC.Injection;
using System;

public class TileVisualizer : MonoBehaviour {
    [Serializable]
    public class TextureData
    {
        public string tileType;
        public Texture texture;
    }

    public GameObject tileParent;
    public GameObject tilePrefab;
    public int poolSize = 1000;
    public TextureData[] textures;

    [Dependency]
    private TileManager tileManager { get; set; }



    private static float tileSize = 1f;
    private Stack<GameObject> tilePool;
    private Dictionary<long, GameObject> visibleTiles;
    private Dictionary<string, Texture> textureDict;
    private Coordinate currentCenter;
    private int viewWidth;
    private int viewHeight;
    private int viewBuffer = 3;

    void Start() {
        this.Inject();
        tilePool = new Stack<GameObject>(poolSize);
        visibleTiles = new Dictionary<long, GameObject>((viewHeight + viewBuffer) * (viewWidth + viewBuffer));
        textureDict = new Dictionary<string, Texture>();
        ParseTextures();
        FillTilePool();
        
        Vector3 bottomLeft = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, -Camera.main.transform.position.z));
        Vector3 center = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, -Camera.main.transform.position.z));

        currentCenter = tileManager.GetCoordByWorldPosition(center, tileSize);
        viewWidth = (currentCenter.X - tileManager.GetCoordByWorldPosition(bottomLeft, tileSize).X) * 2 + viewBuffer;
        viewHeight = (currentCenter.Y - tileManager.GetCoordByWorldPosition(bottomLeft, tileSize).Y) * 2 + viewBuffer;

        OnEnterNewTile(currentCenter, currentCenter);
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
    
    void FillTilePool() {
        for(int i = 0; i <= poolSize; i++) {
            GameObject tile = (GameObject)Instantiate(tilePrefab, new Vector3(999f, 999f, 999f), Quaternion.identity);
            tile.transform.parent = tileParent.transform;
            tile.SetActive(false);
            tilePool.Push(tile);
        }
    }
    
    GameObject ActivateTile(Tile tile, Vector3 position) {
        if (tilePool.Count == 0)
        {
            throw new Exception("TilePool comsumed.");
        }

        GameObject tileGameObject = tilePool.Pop();
        tileGameObject.transform.position = position;
        tileGameObject.GetComponent<MeshRenderer>().material.mainTexture = GetTexture(tile.Type);
        tileGameObject.GetComponent<MeshRenderer>().material.SetTextureOffset("_MainTex", GetTextureOffset(tile.EdgeIndex));
        tileGameObject.SetActive(true);

        return tileGameObject;
    }

    Vector2 GetTextureOffset(int edgeIndex) {
        switch (edgeIndex)
        {
            case 0:
                return new Vector2(0,0);
            case 1:
                return new Vector2(0, 0.25f);
            case 2:
                return new Vector2(0.75f, 0);
            case 3:
                return new Vector2(0.75f, 0.25f);
            case 4:
                return new Vector2(0, 0.75f);
            case 5:
                return new Vector2(0, 0.5f);
            case 6:
                return new Vector2(0.75f, 0.75f);
            case 7:
                return new Vector2(0.75f, 0.5f);
            case 8:
                return new Vector2(0.25f, 0);
            case 9:
                return new Vector2(0.25f, 0.25f);
            case 10:
                return new Vector2(0.5f, 0);
            case 11:
                return new Vector2(0.5f, 0.25f);
            case 12:
                return new Vector2(0.25f, 0.75f);
            case 13:
                return new Vector2(0.25f, 0.5f);
            case 14:
                return new Vector2(0.5f, 0.75f);
            case 15:
                return new Vector2(0.5f, 0.5f);
            default:
                throw new Exception(string.Format("Invalid edge index: " + edgeIndex));
        }
    }

    void DeactivateTile(GameObject tile) {
        tile.SetActive(false);
        tilePool.Push(tile);
    }

    void CheckTile() {
        Vector3 newCenterPos = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, -Camera.main.transform.position.z));
        Coordinate newCenter = tileManager.GetCoordByWorldPosition(newCenterPos, tileSize);

        if(currentCenter.X != newCenter.X || currentCenter.Y != newCenter.Y) {
            OnEnterNewTile(currentCenter, newCenter);
        }

    }

    private Texture GetTexture(string tileType) {
        Texture tex;
        if (!textureDict.TryGetValue(tileType, out tex))
        {
            throw new ArgumentException(string.Format("No texture registered for TileType {0}", tileType.ToString()));
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

                if(!visibleTiles.TryGetValue(newTileId, out existing)) {
                    Vector3 tilePosition = tileManager.GetWorldPositionByTileIndex(x, y, tileSize);
                    Tile tile = tileManager.GetTileByWorldPosition(tilePosition, tileSize);
                    if (tile != null) // Outside world.
                    {
                        GameObject newTileGameObject = ActivateTile(tile, tilePosition);
                        visibleTiles.Add(newTileId, newTileGameObject);
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
                if(visibleTiles.TryGetValue(idToRemove, out tileToRemove)) {
                    visibleTiles.Remove(idToRemove);
                    DeactivateTile(tileToRemove);
                }
            }
        }

        currentCenter = newCenter;
    }

}


using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour {

    private static BoardManager _S;
    public static BoardManager S {
        get {
            
            if(_S == null) {
                
            }
            return _S;
        }
        set {

            if(_S != null) {
                Debug.LogError("An instance of BoardManager has already been assigned");
                Destroy(value);
            }
            else {
                _S = value;
            }
        }
    }

    [Header("Set in inspector")]
    public GameObject backGround;
    public int dimX;
    public int dimY;
    public TilesScriptableObject tilesSO;
    [Tooltip("Number of the layer that is on the tiles")]
    public int TILES_LAYER;
    [HideInInspector]
    public Tile[,] tiles;

    private Vector3 tilesExtents;
    #region selectDetectionVariables
    private int layerMask;
    private RaycastHit hit;
    private Vector2 inputPos;
    private Vector3 mouse3DPos;
    private Camera cam;
    private Ray ray;
    private RaycastHit raycastHit;
    private const int MAX_DIST = 1000;
    private bool isShifting = false;
    #endregion

    
    void Awake()
    {
        _S = this;
        tiles = new Tile[dimX, dimY];
        layerMask = 1 << TILES_LAYER;
        cam = Camera.main;
        PopulateBoard();
    }

    private void PopulateBoard()
    {
        // Get tiles dimensions in order to dispose them correctly
        // I suppose they will have all the same extents
        Tile tmp = Instantiate(tilesSO.GetRandomTile());
        Renderer renderer = tmp.gameObject.GetComponent<Renderer>();
        tmp.gameObject.SetActive(false);
        if (renderer == null)
        {
            Debug.LogError(" The tile you provided has no renderer component!");
            return;
        }
        tilesExtents = renderer.bounds.extents;

        // Get backGround bottom left corner 
        renderer = backGround.GetComponent<Renderer>();
        if (renderer == null) {
            Debug.LogError(" The background you provided has no renderer component!");
            return;
        }
        Vector3 backGroundExtents = renderer.bounds.extents;
        Vector3 backGroundPosition = backGround.gameObject.transform.position;

        Vector3 bottomLeftCorner = new Vector3(backGroundPosition.x - backGroundExtents.x, backGroundPosition.y - backGroundExtents.y, backGroundPosition.z - tilesExtents.z);
        //Debug.DrawLine(bottomLeftCorner, bottomLeftCorner + Vector3.up, Color.red, 20);
        //Debug.DrawLine(bottomLeftCorner, bottomLeftCorner + Vector3.down, Color.red, 20);
        //Debug.DrawLine(bottomLeftCorner, bottomLeftCorner + Vector3.left, Color.red, 20);
        //Debug.DrawLine(bottomLeftCorner, bottomLeftCorner + Vector3.right, Color.red, 20);

        //this simplifies the calculation of where the tiles should be one respect to another 
        bottomLeftCorner.x += tilesExtents.x;
        bottomLeftCorner.y += tilesExtents.y;

        for (int x = 0; x < dimX; x++)
        {

            for (int y = 0; y < dimY; y++)
            {
                tmp = Tile.SpawnTile(x, y);
                // TODO: create a local variable to avoid instantiating a vector3 each time
                tmp.transform.position = new Vector3(bottomLeftCorner.x + tilesExtents.x * (x * 2), bottomLeftCorner.y + tilesExtents.y * (y * 2), bottomLeftCorner.z);
                //Debug.DrawLine(tmp.transform.position, tmp.transform.position + Vector3.up, Color.yellow, 20);
                //Debug.DrawLine(tmp.transform.position, tmp.transform.position + Vector3.down, Color.yellow, 20);
                //Debug.DrawLine(tmp.transform.position, tmp.transform.position + Vector3.left, Color.yellow, 20);
                //Debug.DrawLine(tmp.transform.position, tmp.transform.position + Vector3.right, Color.yellow, 20);
                tiles[x, y] = tmp;

            }
        }
    }

    void Start()
    {

    }


    void Update()
    {

        if (Input.GetMouseButtonDown(0) && !isShifting)
        {
            inputPos = Input.mousePosition;
            mouse3DPos.x = inputPos.x;
            mouse3DPos.y = inputPos.y;

            ray = cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out raycastHit, MAX_DIST, layerMask))
            {
                Select(raycastHit.transform.gameObject);
            }
        }
    }

    private void Select(GameObject hitGO)
    {
        Tile tile = hitGO.GetComponent<Tile>();
        if (tile != null) {
            tile.Select();           
            StartCoroutine(ShiftTilesDown(tile.posX, tile.posY));
        }
        else {
            Debug.Log(" The gameObject hit is not a Tile! ");
        }
    }

    private IEnumerator ShiftTilesDown(int x, int yStart, float shiftDelay = .05f) {
        WaitForSeconds waitForShiftDelay = new WaitForSeconds(shiftDelay);
        // Avoid selection of tiles when something isShifting
        isShifting = true;         
        for (int y = yStart; y < dimY - 1; y++) {
            Tile tileAbove = tiles[x, y + 1];
            if (tileAbove != null) {
                Vector3 shiftedPos = tileAbove.gameObject.transform.position;
                shiftedPos.y -= tilesExtents.y * 2;
                tileAbove.gameObject.transform.position = shiftedPos;                
                tileAbove.posY = y;
                tiles[x, y] = tileAbove;
                tiles[x, y + 1] = null;
                yield return waitForShiftDelay;
            }
            else {
                break;
            }
        }
        isShifting = false;
        CheckMatch(tiles[x, yStart]);
    }

    private void CheckMatch(Tile startTile) {
        int nCount = 0;
        int y = startTile.posY;
        Tile[] equalTiles = new Tile[dimX];
       for(int x = startTile.posX + 1; x < dimX; x++) {
            if (tiles[x, y] != null && startTile.CheckMatch(tiles[x, y])) {
                equalTiles[nCount] = tiles[x, y];
                nCount++;
            }
            else {
                break;
            }
       }

       for (int x = startTile.posX - 1; x >= 0; x--) {
            if (tiles[x,y] != null && startTile.CheckMatch(tiles[x, y])) {
                equalTiles[nCount] = tiles[x, y];
                nCount++;
            }
            else {
                break;
            }
       }
      
        //Debug.Log("<color=red> <---- start printing equal tiles found ----> </color>");
        //for (int i = 0; i < nCount; i++)
        //{
        //    Debug.Log("Equal tile name: "+equalTiles[i].gameObject.name);
        //}
        if (nCount > 1) {
            equalTiles[nCount] = startTile;
            nCount++;
            for(int i = 0; i < nCount; i++) {
                Select(equalTiles[i].gameObject);
            }
       }

    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : Manager<BoardManager>
{
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
    private Coroutine co = null;
    #region selectDetectionVariables
    private int layerMask;
    private RaycastHit hit;
    private Vector2 inputPos;
    private Vector3 mouse3DPos;
    private Camera cam;
    private Ray ray;
    private RaycastHit raycastHit;
    private int[] nSelectedTiles;
    private const int MAX_DIST = 1000;
    #endregion

    
    protected override void Awake()
    {
        base.Awake();
        tiles = new Tile[dimX, dimY];
        layerMask = 1 << TILES_LAYER;
        cam = Camera.main;
        PopulateBoard();
    }

    private void PopulateBoard()
    {
        // Get tiles dimensions in order to dispose them correctly
        // I suppose they will have all the same extents

        Tile tmp = null;
        Renderer renderer = tilesSO.GetRandomTile().GetComponent<Renderer>();
        tilesExtents = renderer.bounds.extents;
        nSelectedTiles = new int[dimX];
        // Get backGround bottom left corner 
        renderer = backGround.GetComponent<Renderer>();
        if (renderer == null) {
            Debug.LogError(" The background you provided has no renderer component!");
            return;
        }
        Vector3 backGroundExtents = renderer.bounds.extents;
        Vector3 backGroundPosition = backGround.transform.position;

        Vector3 bottomLeftCorner = new Vector3(backGroundPosition.x - backGroundExtents.x, backGroundPosition.y - backGroundExtents.y, backGroundPosition.z - tilesExtents.z);
      
        //#if DEBUG
        //Debug.DrawLine(bottomLeftCorner, bottomLeftCorner + Vector3.up, Color.red, 20);
        //Debug.DrawLine(bottomLeftCorner, bottomLeftCorner + Vector3.down, Color.red, 20);
        //Debug.DrawLine(bottomLeftCorner, bottomLeftCorner + Vector3.left, Color.red, 20);
        //Debug.DrawLine(bottomLeftCorner, bottomLeftCorner + Vector3.right, Color.red, 20);
        //#endif

        //this simplifies the calculation of where the tiles should be one respect to another 
        bottomLeftCorner.x += tilesExtents.x;
        bottomLeftCorner.y += tilesExtents.y;

        for (int x = 0; x < dimX; x++)
        {
            nSelectedTiles[x] = 0;
            for (int y = 0; y < dimY; y++)
            {
                tmp = Tile.SpawnTile(x, y);
                // TODO: create a local variable to avoid instantiating a vector3 each time
                tmp.transform.position = new Vector3(bottomLeftCorner.x + tilesExtents.x * (x * 2), bottomLeftCorner.y + tilesExtents.y * (y * 2), bottomLeftCorner.z);
                #if DEBUG
                Debug.DrawLine(tmp.transform.position, tmp.transform.position + Vector3.up, Color.yellow, 20);
                Debug.DrawLine(tmp.transform.position, tmp.transform.position + Vector3.down, Color.yellow, 20);
                Debug.DrawLine(tmp.transform.position, tmp.transform.position + Vector3.left, Color.yellow, 20);
                Debug.DrawLine(tmp.transform.position, tmp.transform.position + Vector3.right, Color.yellow, 20);
                #endif
                tiles[x, y] = tmp;

            }
        }
    }


    void Update()
    {

        if (Input.GetMouseButtonDown(0) && co == null)
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
            nSelectedTiles[tile.posX] += 1;
            tile.Select( () => co = StartCoroutine(ShiftTilesDown(tile.posX, tile.posY)));           
            
        }
        else {
            Debug.Log(" The gameObject hit is not a Tile! ");
        }
    }
    
    private IEnumerator ShiftTilesDown(int x, int firstEmptyY, float shiftDelay = .09f) {
        WaitForSeconds waitForShiftDelay = new WaitForSeconds(shiftDelay);   
        Transform tileAboveTransform;
        Vector3 shiftedPos;
        //Debug.Log("<color=green> Calling from </color>: " + tiles[x, yStart].name);
        int y = firstEmptyY;
        int nEmpty = 1;
        int shiftedY = 0;
        //If a match produced two empty tiles or more, all the board piece should shift of nEmpty tiles
        for (; y > 0; y--)
        {
            if(tiles[x, y - 1] == null)
            {
                nEmpty++;
            }
            else{
                break;
            }
        }

        for (y = firstEmptyY; y < dimY - 1; y++) {
       
            
            Tile tileAbove = tiles[x, y + 1];
            
            if (tileAbove != null) {
                tileAboveTransform = tileAbove.transform;
                shiftedPos = tileAboveTransform.position;
                shiftedPos.y -= tilesExtents.y * 2 * nEmpty;
                tileAboveTransform.position = shiftedPos;
                shiftedY = y - (nEmpty - 1);
                tileAbove.posY = shiftedY;
                tiles[x, shiftedY ] = tileAbove;
                tiles[x, y + 1] = null;
                yield return waitForShiftDelay;
            }
            else {
                // last tile becames null if there is nothing above it
                if ( (dimY - (y + 1) ) == nSelectedTiles[x])
                {
                    tiles[x, y] = null;
                    if(y > 0 && tiles[x,y - 1] == null)
                    {
                        Debug.Break();
                    }
                    break;
                }      
                else
                {
                    nEmpty++;
                }
            }
        }

        int firstShiftedTileY = firstEmptyY - (nEmpty - 1);
        if (firstShiftedTileY - 1 > 0 && tiles[x, firstShiftedTileY - 1] == null)
        {
            if(tiles[x, firstShiftedTileY] == null)
            {
                Debug.Log(" the tile that produced the error is " + x + " y = " + firstShiftedTileY);
                
            }
            else
            {
                tiles[x, firstShiftedTileY].GetComponent<Renderer>().material.color = Color.red;
            }
           
            Debug.Break();
        }
       
        for(int i = y; i > firstShiftedTileY && i >= 0; i--)
        {
            yield return StartCoroutine(CheckMatch(tiles[x, i]));
            PrintBoard();
        }
        co = null;
    }

    private IEnumerator CheckMatch(Tile startTile) {

        //Avoids to call check match on the tile above the last board piece of the column
        if(startTile == null){
            yield break;         
        }
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


        if (nCount > 1) {

#if DEBUG
          
            Debug.Log("<color=red> <---- start printing equal tiles found ----> </color>");
            string message = "Equal tile name: ";
            for (int i = 0; i < nCount; i++)
            {
                message += " " + equalTiles[i].gameObject.name;
            }

            Debug.Log(message);
#endif
            equalTiles[nCount] = startTile;
            nCount++;
            for(int i = 0; i < nCount; i++) {
                Select(equalTiles[i].gameObject);              
            }
        }

    }

    private void PrintBoard()
    {
        string content = "<--- Printing the board --->";
        
        
          
            for(int y = dimY - 1; y >= 0; y--)
            {
                content += "\n";
                for (int x = 0; x < dimX; x++)
                {
                        Tile tile = tiles[x, y];
                            if (tile != null)
                            {
                                content += $" {tile.name} ";
                            }
                            else
                            {
                                content += " ------- ";
                            }
                }
            }
        Debug.Log(content);
    }

}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

/// <summary>
/// This script manages the game: populating the board, calculating the matches and shifting the tiles
/// </summary>
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
    private Camera cam;
    private Ray ray;
    private RaycastHit raycastHit;
    private const int MAX_DIST = 1000;
    private const float SHIFT_SECONDS = 0.8f;
    #endregion


    protected override void Awake()
    {
        base.Awake();
        tiles = new Tile[dimX, dimY];
        layerMask = 1 << TILES_LAYER;
        cam = Camera.main;
        PopulateBoard();
    }
    /// <summary>
    ///  Gets tile dimension boundaries and dispose tiles 
    ///  inside the board rows and column starting from the given background bottom left corner
    /// </summary>
    private void PopulateBoard()
    {

        // Hipotesis: all the tiles will have the same boundary dimensions
        Tile tmp;
        Renderer backGroundRenderer = tilesSO.GetRandomTile().GetComponent<Renderer>();
        tilesExtents = backGroundRenderer.bounds.extents;

        Assert.IsNotNull(backGround, "You have not set the background component in the BoardManager");
        backGroundRenderer = backGround.GetComponent<Renderer>();
        if (backGroundRenderer == null)
        {
            Debug.LogError(" The background you provided has no renderer component!");
            return;
        }
        Vector3 bottomLeftCorner = GetBackgroundBottomLeftCorner(backGroundRenderer);
        //this simplifies the calculation of where the tiles should be one respect to another 
        bottomLeftCorner.x += tilesExtents.x;
        bottomLeftCorner.y += tilesExtents.y;

        Vector3 newPosition = Vector3.zero;
        newPosition.z = bottomLeftCorner.z;

        int[] previousLeft = new int[dimY];
        int previousBelow = 100;

        for (int x = 0; x < dimX; x++)
        {

            for (int y = 0; y < dimY; y++)
            {
                tmp = SpawnTile(previousLeft, previousBelow, x, y);
                tmp.transform.position = FindNewPosition(bottomLeftCorner, ref newPosition, x, y);
                tiles[x, y] = tmp;

                int tmpID = tmp.GetID();
                previousLeft[y] = tmpID;
                previousBelow = tmpID;
            }
        }

        Vector3 GetBackgroundBottomLeftCorner(Renderer renderer)
        {
            Vector3 backGroundExtents = renderer.bounds.extents;
            Vector3 backGroundPosition = backGround.transform.position;
            Vector3 bottomLeftCorner = new Vector3(backGroundPosition.x - backGroundExtents.x, backGroundPosition.y - backGroundExtents.y, backGroundPosition.z - tilesExtents.z);
            return bottomLeftCorner;
        }
    }



    private Vector3 FindNewPosition(Vector3 bottomLeftCorner, ref Vector3 newPosition, int x, int y)
    {
        newPosition.x = bottomLeftCorner.x + tilesExtents.x * (x * 2);
        newPosition.y = bottomLeftCorner.y + tilesExtents.y * (y * 2);
        return newPosition;
    }

    /// <summary>
    /// Spawns a tile considering the previousLeft spawned tiles and the one underneath
    /// </summary>
    /// <param name="previousLeft">Array of the previous spawned tiles</param>
    /// <param name="previousBelow"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    private Tile SpawnTile(int[] previousLeft, int previousBelow, int x, int y)
    {
        Tile tmp;
        if (x == 0 && y == 0)
        {
            tmp = Tile.SpawnTile(x, y);
        }
        else if (y == 0)
        {
            tmp = Tile.SpawnTile(x, y, previousLeft[y], previousLeft[y]);
        }
        else
        {
            tmp = Tile.SpawnTile(x, y, previousLeft[y], previousBelow);
        }

        return tmp;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && co == null)
        {

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
        if (tile != null)
        {
            tile.Select(() => co = StartCoroutine(ShiftTilesDown(tile.posX, tile.posY)));
        }
        else
        {
            Debug.Log(" The gameObject hit is not a Tile! ");
        }
    }

    private IEnumerator ShiftTilesDown(int x, int yStart)
    {
        WaitForSeconds waitForShiftDelay = new WaitForSeconds(SHIFT_SECONDS);
        yield return waitForShiftDelay;

        int y;
        for (y = yStart; y < dimY - 1; y++)
        {

            if (!ShiftTile(x, y + 1))
            {
                break;
            }
        }
        //Once the tiles shifted check if the action produced matches
        for (int i = yStart; i < y; i++)
        {
            Tile tmp = tiles[x, i];
            if (tmp == null)
            {
                break;
            }
            else
            {
                yield return CheckMatch(tmp);
            }
        }
        co = null;
    }

    /// <summary>
    /// Checks if there is a match comparing the start tile with the neighboring ones, 
    /// and if it finds more than 2 matches, it starts the matching process
    /// </summary>
    /// <param name="startTile">Tile to check</param>
    /// <returns></returns>
    private IEnumerator CheckMatch(Tile startTile)
    {

        if (startTile == null)
        {
            yield break;
        }

        //Debug.Log($" Checking tile {startTile.gameObject.name}");
        int nCount = 1;
        int y = startTile.posY;
        int startX = startTile.posX;
        int endX;

        for (int x = startX + 1; x < dimX; x++)
        {
            if (tiles[x, y] != null && startTile.CheckMatch(tiles[x, y]))
            {
                nCount++;
            }
            else
            {
                break;
            }
        }

        endX = startX + (nCount - 1);

        for (int x = startX - 1; x >= 0; x--)
        {
            if (tiles[x, y] != null && startTile.CheckMatch(tiles[x, y]))
            {
                nCount++;
            }
            else
            {
                break;
            }
        }

        startX = endX - (nCount - 1);

        if (nCount > 2)
        {
            yield return MakeMatch(startX, endX, y);
        }

    }

    /// <summary>
    /// Calls the action attributed to the match and shifts all the 
    /// tiles above the matching group one position underneath
    /// </summary>
    /// <param name="startX">first column index of the matching group</param>
    /// <param name="endX">last column index of the matching group</param>
    /// <param name="startY">row index of the matching group</param>
    /// <returns></returns>
    private IEnumerator MakeMatch(int startX, int endX, int startY)
    {
        //Play some effect on the tiles
        for (int i = startX; i <= endX; i++)
        {
            tiles[i, startY].Match();
        }
        yield return ShiftMatchedTilesDown(startX, endX, startY);
    }
    /// <summary>
    /// It will shift all the tiles above the matched one and it will also check for new matches starting from
    /// the bottom row going upper
    /// </summary>
    /// <param name="startX">X(column) where the match started</param>
    /// <param name="endX">X(column) where the match ended</param>
    /// <param name="startY">Y(row) where the match started</param>
    /// <returns></returns>
    private IEnumerator ShiftMatchedTilesDown(int startX, int endX, int startY)
    {
        //Debug.Log($"Shifting a match down startX {startX} and endX {endX} "); 
        WaitForSeconds waitForShiftDelay = new WaitForSeconds(SHIFT_SECONDS);
        int nMatched = (endX - startX) + 1;
        int nEmpty;
        int lastFullY = dimY - 1;
        yield return waitForShiftDelay;
        for (int y = startY; y < lastFullY; y++)
        {
            nEmpty = 0;
            for (int x = startX; x <= endX; x++)
            {
                if (!ShiftTile(x, y + 1))
                {
                    nEmpty++;
                }
            }
            //If it is trying to shift down only empty tiles on all the columns 
            //We can end the loop
            if (nEmpty == nMatched)
            {
                lastFullY = y;
                break;
            }
        }
        //After the tiles were shifted down, they could have created a positive situation for new matches
        for (int y = startY; y < lastFullY; y++)
        {
            yield return StartCoroutine(CheckMatch(tiles[startX, y]));
            yield return StartCoroutine(CheckMatch(tiles[endX, y]));
        }
    }

    /// <summary>
    /// Shifts the tile with the current x, y down of one position 
    /// </summary>
    /// <param name="x"> x of the tile that will shifted</param>
    /// <param name="y"> y of the tile that will be shifted</param>
    /// <returns>true if the tile was shifted</returns>
    private bool ShiftTile(int x, int y)
    {
        Tile tileToShift = tiles[x, y];
        if (tileToShift != null)
        {
            Transform tileToShiftTransform = tileToShift.transform;
            Vector3 shiftedPos = tileToShiftTransform.position;
            shiftedPos.y -= tilesExtents.y * 2;
            tileToShiftTransform.position = shiftedPos;
            tileToShift.posY = y - 1;
            tiles[x, y - 1] = tileToShift;
            tiles[x, y] = null;
            return true;
        }
        else
        {
            return false;
        }
    }


#if DEBUG
    /// <summary>
    /// Prints the board for debug reason starting from the top left corner
    /// </summary>
    private void PrintBoard()
    {
        string content = "<--- Printing the board --->";



        for (int y = dimY - 1; y >= 0; y--)
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
#endif
}

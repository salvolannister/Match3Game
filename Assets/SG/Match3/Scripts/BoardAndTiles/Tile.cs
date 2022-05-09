using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

/// <summary>
/// Base class from which every tile should derive from, it provides a basic match and select functions.
/// (For tile we intend the moving and customizable piece in the board)
/// </summary>
[RequireComponent(typeof(Renderer))]
public class Tile : MonoBehaviour
{

    [Header("Set in the inspector")]
    [SerializeField] private int iD;
  
    [HideInInspector] public int posX;
    [HideInInspector] public int posY;

    public static int nTile = 0;
    public static Tile SpawnTile(int x, int y)
    {
        Tile tile = Instantiate(Manager<BoardManager>.Get().tilesSO.GetRandomTile());
        InitTile(x, y, tile);
        return tile;
    }

    public static Tile SpawnTile(int x, int y, int previousLeftId, int previousBelowId)
    {
        Tile tile = Instantiate(Manager<BoardManager>.Get().tilesSO.GetRandomTile(previousLeftId, previousBelowId));
        InitTile(x, y, tile);
        return tile;
    }

    private static void InitTile(int x, int y, Tile tile)
    {
        tile.posX = x;
        tile.posY = y;
        tile.gameObject.name = "Tile " + nTile.ToString("00");
        nTile++;
    }

    /// <summary>
    /// This function could be used in the future also to add an effect on selection. The callback ensures that 
    /// every action related to the "Select" happens after the effect it's played. The method is virtual because this could allow 
    /// to have different behaviours for classes that hinerits that extends Tile
    /// </summary>
    public virtual void Select(Action callBack) {
        callBack?.Invoke();
        Destroy(gameObject);
    }

    public bool CheckMatch(Tile tile) {
        return tile.iD == iD;
    }

    public int GetID()
    {
        return iD;
    }
    /// <summary>
    /// This function could be used in the future also to add an effect on matches 
    /// based on the type of tile.
    /// </summary>
    public virtual void Match() {
        this.Select(null);
    }

}

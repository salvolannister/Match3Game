using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class Tile : MonoBehaviour {
    
    [Header("Set in the inspector")]
    [SerializeField] int iD;
    [Header("Set Dynamically")]
    [HideInInspector]public int posX;
    [HideInInspector]public int posY;

    public static int nTile = 0;
    public static Tile SpawnTile(int x, int y) {
        Tile tile = Instantiate(Manager<BoardManager>.Get().tilesSO.GetRandomTile());
        tile.posX = x;
        tile.posY = y;
        tile.gameObject.name = "Tile " + nTile.ToString("00");
        nTile++;
        return tile;
    }

    /// <summary>
    /// This function could be used in the future also to add effect on selection
    /// </summary>
    public void Select(Action callBack) {      
        callBack?.Invoke();
        Destroy(gameObject);
    }
   
    public bool CheckMatch(Tile tile)
    {
        return tile.iD == iD;  
    }
    
}

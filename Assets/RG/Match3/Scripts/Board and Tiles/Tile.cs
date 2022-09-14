using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour {
    
    [Header("Set in the inspector")]
    public int iD;
    [Header("Set Dynamically")]
    public int posX;
    public int posY;

    public static int nTile = 0;
    public static Tile SpawnTile(int x, int y) {
        Tile tile = Instantiate(BoardManager.S.tilesSO.GetRandomTile());
        tile.posX = x;
        tile.posY = y;
        tile.gameObject.name = "Tile " + nTile.ToString("00");
        nTile++;
        return tile;
    }

    /// <summary>
    /// This function could be used in the future also to add effect on selection
    /// </summary>
    public void Select() {      
        gameObject.SetActive(false);
    }
   
    public bool CheckMatch(Tile tile)
    {
        return tile.iD == iD;  
    }
    
}

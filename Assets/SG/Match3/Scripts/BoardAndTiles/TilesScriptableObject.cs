using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Scriptable Object used to customize the game: the designer won't need to change anything in the editor 
/// to change the graphic aspects of the game
/// </summary>
[CreateAssetMenu(menuName = "Scriptable Objects/TilesSO", fileName = "TilesSO.asset")]
[System.Serializable]
public class TilesScriptableObject : ScriptableObject
{

    [SerializeField]
    public Tile[] tilePrefabs;
   
    public Tile GetRandomTile() {
        int n = Random.Range(0, tilePrefabs.Length);
        return tilePrefabs[n];
    }

    public Tile GetRandomTile(int previousID, int previousLeft){

        int tileChosen;
        do{
            tileChosen = Random.Range(0, tilePrefabs.Length);
        }
        while (tileChosen == previousID || tileChosen == previousLeft);

        return tilePrefabs[tileChosen];
    }
}

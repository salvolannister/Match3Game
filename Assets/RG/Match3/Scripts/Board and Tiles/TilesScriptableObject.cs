using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/TilesSO", fileName = "TilesSO.asset")]
[System.Serializable]
public class TilesScriptableObject : ScriptableObject {

    [SerializeField]
    public Tile[] tilePrefabs;

    public Tile GetRandomTile()
    {
        int n = Random.Range(0, tilePrefabs.Length);
        return tilePrefabs[n];
    }
}

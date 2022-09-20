using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tiles : MonoBehaviour
{
    [SerializeField] private BaseTile[,,] childTiles;
    public BaseTile[,,] ChildTiles { get => childTiles; }

    // Start is called before the first frame update
    void Start()
    {
        childTiles = new BaseTile[21, 21, 21];
        BaseTile origin = GetComponentInChildren<BaseTile>();

    }

    public void AddTile()
    {

    }

    private BaseTile GetTileByCoordinate(int x, int y, int z)
    {
        return childTiles[x-10, y-10, z-10];
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

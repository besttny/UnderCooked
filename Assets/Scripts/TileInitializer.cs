using UnityEngine;
using System.Collections.Generic;

public class GridObjectInitializer : MonoBehaviour
{
    public int width = 10;
    public int height = 10;
    public float tileSize = 1f;

    public Vector3 gridOrigin = Vector3.zero; 

    public Tile[,] grid;
    public List<Tile> gridDebug = new List<Tile>();


    void Start()
    {
        grid = new Tile[width, height];
        InitTilesFromScene();

        gridDebug.Clear();

        for (int x = 0; x < width; x++)
            for (int z = 0; z < height; z++)
                gridDebug.Add(grid[x, z]);
    }

    void InitTilesFromScene()
    {
        foreach (Floor f in Object.FindObjectsByType<Floor>(FindObjectsSortMode.None))
        {
            GetXZFromPosition(f.transform.position, out int x, out int z);

            if (InBounds(x, z))
            {
                f.Init(x, z);
                grid[x, z] = f;
            }
        }

        foreach (Counter c in Object.FindObjectsByType<Counter>(FindObjectsSortMode.None))
        {
            GetXZFromPosition(c.transform.position, out int x, out int z);

            if (InBounds(x, z))
            {
                c.Init(x, z);
                grid[x, z] = c;
            }
        }

        Debug.Log("Grid initialized from positions");
    }

    void GetXZFromPosition(Vector3 pos, out int x, out int z)
    {
        Vector3 local = pos - gridOrigin;

        x = Mathf.RoundToInt(local.x / tileSize);
        z = Mathf.RoundToInt(local.z / tileSize);
    }

    bool InBounds(int x, int z)
    {
        return x >= 0 && z >= 0 && x < width && z < height;
    }
}

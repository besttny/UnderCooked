using UnityEngine;

public class Floor : Tile
{
    public override void Init(int gridX, int gridZ)
    {
        base.Init(gridX, gridZ);
        walkable = true;
    }
}
using UnityEngine;

public class Tile : MonoBehaviour
{
    public int x;
    public int z;
    public bool walkable;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public virtual void Init(int gridX, int gridZ)
    {
        x = gridX;
        z = gridZ;
        walkable = true; 
    }
    
}

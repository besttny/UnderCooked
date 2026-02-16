using UnityEngine;

public class Choppable : MonoBehaviour
{
    public float chopTime = 3f;              // ⭐ cutting time depends on ingredient
    public GameObject choppedResultPrefab;
    public bool destroyOriginal = true;
}
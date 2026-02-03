using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridMapBuilder : MonoBehaviour
{
    [Header("Grid Size")]
    public int width = 10;
    public int height = 10;
    public float tileSize = 1f;

    [Header("Prefabs")]
    public GameObject floorPrefab;
    public GameObject counterPrefab;

    [Header("Heights")]
    public float floorY = 0f;
    public float counterY = 0.55f;

    [Header("Parents")]
    public Transform floorParent;
    public Transform counterParent;
    public Transform spawnPointsParent;
    public Transform ingredientParent;

    [Header("Spawn points on counter border")]
    public bool createSpawnPointsOnCounter = true;
    public float spawnPointYOffset = 0.55f;

    [Header("Ingredient Spawner Settings")]
    public List<Ingredient> ingredientTypes = new List<Ingredient>();
    public int maxPerIngredient = 5;
    public float spawnDelay = 3.0f;

    public List<Transform> counterSpawnPoints = new List<Transform>();
    public List<Transform> floorSpawnPoints = new List<Transform>();

    [System.Serializable]
    public class Ingredient
    {
        public string name;
        public GameObject prefab;
        [HideInInspector] public List<GameObject> spawnedInstances = new List<GameObject>();
    }

    private void Start()
    {
        // 1. Build map if lists are empty (failsafe for runtime)
        if (floorSpawnPoints.Count == 0 && counterSpawnPoints.Count == 0)
        {
            BuildMap();
        }

        // 2. SPAWN ONE IMMEDIATELY on Play
        TrySpawnRandomIngredient();

        // 3. Start the repeating spawn routine
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnDelay);

            // Clean up null references if items were destroyed
            foreach (var ing in ingredientTypes)
            {
                ing.spawnedInstances.RemoveAll(item => item == null);
            }

            TrySpawnRandomIngredient();
        }
    }

    void TrySpawnRandomIngredient()
    {
        if (ingredientTypes.Count == 0) return;

        int index = Random.Range(0, ingredientTypes.Count);
        Ingredient target = ingredientTypes[index];

        if (target.spawnedInstances.Count < maxPerIngredient && target.prefab != null)
        {
            Transform point = GetEmptySpawnPoint();
            if (point != null)
            {
                // Spawn 0.1 units above the floor tile center
                Vector3 spawnPos = point.position + new Vector3(0, 0.1f, 0);
                GameObject newItem = Instantiate(target.prefab, spawnPos, Quaternion.identity);
                
                if (ingredientParent != null) newItem.transform.SetParent(ingredientParent);
                
                target.spawnedInstances.Add(newItem);
            }
        }
    }

    Transform GetEmptySpawnPoint()
    {
        if (floorSpawnPoints.Count == 0) return null;

        // Create a copy of the floor list and shuffle it
        List<Transform> shuffledPoints = new List<Transform>(floorSpawnPoints);
        for (int i = 0; i < shuffledPoints.Count; i++)
        {
            int rnd = Random.Range(i, shuffledPoints.Count);
            Transform temp = shuffledPoints[i];
            shuffledPoints[i] = shuffledPoints[rnd];
            shuffledPoints[rnd] = temp;
        }

        foreach (var p in shuffledPoints)
        {
            // FIX: Check 0.5 units above the floor so we don't hit the floor's own collider
            Vector3 checkPos = p.position + Vector3.up * 0.5f; 
            
            // If the sphere (radius 0.3) hits nothing, the space is clear
            if (Physics.OverlapSphere(checkPos, 0.3f).Length == 0) 
            {
                return p;
            }
        }
        return null;
    }

    [ContextMenu("Build Map")]
    public void BuildMap()
    {
        EnsureParents();
        ClearChildren();
        counterSpawnPoints.Clear();
        floorSpawnPoints.Clear();

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                Vector3 basePos = new Vector3(x * tileSize, 0f, z * tileSize);
                bool isBorder = (x == 0 || z == 0 || x == width - 1 || z == height - 1);

                // 1. Create Floor tiles everywhere
                if (floorPrefab != null)
                {
                    var floor = Instantiate(floorPrefab, new Vector3(basePos.x, floorY, basePos.z), Quaternion.identity, floorParent);
                    floor.name = $"Floor_{x}_{z}";

                    // Only the center (non-border) tiles are valid for random spawning
                    if (!isBorder)
                    {
                        floorSpawnPoints.Add(floor.transform);
                    }
                }

                // 2. Create Counters on the border only
                if (isBorder && counterPrefab != null)
                {
                    var counter = Instantiate(counterPrefab, new Vector3(basePos.x, counterY, basePos.z), Quaternion.identity, counterParent);
                    counter.name = $"Counter_{x}_{z}";

                    // Optional: Create specific spawn points on top of counters
                    if (createSpawnPointsOnCounter)
                    {
                        var sp = new GameObject($"SP_{x}_{z}").transform;
                        sp.SetParent(spawnPointsParent);
                        sp.position = new Vector3(basePos.x, spawnPointYOffset, basePos.z);
                        counterSpawnPoints.Add(sp);
                    }
                }
            }
        }
    }

    void EnsureParents()
    {
        if (floorParent == null) floorParent = GetOrCreate("Floor");
        if (counterParent == null) counterParent = GetOrCreate("Counters");
        if (spawnPointsParent == null) spawnPointsParent = GetOrCreate("SpawnPoints");
        if (ingredientParent == null) ingredientParent = GetOrCreate("Ingredients");
    }

    Transform GetOrCreate(string name)
    {
        var go = GameObject.Find(name);
        if (go == null) go = new GameObject(name);
        go.transform.SetParent(transform);
        return go.transform;
    }

    void ClearChildren()
    {
        if (floorParent != null) while (floorParent.childCount > 0) DestroyImmediate(floorParent.GetChild(0).gameObject);
        if (counterParent != null) while (counterParent.childCount > 0) DestroyImmediate(counterParent.GetChild(0).gameObject);
        if (spawnPointsParent != null) while (spawnPointsParent.childCount > 0) DestroyImmediate(spawnPointsParent.GetChild(0).gameObject);
    }
}
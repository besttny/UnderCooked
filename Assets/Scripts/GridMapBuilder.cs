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

    [Header("Floor Checkerboard")]
    public bool checkerboard = true;
    public bool invertPattern = false;
    public Material floorWhiteMat;
    public Material floorBlackMat;

    [Header("Spawn points on counter border")]
    public bool createSpawnPointsOnCounter = true;

    [Header("Ingredient Spawner Settings")]
    public List<Ingredient> ingredientTypes = new List<Ingredient>();
    public int maxPerIngredient = 5;
    public float spawnDelay = 3.0f;

    [Header("Advanced Spawn Control")]
    public int maxTotalItemsOnMap = 12;
    public float minItemSpacing = 0.6f;
    public float spawnHeightOffset = 0.05f;
    public LayerMask itemLayerMask; // ตั้งเป็น Layer "Item" ใน Inspector

    [System.Serializable]
    public class Ingredient
    {
        public string name;
        public GameObject prefab;
        [HideInInspector] public List<GameObject> spawnedInstances = new List<GameObject>();
    }

    // spawn points
    public List<Transform> counterSpawnPoints = new List<Transform>();
    public List<Transform> floorSpawnPoints = new List<Transform>(); // เผื่ออนาคต

    // lock จุด spawn ว่ามีของอยู่แล้ว
    private readonly Dictionary<Transform, GameObject> pointToItem = new Dictionary<Transform, GameObject>();

    void Start()
    {
        // ถ้ายังไม่ build มาก่อน ให้ build ตอนเริ่มเล่น
        if (counterSpawnPoints.Count == 0 && floorSpawnPoints.Count == 0)
            BuildMap();

        // spawn ทันที 1 ครั้ง
        CleanupSpawnedLists();
        TrySpawnRandomIngredient();

        // spawn ตามเวลา
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnDelay);
            CleanupSpawnedLists();
            TrySpawnRandomIngredient();
        }
    }

    void CleanupSpawnedLists()
    {
        foreach (var ing in ingredientTypes)
            ing.spawnedInstances.RemoveAll(x => x == null);

        var keys = new List<Transform>(pointToItem.Keys);
        foreach (var k in keys)
            if (pointToItem[k] == null) pointToItem.Remove(k);
    }

    int GetTotalAliveItems()
    {
        int total = 0;
        foreach (var ing in ingredientTypes) total += ing.spawnedInstances.Count;
        return total;
    }

    void TrySpawnRandomIngredient()
    {
        if (ingredientTypes == null || ingredientTypes.Count == 0) return;

        // จำกัดจำนวนรวมบนแมพ
        if (GetTotalAliveItems() >= maxTotalItemsOnMap) return;

        // เลือกเฉพาะชนิดที่ยังไม่เต็มโควต้า และมี prefab
        List<Ingredient> candidates = new List<Ingredient>();
        foreach (var ing in ingredientTypes)
        {
            if (ing.prefab == null) continue;
            if (ing.spawnedInstances.Count < maxPerIngredient)
                candidates.Add(ing);
        }
        if (candidates.Count == 0) return;

        Ingredient target = candidates[Random.Range(0, candidates.Count)];

        Transform point = GetEmptySpawnPoint();
        if (point == null) return;

        Vector3 spawnPos = point.position + Vector3.up * spawnHeightOffset;
        GameObject newItem = Instantiate(target.prefab, spawnPos, Quaternion.identity);

        // ✅ บังคับให้ของอยู่ใต้ LevelBuilder/__Generated/Ingredients เสมอ
        newItem.transform.SetParent(GetOrCreateIngredientsRoot());

        target.spawnedInstances.Add(newItem);
        pointToItem[point] = newItem;
    }

    Transform GetEmptySpawnPoint()
    {
        // เกิดบนเคาน์เตอร์รอบขอบเป็นหลัก
        List<Transform> source = (counterSpawnPoints.Count > 0) ? counterSpawnPoints : floorSpawnPoints;
        if (source.Count == 0) return null;

        // สุ่มลำดับ
        List<Transform> shuffled = new List<Transform>(source);
        for (int i = 0; i < shuffled.Count; i++)
        {
            int rnd = Random.Range(i, shuffled.Count);
            (shuffled[i], shuffled[rnd]) = (shuffled[rnd], shuffled[i]);
        }

        foreach (var p in shuffled)
        {
            // จุดนี้มีของอยู่แล้วไหม
            if (pointToItem.ContainsKey(p) && pointToItem[p] != null) continue;

            // กันเกิดชิดเกิน (เช็คเฉพาะ item layer)
            Collider[] hits = Physics.OverlapSphere(p.position, minItemSpacing, itemLayerMask);
            if (hits.Length == 0) return p;
        }
        return null;
    }

    Transform GetOrCreateGeneratedRoot()
    {
        var t = transform.Find("__Generated");
        if (t != null) return t;

        var go = new GameObject("__Generated");
        go.transform.SetParent(transform);
        return go.transform;
    }

    Transform GetOrCreateIngredientsRoot()
    {
        Transform genRoot = GetOrCreateGeneratedRoot();
        var t = genRoot.Find("Ingredients");
        if (t != null) return t;

        var go = new GameObject("Ingredients");
        go.transform.SetParent(genRoot);
        return go.transform;
    }

    void ClearGenerated(Transform root)
    {
        for (int i = root.childCount - 1; i >= 0; i--)
        {
            if (Application.isPlaying) Destroy(root.GetChild(i).gameObject);
            else DestroyImmediate(root.GetChild(i).gameObject);
        }
    }

    [ContextMenu("Build Map")]
    public void BuildMap()
    {
        Transform genRoot = GetOrCreateGeneratedRoot();
        ClearGenerated(genRoot);

        counterSpawnPoints.Clear();
        floorSpawnPoints.Clear();
        pointToItem.Clear();

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                Vector3 basePos = new Vector3(x * tileSize, 0f, z * tileSize);
                bool isBorder = (x == 0 || z == 0 || x == width - 1 || z == height - 1);

                // Floor
                if (floorPrefab != null)
                {
                    var floor = Instantiate(
                        floorPrefab,
                        new Vector3(basePos.x, floorY, basePos.z),
                        Quaternion.identity,
                        genRoot
                    );
                    floor.name = $"Floor_{x}_{z}";
                    ApplyCheckerboardMaterial(floor, x, z);

                    if (!isBorder) floorSpawnPoints.Add(floor.transform);
                }

                // Counter (border)
                if (isBorder && counterPrefab != null)
                {
                    var counter = Instantiate(
                        counterPrefab,
                        new Vector3(basePos.x, counterY, basePos.z),
                        Quaternion.identity,
                        genRoot
                    );
                    counter.name = $"Counter_{x}_{z}";

                    // Spawn point บนหน้าเคาน์เตอร์จริง
                    if (createSpawnPointsOnCounter)
                    {
                        var sp = new GameObject($"SP_{x}_{z}").transform;
                        sp.SetParent(genRoot);

                        float topY = counter.transform.position.y + 0.5f;
                        var col = counter.GetComponent<Collider>();
                        if (col != null) topY = col.bounds.max.y;

                        sp.position = new Vector3(basePos.x, topY + spawnHeightOffset, basePos.z);
                        counterSpawnPoints.Add(sp);
                    }
                }
            }
        }

        // สร้าง Ingredients root ไว้เลย (กันไม่มีตอนเริ่มเกม)
        GetOrCreateIngredientsRoot();
    }

    void ApplyCheckerboardMaterial(GameObject floorObj, int x, int z)
    {
        if (!checkerboard) return;
        if (floorWhiteMat == null || floorBlackMat == null) return;

        bool isWhite = ((x + z) % 2 == 0);
        if (invertPattern) isWhite = !isWhite;

        var r = floorObj.GetComponentInChildren<Renderer>();
        if (r != null) r.sharedMaterial = isWhite ? floorWhiteMat : floorBlackMat;
    }
}

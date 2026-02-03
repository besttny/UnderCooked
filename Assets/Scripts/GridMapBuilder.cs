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
    public float counterY = 0.55f; // ปรับให้วางบนพื้นพอดี (พื้น 0.1, counter สูง 1 => 0.55)

    [Header("Parents (auto-created if empty)")]
    public Transform floorParent;
    public Transform counterParent;
    public Transform spawnPointsParent;

    [Header("Spawn points on counter border")]
    public bool createSpawnPointsOnCounter = true;
    public float spawnPointYOffset = 0.55f; // จุดเกิดของบน counter (มักเท่ากับ counterY)

    // เก็บ list เผื่อเอาไปใช้ spawn วัตถุดิบ/อุปกรณ์
    public List<Transform> counterSpawnPoints = new List<Transform>();
    public Tile[,] grid;

    [ContextMenu("Build Map")]
    public void BuildMap()
    {
        EnsureParents();
        ClearChildren();

        counterSpawnPoints.Clear();

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                Vector3 basePos = new Vector3(x * tileSize, 0f, z * tileSize);

                // Floor ทุกช่อง
                if (floorPrefab != null)
                {
                    var floor = Instantiate(
                        floorPrefab,
                        new Vector3(basePos.x, floorY, basePos.z),
                        Quaternion.identity,
                        floorParent
                    );
                    floor.name = $"Floor_{x}_{z}";
                }

                // Counter เฉพาะขอบ
                bool isBorder = (x == 0 || z == 0 || x == width - 1 || z == height - 1);
                if (isBorder && counterPrefab != null)
                {
                    var counter = Instantiate(
                        counterPrefab,
                        new Vector3(basePos.x, counterY, basePos.z),
                        Quaternion.identity,
                        counterParent
                    );
                    counter.name = $"Counter_{x}_{z}";
                    counter.layer = LayerMask.NameToLayer("Interactable");


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
        if (floorParent == null)
        {
            var go = GameObject.Find("Floor");
            if (go == null) go = new GameObject("Floor");
            floorParent = go.transform;
            floorParent.SetParent(transform);
        }

        if (counterParent == null)
        {
            var go = GameObject.Find("Counters");
            if (go == null) go = new GameObject("Counters");
            counterParent = go.transform;
            counterParent.SetParent(transform);
        }

        if (spawnPointsParent == null)
        {
            var go = GameObject.Find("SpawnPoints");
            if (go == null) go = new GameObject("SpawnPoints");
            spawnPointsParent = go.transform;
            spawnPointsParent.SetParent(transform);
        }
    }

    void ClearChildren()
    {
        // ลบของเดิมก่อนสร้างใหม่ (ใช้ DestroyImmediate เพราะใช้กับ ContextMenu ใน editor)
        for (int i = floorParent.childCount - 1; i >= 0; i--)
            DestroyImmediate(floorParent.GetChild(i).gameObject);

        for (int i = counterParent.childCount - 1; i >= 0; i--)
            DestroyImmediate(counterParent.GetChild(i).gameObject);

        for (int i = spawnPointsParent.childCount - 1; i >= 0; i--)
            DestroyImmediate(spawnPointsParent.GetChild(i).gameObject);
    }
}

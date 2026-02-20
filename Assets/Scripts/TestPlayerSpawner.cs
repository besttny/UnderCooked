using UnityEngine;

public class TestPlayerSpawner : MonoBehaviour
{
    public GameObject playerPrefab;

    public Vector3 p1Spawn = new Vector3(4f, 0.6f, 4.5f);
    public Vector3 p2Spawn = new Vector3(6f, 0.6f, 4.5f);
    public Vector3 p3Spawn = new Vector3(5f, 0.6f, 4.5f);

    void Start()
    {
        if (playerPrefab == null) return;

        SpawnPlayer("Player1", p1Spawn, 0, Color.red);
        SpawnPlayer("Player2", p2Spawn, 1, Color.cyan);
        SpawnPlayer("Player3", p3Spawn, 2, new Color(0.6f, 0f, 1f)); // purple
    }

    void SpawnPlayer(string name, Vector3 pos, int index, Color color)
    {
        var p = Instantiate(playerPrefab, pos, Quaternion.identity);
        p.name = name;

        p.GetComponent<PlayerInputBridge>()?.SetPlayerIndex(index);

        // ⭐ Find the indicator pyramid mesh
        var renderer = p.transform
            .Find("Indicator/Pyramid/default")
            ?.GetComponent<MeshRenderer>();

        if (renderer != null)
        {
            renderer.material.color = color;
        }
    }
}
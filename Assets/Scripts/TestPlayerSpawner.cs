using UnityEngine;

public class TestPlayerSpawner : MonoBehaviour
{
    public GameObject playerPrefab;

    public Vector3 p1Spawn = new Vector3(4.5f, 0.6f, 4.5f);
    public Vector3 p2Spawn = new Vector3(6.0f, 0.6f, 4.5f);
    public Vector3 p3Spawn = new Vector3(5.25f, 0.6f, 6.0f);

    void Start()
    {
        if (playerPrefab == null) return;

        var p1 = Instantiate(playerPrefab, p1Spawn, Quaternion.identity);
        p1.name = "Player1";
        p1.GetComponent<PlayerInputBridge>()?.SetPlayerIndex(0);

        var p2 = Instantiate(playerPrefab, p2Spawn, Quaternion.identity);
        p2.name = "Player2";
        p2.GetComponent<PlayerInputBridge>()?.SetPlayerIndex(1);

        var p3 = Instantiate(playerPrefab, p3Spawn, Quaternion.identity);
        p3.name = "Player3";
        p3.GetComponent<PlayerInputBridge>()?.SetPlayerIndex(2);
    }
}

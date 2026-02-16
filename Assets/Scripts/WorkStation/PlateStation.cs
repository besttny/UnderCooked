using UnityEngine;

public class PlateStation : Workstation
{
    public GameObject platePrefab;

    void Start()
    {
        TrySpawnPlate();
    }

    void Update()
    {
        // if plate taken → spawn new one
        if (placePoint.childCount == 0)
        {
            TrySpawnPlate();
        }
    }

    void TrySpawnPlate()
    {
        if (platePrefab == null) return;
        if (placePoint == null) return;
        if (placePoint.childCount > 0) return;

        GameObject plate = Instantiate(
            platePrefab,
            placePoint.position,
            Quaternion.identity,
            placePoint
        );

        // make it behave like placed item
        foreach (var col in plate.GetComponentsInChildren<Collider>())
            col.enabled = true;

        var rb = plate.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }

    // not used (station is passive)
    public override void Use(GameObject player) { }
}

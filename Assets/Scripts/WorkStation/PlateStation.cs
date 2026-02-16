using UnityEngine;

public class PlateStation : Workstation
{
    public GameObject platePrefab;

    public override void Use(GameObject player)
    {
        if (player == null) return;
        Debug.Log("Using Plate Station!");

        var combat = player.GetComponent<PlayerCombat>();
        if (combat == null) return;
        if (combat.heldItem != null) return;
        var interact = player.GetComponent<PlayerInteract>();
        if (interact.holdPoint == null) return;

        // spawn plate
        GameObject plate = Instantiate(
            platePrefab,
            interact.holdPoint.position,
            Quaternion.identity
        );

        // give to player
        combat.heldItem = plate;

        plate.transform.SetParent(interact.holdPoint);
        plate.transform.localPosition = Vector3.zero;
        plate.transform.localRotation = Quaternion.identity;

        // disable physics while held
        foreach (var col in plate.GetComponentsInChildren<Collider>())
            col.enabled = false;
    }
}

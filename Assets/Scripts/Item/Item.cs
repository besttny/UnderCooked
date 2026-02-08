using UnityEngine;

public class Item : MonoBehaviour, IInteractable
{
    public string itemName;
    public Collider[] cols; 
    public bool isHeld = false;

    public void Awake()
    {
        cols = GetComponentsInChildren<Collider>();
    }

    public void Interact(PlayerController player)
    {
        Debug.Log("Player interacted with Item: " + itemName);
        if (!player.HasItem())
        {
            // pick up item
            player.PickupItem(this.gameObject);
        }
    }
    public void SetHeld(bool isHeld)
    {
        this.isHeld = isHeld;
        foreach (Collider col in cols)
        {
            col.enabled = !isHeld;
        }
    }

}
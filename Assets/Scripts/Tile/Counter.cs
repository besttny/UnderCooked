using UnityEngine;

public class Counter : Tile, IInteractable
{
    
    public GameObject currentItem; //holditem
    public override void Init(int gridX, int gridZ)
    {
        base.Init(gridX, gridZ);
        walkable = false;
    }
    public void TrySpawnItem()
    {
        /*if (holdItemPrefab == null) return;
        if (currentItem != null) return;

        Vector3 pos = spawnPoint != null
            ? spawnPoint.position
            : transform.position + Vector3.up * 0.6f;

        currentItem = Instantiate(holdItemPrefab, pos, Quaternion.identity, transform);*/
    }
    public bool HasItem()
    {
        return currentItem != null;
    }
    public GameObject TakeItem() // remove item from counter
    {
        if (currentItem == null) return null;

        GameObject item = currentItem;
        currentItem = null;
        item.transform.SetParent(null);

        return item;
    }
    public void PlaceItem(GameObject item) // place item on counter
    {
        if (currentItem != null) return;

        currentItem = item;
        //item.transform.SetParent(transform);
        item.transform.position = transform.position + Vector3.up * 0.6f;

        Item itemComp = item.GetComponent<Item>();
        if (itemComp != null)
        {
            itemComp.SetHeld(false);
        }
    }

    public void Interact(PlayerController player)
    {
        Debug.Log("Player interacted with Counter at (" + x + ", " + z + ")");
        if(player.HasItem())
        {
            if(!HasItem())
            {
                // place item on counter
                GameObject item = player.TakeItem();
                PlaceItem(item);
            }
        }
        else
        {
            if(HasItem())
            {
                GameObject item = TakeItem();
                player.PickupItem(item);
            }
        }
    }
}
using UnityEngine;

public class PanStation : Workstation
{
    public Pan pan;

    void Update()
    {
        if (pan != null)
            pan.TickCook(Time.deltaTime);
    }

    public override bool TryPlaceItem(GameObject item, GameObject player)
    {
        if (pan == null) return false;

        if (pan.TryInsert(item))
        {
            Debug.Log("Item placed on pan");
            return true;
        }

        Debug.Log("Pan refused item");
        return false;
    }
    
    public override void Use(GameObject player)
    {
        var controller = player.GetComponent<PlayerCombat>();
        if (controller == null) return;

        GameObject cooked = pan.TakeItem();
        if (cooked != null)
        {
            cooked.transform.SetParent(player.transform);
            cooked.transform.localPosition = Vector3.forward * 0.6f;
            controller.heldItem = cooked;
        }
    }
}
using UnityEngine;

public class Workstation : MonoBehaviour
{
    public Transform placePoint;

    protected GameObject currentItem; // ⭐ STORE ITEM HERE

    // =========================
    // PLACE ITEM INTO STATION
    // =========================
    public virtual bool TryPlaceItem(GameObject item, GameObject player)
    {
        if (currentItem != null) return false;

        currentItem = item;

        item.transform.SetParent(placePoint);
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.identity;

        // Disable physics
        Rigidbody rb = item.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        foreach (var col in item.GetComponents<Collider>())
            col.enabled = false;

        return true;
    }

    // =========================
    // TAKE ITEM (E PICKUP)
    // =========================
    public virtual GameObject TakeItem()
    {
        if (currentItem == null) return null;

        GameObject item = currentItem;
        currentItem = null;

        return item;
    }

    public virtual bool HasItem()
    {
        return currentItem != null;
    }

    // =========================
    // INTERACT (PRESS F)
    // =========================
    public virtual void Use(GameObject player)
    {
    }

    // =========================
    public Transform GetPlacePoint()
    {
        return placePoint;
    }
}
using UnityEngine;

public class PanStation : MonoBehaviour
{
    public Transform cookPoint;

    GameObject currentItem;
    Cookable currentCookable;

    float cookTimer = 0f;
    bool isCooking = false;
    bool isCooked = false;

    void Update()
    {
        if (!isCooking || currentCookable == null) return;

        cookTimer += Time.deltaTime;

        if (cookTimer >= currentCookable.cookTime)
        {
            FinishCooking();
        }
    }

    public void Interact(PlayerCombat player)
    {
        // =========================
        // PLAYER HOLDING ITEM → PLACE
        // =========================
        if (player.heldItem != null)
        {
            if (currentItem != null) return;

            Cookable cookable = player.heldItem.GetComponent<Cookable>();
            if (cookable == null) return;

            currentItem = player.heldItem;
            currentCookable = cookable;
            player.heldItem = null;

            PlaceOnPan(currentItem);

            cookTimer = 0f;
            isCooking = true;
            isCooked = false;

            return;
        }

        // =========================
        // PLAYER EMPTY HAND → PICKUP
        // =========================
        if (player.heldItem == null && currentItem != null)
        {
            if (!isCooked)
            {
                Debug.Log("Still cooking...");
                return;
            }

            GiveToPlayer(player);
        }
    }

    void PlaceOnPan(GameObject item)
    {
        item.transform.SetParent(cookPoint);
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.identity;

        foreach (var col in item.GetComponentsInChildren<Collider>())
            col.enabled = false;

        Rigidbody rb = item.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }

    void FinishCooking()
    {
        isCooking = false;
        isCooked = true;

        GameObject result = currentItem;

        if (currentCookable.cookedResultPrefab != null)
        {
            result = Instantiate(
                currentCookable.cookedResultPrefab,
                cookPoint.position,
                cookPoint.rotation,
                cookPoint
            );
        }

        if (currentCookable.destroyOriginal)
        {
            Destroy(currentItem);
        }

        currentItem = result;
        currentCookable = currentItem.GetComponent<Cookable>(); // optional if cooked also cookable
    }

    void GiveToPlayer(PlayerCombat player)
    {
        currentItem.transform.SetParent(player.transform);
        currentItem.transform.localPosition = Vector3.forward * 0.6f;

        foreach (var col in currentItem.GetComponentsInChildren<Collider>())
            col.enabled = false;

        Rigidbody rb = currentItem.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        player.heldItem = currentItem;

        currentItem = null;
        currentCookable = null;
        isCooking = false;
        isCooked = false;
    }
}

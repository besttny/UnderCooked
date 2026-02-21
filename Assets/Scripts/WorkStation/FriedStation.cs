using UnityEngine;
using System.Collections;

public class FriedStation : Workstation
{
    //public GameObject currentItem;
    public ProcessingBarUI progressUI;

    bool busy = false;

    // =========================
    // PLACE ITEM → AUTO COOK
    // =========================

    void LateUpdate()
    {
        if (!placePoint) return;

        if (placePoint.childCount > 0)
            currentItem = placePoint.GetChild(0).gameObject;
        else
            currentItem = null;
    }
    
    public override bool TryPlaceItem(GameObject item, GameObject player)
    {
        if (currentItem != null) return false;

        currentItem = item;

        item.transform.SetParent(placePoint);
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.identity;

        var cookable = item.GetComponent<Cookable>();

        // disable physics while cooking
        if(cookable != null){
            foreach (var c in item.GetComponentsInChildren<Collider>())
                c.enabled = false;
        }

        var rb = item.GetComponent<Rigidbody>();
        if (rb)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        Debug.Log("Item placed → auto cooking");

        if (cookable != null)
            StartCoroutine(CookRoutine(cookable));
        else
            Debug.LogWarning("Item has no Cookable!");

        return true;
    }

    // =========================
    // PRESS F = PICK UP (when done)
    // =========================
    public override void Use(GameObject player)
    {
        if (busy) return;
        if (currentItem == null) return;

        var combat = player.GetComponent<PlayerCombat>();
        if (combat == null) return;
        if (combat.heldItem != null) return;

        combat.heldItem = currentItem;
        currentItem.transform.SetParent(null);

        foreach (var c in currentItem.GetComponentsInChildren<Collider>())
            c.enabled = true;

        var rb = currentItem.GetComponent<Rigidbody>();
        if (rb)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        currentItem = null;
    }

    IEnumerator CookRoutine(Cookable cookable)
    {
        busy = true;

        float timer = 0f;
        float cookTime = cookable.cookTime;

        if (progressUI) progressUI.Show();

        while (timer < cookTime)
        {
            timer += Time.deltaTime;
            if (progressUI) progressUI.SetProgress(timer / cookTime);
            yield return null;
        }

        // FINISH
        GameObject rawItem = currentItem;

        if (cookable.cookedResultPrefab != null)
        {
            GameObject result = Instantiate(
                cookable.cookedResultPrefab,
                placePoint.position,
                Quaternion.identity,
                placePoint
            );

            currentItem = result;
        }
        else
        {
            currentItem = null;
        }

        if (cookable.destroyOriginal && rawItem)
            Destroy(rawItem);

        busy = false;
        if (progressUI) progressUI.Hide();

        Debug.Log("Cooking complete!");
    }

    public override GameObject TakeItem()
    {
        if (currentItem == null) return null;
        if (busy) return null;

        GameObject item = currentItem;
        currentItem = null;

        return item;
    }
}

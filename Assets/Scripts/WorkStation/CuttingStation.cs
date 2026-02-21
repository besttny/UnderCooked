using UnityEngine;
using System.Collections;

public class CuttingBoardStation : Workstation
{
    //public GameObject currentItem; 
    public ProcessingBarUI progressUI;

    bool busy = false;

    // =========================
    // PLACE ITEM (called by PlayerInteract)
    // =========================
    public override bool TryPlaceItem(GameObject item, GameObject player)
    {
        var chopable = item.GetComponent<Choppable>();

        if (currentItem != null) return false;

        currentItem = item;

        item.transform.SetParent(placePoint);
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.identity;

        // disable physics while on board
        if(chopable != null){
             foreach (var c in item.GetComponentsInChildren<Collider>())
                c.enabled = false;
        }
        var rb = item.GetComponent<Rigidbody>();
        if (rb)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        Debug.Log("Ingredient placed on cutting board");
        return true;
    }

    // =========================
    // PRESS F TO CHOP
    // =========================
    public override void Use(GameObject player)
    {
        Debug.Log("Cutting board used!");

        if (busy) return;
        if (currentItem == null) return;

        var controller = player.GetComponent<PlayerController>();
        if (controller == null) return;

        var chop = currentItem.GetComponent<Choppable>();
        if (chop == null)
        {
            Debug.Log("Item not choppable");
            return;
        }

        StartCoroutine(ChopRoutine(controller, chop));
    }

    IEnumerator ChopRoutine(PlayerController controller, Choppable chop)
    {
        busy = true;

        float timer = 0f;
        float chopTime = chop.chopTime;

        if (progressUI) progressUI.Show();

        while (timer < chopTime)
        {
            if (controller.IsMoving)
            {
                Debug.Log("Chop canceled — player moved");
                busy = false;
                if (progressUI) progressUI.Hide();
                yield break;
            }

            timer += Time.deltaTime;
            if (progressUI) progressUI.SetProgress(timer / chopTime);

            yield return null;
        }

        // FINISH
        GameObject rawItem = currentItem;

        if (chop.choppedResultPrefab != null)
        {
            GameObject result = Instantiate(
                chop.choppedResultPrefab,
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

        if (chop.destroyOriginal && rawItem)
            Destroy(rawItem);

        busy = false;
        if (progressUI) progressUI.Hide();

        Debug.Log("Chop complete!");
    }

    // auto detect item on board
    void LateUpdate()
    {
        if (!placePoint) return;

        if (placePoint.childCount > 0)
            currentItem = placePoint.GetChild(0).gameObject;
        else
            currentItem = null;
    }
}

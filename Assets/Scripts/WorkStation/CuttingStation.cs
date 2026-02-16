using UnityEngine;
using System.Collections;

public class CuttingBoardStation : Workstation
{
    public float chopTime = 5f; // time in seconds to chop

    public GameObject currentItem;
    bool busy = false;

    public override void Use(GameObject player)
    {
        Debug.Log("Cutting board used!");
        if (busy) return;

        var combat = player.GetComponent<PlayerCombat>();
        var interact = player.GetComponent<PlayerInteract>();
        var controller = player.GetComponent<PlayerController>();

        if (combat == null || interact == null || controller == null) return;

        // ---------- PLACE ITEM ----------
       /*if (currentItem == null && combat.heldItem != null)
        {
            currentItem = combat.heldItem;
            combat.heldItem = null;

            currentItem.transform.SetParent(placePoint);
            currentItem.transform.localPosition = Vector3.zero;
            currentItem.transform.localRotation = Quaternion.identity;

            return;
        }*/

        // ---------- START CHOP ----------
        if (currentItem != null)
        {
            var chop = currentItem.GetComponent<Choppable>();
            if (chop == null) return;

            StartCoroutine(ChopRoutine(player, controller, chop));
        }
    }

    IEnumerator ChopRoutine(GameObject player, PlayerController controller, Choppable chop)
    {
        busy = true;
        float t = 0f;

        while (t < chopTime)
        {
            if (controller.IsMoving)
            {
                Debug.Log("Chop canceled — player moved");
                busy = false;
                yield break;
            }

            t += Time.deltaTime;
            yield return null;
        }

        // ---- FINISH ----
        Vector3 pos = placePoint.position;

        if (chop.choppedResultPrefab != null)
        {
            Instantiate(chop.choppedResultPrefab, pos, Quaternion.identity, placePoint);
        }

        if (chop.destroyOriginal)
            Destroy(currentItem);

        currentItem = null;
        busy = false;

        Debug.Log("Chop complete!");
    }

    void LateUpdate()
    {
        if (placePoint == null) return;

        if (placePoint.childCount > 0)
        {
            var obj = placePoint.GetChild(0).gameObject;
            if (currentItem != obj)
            {
                currentItem = obj;
                Debug.Log($"CuttingBoard currentItem set to {obj.name}");
            }
        }
        else
        {
            currentItem = null;
        }
    }

}

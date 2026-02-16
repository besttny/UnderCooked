using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    [Header("Carry")]
    public Transform holdPoint;
    public float interactRadius = 2.5f;

    [Header("Layers")]
    public LayerMask itemLayer;
    public LayerMask counterLayer;

    [Header("Drop")]
    public float dropForward = 0.8f;
    public float dropUp = 0.25f;
    public float placeHeightOffset = 0.05f;

    PlayerCombat combat;
    PlayerStatus status;

    void Awake()
    {
        combat = GetComponent<PlayerCombat>();
        status = GetComponent<PlayerStatus>();

        if (holdPoint == null)
        {
            GameObject hp = new GameObject("HoldPoint");
            hp.transform.SetParent(transform);
            hp.transform.localPosition = new Vector3(0, 1.0f, 0.4f);
            holdPoint = hp.transform;
        }
    }

    public void OnInteract()
    {
        Debug.Log("INTERACT PRESSED");

        if (status != null && status.IsStunned) return;
        if (combat == null) return;

        if (combat.heldItem == null)
            TryPickup();
        else
            TryPlace();
    }

    // =====================================================
    // PICKUP
    // =====================================================
    void TryPickup()
    {
        var hits = Physics.OverlapSphere(transform.position, interactRadius, itemLayer);

        if (hits.Length == 0)
        {
            Debug.Log("No item to pickup");
            return;
        }

        GameObject closest = null;
        float bestDist = float.MaxValue;

        foreach (var h in hits)
        {
            var go = h.attachedRigidbody ? h.attachedRigidbody.gameObject : h.gameObject;
            float d = Vector3.Distance(transform.position, go.transform.position);

            if (d < bestDist)
            {
                bestDist = d;
                closest = go;
            }
        }

        if (closest == null) return;

        // =====================================================
        // ⭐ PLATE INTERACTION FIRST
        // =====================================================
        if (combat.heldItem != null)
        {
            Plate plate = combat.heldItem.GetComponent<Plate>();

            if (plate != null)
            {
                Ingredient ingredient = closest.GetComponent<Ingredient>();

                if (ingredient != null && ingredient.canPlate)
                {
                    bool added = plate.TryAddIngredient(closest);

                    if (added)
                    {
                        Debug.Log("Added ingredient to plate");
                        return; // STOP pickup
                    }
                }
            }
        }

        // =====================================================
        // ⭐ NORMAL PICKUP
        // =====================================================
        Debug.Log("Picked up: " + closest.name);
        // ⭐⭐⭐ ADD THIS BLOCK ⭐⭐⭐
        Ingredient ing = closest.GetComponent<Ingredient>();
        if (ing != null && ing.canPlate && !ing.alreadyScored)
        {
            PlayerScore ps = GetComponent<PlayerScore>();
            if (ps != null)
                ps.AddScore(ing.scoreValue);
            ing.alreadyScored = true;
        }
        // ⭐⭐⭐ END BLOCK ⭐⭐⭐

        combat.heldItem = closest;
        SetItemPhysics(closest, true);

        closest.transform.SetParent(holdPoint);
        closest.transform.localPosition = Vector3.zero;

        // apply special rotation for plate
        if (closest.GetComponent<Plate>() != null)
        {
            // rotate plate flat
            closest.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        }
        else
        {
            // normal items
            closest.transform.localRotation = Quaternion.identity;
        }
    }

    // =====================================================
    // PLACE / INTERACT WITH STATION
    // =====================================================
    void TryPlace()
    {
        GameObject held = combat.heldItem;
        if (held == null) return;

        // =====================================================
        // ⭐ 1. PLATE + INGREDIENT COMBINE (TOP PRIORITY)
        // =====================================================
        Plate plate = held.GetComponent<Plate>();
        if (plate != null)
        {
            var itemHits = Physics.OverlapSphere(transform.position, interactRadius, itemLayer);

            GameObject closestIngredient = null;
            float bestDist = float.MaxValue;

            foreach (var h in itemHits)
            {
                var go = h.attachedRigidbody ? h.attachedRigidbody.gameObject : h.gameObject;

                Ingredient ing = go.GetComponent<Ingredient>();
                if (ing == null || !ing.canPlate) continue;

                float d = Vector3.Distance(transform.position, go.transform.position);
                if (d < bestDist)
                {
                    bestDist = d;
                    closestIngredient = go;
                }
            }

            if (closestIngredient != null)
            {
                bool added = plate.TryAddIngredient(closestIngredient);

                if (added)
                {
                    Debug.Log("Ingredient added to plate");
                    return; // STOP — do not place on counter
                }
            }
        }

        // =====================================================
        // ⭐ 2. STATION / COUNTER DETECTION
        // =====================================================
        var hits = Physics.OverlapSphere(transform.position, interactRadius, counterLayer);

        Collider closest = null;
        float bestDistCounter = float.MaxValue;

        foreach (var h in hits)
        {
            float d = Vector3.Distance(transform.position, h.transform.position);
            if (d < bestDistCounter)
            {
                bestDistCounter = d;
                closest = h;
            }
        }

        if (closest != null)
        {
            var station = closest.GetComponentInParent<Workstation>();

            // =====================================================
            // ⭐ 3. WORKSTATION
            // =====================================================
            if (station != null)
            {
                Debug.Log("Using station: " + station.name);

                Transform placePoint = station.GetPlacePoint();
                if (placePoint != null && placePoint.childCount > 0)
                {
                    Debug.Log("Station already occupied");
                    return;
                }

                bool placed = station.TryPlaceItem(held, gameObject);

                if (placed)
                    combat.heldItem = null;
                else
                    Debug.Log("Station rejected item");

                return;
            }

            // =====================================================
            // ⭐ 4. NORMAL COUNTER PLACE
            // =====================================================
            foreach (Transform child in closest.transform)
            {
                int bit = 1 << child.gameObject.layer;
                if ((itemLayer.value & bit) != 0)
                {
                    Debug.Log("Counter already occupied");
                    return;
                }
            }

            Debug.Log("Placed on counter");

            float topY = closest.bounds.max.y;

            Vector3 pos = new Vector3(
                closest.bounds.center.x,
                topY + placeHeightOffset,
                closest.bounds.center.z
            );

            held.transform.SetParent(closest.transform);
            held.transform.position = pos;
            held.transform.rotation = Quaternion.identity;

            SetItemPhysics(held, false, true);
            combat.heldItem = null;
            return;
        }

        // =====================================================
        // ⭐ 5. DROP ON FLOOR
        // =====================================================
        Debug.Log("Dropped on floor");

        held.transform.SetParent(null);
        held.transform.position =
            transform.position +
            transform.forward * dropForward +
            Vector3.up * dropUp;

        SetItemPhysics(held, false, false);
        combat.heldItem = null;
    }


    // =====================================================
    // PHYSICS HANDLING
    // =====================================================
    void SetItemPhysics(GameObject item, bool carried, bool placedOnCounter = false)
    {
        foreach (var col in item.GetComponents<Collider>())
            col.enabled = !carried;

        var rb = item.GetComponent<Rigidbody>();
        if (rb == null) return;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        if (carried || placedOnCounter)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
        else
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }
    }
}

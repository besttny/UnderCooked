using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    [Header("Carry")]
    public Transform holdPoint;              // จุดถือของ (สร้างเป็นลูกของ Player/Model แล้วลากมาใส่)
    public float pickupRadius = 3.0f;

    [Header("Layers")]
    public LayerMask itemLayer;
    public LayerMask counterLayer;

    [Header("Placement")]
    public float placeHeightOffset = 0.05f;
    public float dropForward = 0.8f;
    public float dropUp = 0.25f;

    PlayerCombat combat;
    PlayerStatus status;

    void Awake()
    {
        combat = GetComponent<PlayerCombat>();
        status = GetComponent<PlayerStatus>();

        if (holdPoint == null)
        {
            // fallback: ถ้าไม่ลากมา จะสร้างให้ใต้ตัว Player เลย
            var hp = new GameObject("HoldPoint");
            hp.transform.SetParent(transform);
            hp.transform.localPosition = new Vector3(0, 1.0f, 0.35f);
            holdPoint = hp.transform;
        }
    }

    // ถูกเรียกจาก PlayerInputBridge ผ่าน SendMessage("OnInteract")
    public void OnInteract()
    {
        Debug.Log($"[PlayerInteract] OnInteract on {gameObject.name}");
        if (status != null && status.IsStunned) return;
        if (combat == null) return;

        if (combat.heldItem == null) TryPickup();
        else TryPlaceOrDrop();
    }

    void TryPickup()
    {
        var hits = Physics.OverlapSphere(transform.position, pickupRadius, itemLayer);
        if (hits.Length == 0) return;

        // หาอันที่ใกล้สุด
        GameObject best = null;
        float bestD = float.MaxValue;

        foreach (var h in hits)
        {
            var go = h.attachedRigidbody != null ? h.attachedRigidbody.gameObject : h.gameObject;
            float d = Vector3.Distance(transform.position, go.transform.position);
            if (d < bestD)
            {
                bestD = d;
                best = go;
            }
        }

        if (best == null) return;

        // จับของขึ้นมือ
        combat.heldItem = best;

        SetItemPhysics(best, carried: true);

        best.transform.SetParent(holdPoint);
        best.transform.localPosition = Vector3.zero;
        best.transform.localRotation = Quaternion.identity;
    }

    void TryPlaceOrDrop()
    {
        var item = combat.heldItem;
        if (item == null) return;

        // หาเคาน์เตอร์ใกล้สุด
        var counters = Physics.OverlapSphere(transform.position, pickupRadius, counterLayer);

        Collider bestCol = null;
        float bestD = float.MaxValue;

        foreach (var c in counters)
        {
            float d = Vector3.Distance(transform.position, c.transform.position);
            if (d < bestD)
            {
                bestD = d;
                bestCol = c;
            }
        }

        if (bestCol != null)
        {
            // วางบนเคาน์เตอร์
            float topY = bestCol.bounds.max.y;
            Vector3 placePos = new Vector3(bestCol.bounds.center.x, topY + placeHeightOffset, bestCol.bounds.center.z);

            item.transform.SetParent(bestCol.transform);
            item.transform.position = placePos;

            SetItemPhysics(item, carried: false, placedOnCounter: true);

            combat.heldItem = null;
            return;
        }

        // ไม่มีเคาน์เตอร์ใกล้ ๆ -> หล่นลงพื้นด้านหน้า
        item.transform.SetParent(null);
        item.transform.position = transform.position + transform.forward * dropForward + Vector3.up * dropUp;

        SetItemPhysics(item, carried: false, placedOnCounter: false);

        combat.heldItem = null;
    }

    void SetItemPhysics(GameObject item, bool carried, bool placedOnCounter = false)
    {
        // colliders: ตอนถือ ปิด, ตอนวาง/หล่น เปิด (เพื่อให้หยิบใหม่ได้)
        foreach (var col in item.GetComponentsInChildren<Collider>())
            col.enabled = !carried;

        var rb = item.GetComponent<Rigidbody>();
        if (rb == null) return;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        if (carried)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
        else if (placedOnCounter)
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

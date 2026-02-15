using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Weapon Visual")]
    public GameObject rollingPinVisual;
    public Transform weaponOrigin; // ถ้าไม่ใส่จะใช้ transform

    [Header("Attack")]
    public float attackRange = 1.2f;
    public float attackRadius = 0.6f;
    public float stunDuration = 0.8f;
    public float cooldown = 0.6f;
    public LayerMask playerLayer;

    [Header("Hand (for later pickup system)")]
    public GameObject heldItem;

    float nextAttackTime = 0f;

    void Update()
    {
        bool emptyHand = (heldItem == null);
        if (rollingPinVisual != null) rollingPinVisual.SetActive(emptyHand);
    }

    public void OnAttackPressed()
    {
        // ดีบัค: ถ้าอยากเช็คว่าปุ่มยิงมาถึงจริงไหม ให้เปิดบรรทัดนี้
        Debug.Log($"{name} Attack!");

        if (Time.time < nextAttackTime) return;
        if (heldItem != null) return;

        nextAttackTime = Time.time + cooldown;

        Transform origin = (weaponOrigin != null) ? weaponOrigin : transform;
        Vector3 center = origin.position + origin.forward * attackRange;

        var hits = Physics.OverlapSphere(center, attackRadius, playerLayer, QueryTriggerInteraction.Collide);

        // ดีบัค: ดูว่าหาเจอกี่ตัว
        // Debug.Log($"{name} hits: {hits.Length}");

        foreach (var h in hits)
        {
            // กันตีโดนตัวเอง (เทียบ root)
            if (h.transform.root == transform.root) continue;

            var st = h.GetComponentInParent<PlayerStatus>();
            if (st != null) st.Stun(stunDuration);

            var otherCombat = h.GetComponentInParent<PlayerCombat>();
            if (otherCombat != null && otherCombat.heldItem != null)
                otherCombat.DropHeldItem();

            break;
        }
    }

    public void DropHeldItem()
    {
        if (heldItem == null) return;

        heldItem.transform.SetParent(null);

        // เปิด collider คืน เพื่อให้หยิบใหม่ได้
        foreach (var col in heldItem.GetComponentsInChildren<Collider>())
            col.enabled = true;

        var rb = heldItem.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        heldItem.transform.position = transform.position + transform.forward * 0.8f + Vector3.up * 0.3f;
        heldItem = null;
    }
    void OnDrawGizmosSelected()
    {
        Transform origin = (weaponOrigin != null) ? weaponOrigin : transform;
        Vector3 center = origin.position + origin.forward * attackRange;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(center, attackRadius);
    }
}

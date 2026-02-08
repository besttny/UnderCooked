using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float dashPower = 8f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 1f;
    public float rotationSpeed = 10f; // New: smoother rotation
    public float interactDistance = 1.5f;
    public LayerMask interactMask;
    public GameObject holdItem;
    public Transform ingredientRoot;
    public float ItemOffsetY = 0.5f;
    public float dropDistance = 0.3f;

    private Vector2 moveInput;
    private Rigidbody rb;
    private bool isDashing;
    private float dashEndTime;
    private float lastDashTime;
    private Vector3 dashDirection; // New: store dash direction
    

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // Important: Lock rotation so physics doesn't interfere
        rb.freezeRotation = true;

        // Optional: Adjust drag for smoother stopping
        rb.linearDamping = 0f;

        ingredientRoot = GameObject.Find("__Generated/Ingredients")?.transform;

    }

    void FixedUpdate()
    {
        if (isDashing)
        {
            if (Time.time >= dashEndTime)
            {
                isDashing = false;
                // Reset velocity after dash for cleaner transition
                rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            }
            else
            {
                // Maintain dash velocity
                rb.linearVelocity = new Vector3(dashDirection.x * dashPower, rb.linearVelocity.y, dashDirection.z * dashPower);
            }
            return;
        }

        // Normal movement
        Vector3 move = new Vector3(moveInput.x, 0, moveInput.y).normalized; // Normalize for consistent speed
        rb.linearVelocity = new Vector3(move.x * moveSpeed, rb.linearVelocity.y, move.z * moveSpeed);

        // Smooth rotation
        if (move != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnDash(InputValue value)
    {
        if (!value.isPressed) return;
        if (Time.time < lastDashTime + dashCooldown) return;
        if (moveInput == Vector2.zero) return;
        if (isDashing) return; // Prevent dash overlap

        dashDirection = new Vector3(moveInput.x, 0, moveInput.y).normalized;
        rb.linearVelocity = dashDirection * dashPower;

        isDashing = true;
        dashEndTime = Time.time + dashDuration;
        lastDashTime = Time.time;
    }
    public bool HasItem()
    {
        return holdItem != null;
    }
    public void PickupItem(GameObject item) // place item on player
    {
        holdItem = item;

        item.transform.SetParent(transform);
        item.transform.localPosition = new Vector3(0, ItemOffsetY, 0.5f);

        Item itemComp = item.GetComponent<Item>();
        if (itemComp != null)
        {
            itemComp.SetHeld(true);
        }
    }
    public GameObject TakeItem() // remove item from player
    {
        if (holdItem == null) return null;

        GameObject item = holdItem;
        holdItem = null;

        if (ingredientRoot != null)
            item.transform.SetParent(ingredientRoot);
        else
            item.transform.SetParent(null);

        return item;
    }

    public void OnInteract(InputValue value)
    {
        if (!value.isPressed) return;

        Vector3 dir = transform.forward;
        Vector3 origin = transform.position + Vector3.up * 0f;

        Collider[] nearHits = Physics.OverlapSphere( // nearby check
            origin + dir * 0.4f,
            0.6f,
            interactMask,
            QueryTriggerInteraction.Ignore
        );

        IInteractable closest = null;
        float bestDist = float.MaxValue;

        foreach (var col in nearHits)
        {
            var interactable = col.GetComponentInParent<IInteractable>();
            if (interactable == null) continue;

            float d = Vector3.Distance(origin, col.transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                closest = interactable;
            }
        }

        if (closest != null)
        {
            closest.Interact(this);
            return;
        }

        if (Physics.SphereCast( // distance check
                origin,
                0.5f,
                dir,
                out RaycastHit hit,
                interactDistance,
                interactMask,
                QueryTriggerInteraction.Ignore))
        {
            var interactable = hit.collider.GetComponentInParent<IInteractable>();
            if (interactable != null)
                interactable.Interact(this);
        }
    }

    public void DropItem()
    {
        if (holdItem == null) return;

        GameObject item = holdItem;
        holdItem = null;

        item.transform.SetParent(
            ingredientRoot != null ? ingredientRoot : null
        );

        Vector3 dropPos =
            transform.position +
            transform.forward * dropDistance;
        dropPos.y = 0.1f; 


        item.transform.position = dropPos;

        // re-enable colliders
        Item itemComp = item.GetComponent<Item>();
        if (itemComp != null)
        {
            itemComp.SetHeld(false);
        }
    }

    public void OnDrop(InputValue value)
    {
        if (!value.isPressed) return;
        DropItem();
    }


}
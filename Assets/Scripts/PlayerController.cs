using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float dashPower = 8f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 1f;
    public float rotationSpeed = 10f; // New: smoother rotation
    public bool IsMoving => moveInput.sqrMagnitude > 0.01f; // check if player is moving

    private Vector2 moveInput;
    private Rigidbody rb;
    private bool isDashing;
    private float dashEndTime;
    private float lastDashTime;
    private Vector3 dashDirection; // New: store dash direction
    private PlayerStatus status;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        status = GetComponent<PlayerStatus>();

        // Important: Lock rotation so physics doesn't interfere
        rb.freezeRotation = true;

        // Optional: Adjust drag for smoother stopping
        rb.linearDamping = 0f;
    }

    void FixedUpdate()
    {
        if (status != null && status.IsStunned)
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            return;
        }
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

    public void OnMove(Vector2 v)
    {
        moveInput = v;
    }

    public void OnDash()
    {
        // จำลองเหมือนกดปุ่ม
        if (Time.time < lastDashTime + dashCooldown) return;
        if (moveInput == Vector2.zero) return;
        if (isDashing) return;

        dashDirection = new Vector3(moveInput.x, 0, moveInput.y).normalized;
        rb.linearVelocity = dashDirection * dashPower;

        isDashing = true;
        dashEndTime = Time.time + dashDuration;
        lastDashTime = Time.time;
    }

    public void OnInteract()
    {
        Debug.Log($"{name} Interact!");
    }
}
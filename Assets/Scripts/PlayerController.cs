using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float dashPower = 8f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 1f;
    public float rotationSpeed = 10f; // New: smoother rotation

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
        rb.drag = 0f;
    }

    void FixedUpdate()
    {
        if (isDashing)
        {
            if (Time.time >= dashEndTime)
            {
                isDashing = false;
                // Reset velocity after dash for cleaner transition
                rb.velocity = new Vector3(0, rb.velocity.y, 0);
            }
            else
            {
                // Maintain dash velocity
                rb.velocity = new Vector3(dashDirection.x * dashPower, rb.velocity.y, dashDirection.z * dashPower);
            }
            return;
        }

        // Normal movement
        Vector3 move = new Vector3(moveInput.x, 0, moveInput.y).normalized; // Normalize for consistent speed
        rb.velocity = new Vector3(move.x * moveSpeed, rb.velocity.y, move.z * moveSpeed);

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
        rb.velocity = dashDirection * dashPower;

        isDashing = true;
        dashEndTime = Time.time + dashDuration;
        lastDashTime = Time.time;
    }

    public void OnInteract(InputValue value)
    {
        if (value.isPressed)
            Debug.Log("Interact!");
    }
}
using UnityEngine;

public class Player2KeyboardInput : MonoBehaviour
{
    private PlayerController controller;

    void Awake()
    {
        controller = GetComponent<PlayerController>();
    }

    void Update()
    {
        if (controller == null) return;

        // Arrow keys -> move
        float x = 0f, y = 0f;
        if (Input.GetKey(KeyCode.LeftArrow))  x -= 1f;
        if (Input.GetKey(KeyCode.RightArrow)) x += 1f;
        if (Input.GetKey(KeyCode.DownArrow))  y -= 1f;
        if (Input.GetKey(KeyCode.UpArrow))    y += 1f;

        controller.OnMove(new Vector2(x, y));

        // Dash
        if (Input.GetKeyDown(KeyCode.RightShift))
            controller.OnDash();

        // Interact
        if (Input.GetKeyDown(KeyCode.Return))
            controller.OnInteract();

        // Attack (ถ้าคุณทำเมธอดไว้แล้วค่อยปลด comment)
        if (Input.GetKeyDown(KeyCode.RightControl))
            SendMessage("OnAttackPressed", SendMessageOptions.DontRequireReceiver);
    }
}

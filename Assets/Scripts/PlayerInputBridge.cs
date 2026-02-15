using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputBridge : MonoBehaviour
{
    [Tooltip("0 = Player (WASD), 1 = Player2 (Arrow Keys)")]
    public int playerIndex = 0;

    private PlayerControls controls;
    private InputActionMap map;
    private InputAction move, dash, interact, attack;

    void Awake()
    {
        controls = new PlayerControls();
    }

    void OnEnable()
    {
        BindForIndex(playerIndex);
    }

    void OnDisable()
    {
        Unbind();
    }

    public void SetPlayerIndex(int index)
    {
        playerIndex = index;
        if (isActiveAndEnabled)
        {
            Unbind();
            BindForIndex(playerIndex);
        }
    }

    void BindForIndex(int index)
    {
        string mapName = (index == 0) ? "Player" : "Player2";

        map = controls.asset.FindActionMap(mapName, true);

        move = map.FindAction("Move", true);
        dash = map.FindAction("Dash", true);
        interact = map.FindAction("Interact", true);
        attack = map.FindAction("Attack", true);

        // ✅ เปลี่ยนชื่อ handler กันชนกับ SendMessage("OnMove", Vector2)
        move.performed += HandleMove;
        move.canceled  += HandleMoveCancel;

        dash.performed += _ => SendMessage("OnDash", SendMessageOptions.DontRequireReceiver);
        interact.performed += _ =>
        {
            Debug.Log($"[Bridge] Interact fired on {gameObject.name} (map={map.name}, index={playerIndex})");
            SendMessage("OnInteract", SendMessageOptions.DontRequireReceiver);
        };
        attack.performed += _ => SendMessage("OnAttackPressed", SendMessageOptions.DontRequireReceiver);

        map.Enable();
    }

    void Unbind()
    {
        if (move != null)
        {
            move.performed -= HandleMove;
            move.canceled  -= HandleMoveCancel;
        }

        if (map != null) map.Disable();

        map = null;
        move = dash = interact = attack = null;
    }

    void HandleMove(InputAction.CallbackContext ctx)
    {
        Vector2 v = ctx.ReadValue<Vector2>();
        SendMessage("OnMove", v, SendMessageOptions.DontRequireReceiver);
    }

    void HandleMoveCancel(InputAction.CallbackContext ctx)
    {
        SendMessage("OnMove", Vector2.zero, SendMessageOptions.DontRequireReceiver);
    }
}

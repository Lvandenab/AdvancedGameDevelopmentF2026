using UnityEngine;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(CharacterController))]
public class Playermov : MonoBehaviour
{
    public float speed = 5f;
    public float sensitivity = 2f;
    public float gravity = 0f;

    // Optional: assign the player's camera (usually a child) to apply pitch rotation
    public Transform playerCamera;
    private float _pitch;

    private CharacterController _controller;
    private Vector3 _verticalVelocity = Vector3.zero;

    void Start()
    {
        _controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Cursor lock/unlock: both input systems supported
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            return;
        }

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && Cursor.lockState != CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
#else
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            return;
        }

        if (Input.GetMouseButtonDown(0) && Cursor.lockState != CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
#endif

        Vector3 input = Vector3.zero;
        float mouseX = 0f;
        float mouseY = 0f;

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        // Keyboard WASD / arrow keys using new Input System (low-level)
        if (Keyboard.current != null)
        {
            float h = (Keyboard.current.dKey.isPressed ? 1f : 0f) - (Keyboard.current.aKey.isPressed ? 1f : 0f);
            float v = (Keyboard.current.wKey.isPressed ? 1f : 0f) - (Keyboard.current.sKey.isPressed ? 1f : 0f);
            input = new Vector3(h, 0f, v);
        }

        // Mouse delta (delta is already frame-based)
        if (Mouse.current != null)
        {
            Vector2 delta = Mouse.current.delta.ReadValue();
            mouseX = delta.x;
            mouseY = delta.y;
        }
#else
        // Legacy Input system
        float hLegacy = Input.GetAxis("Horizontal");
        float vLegacy = Input.GetAxis("Vertical");
        input = new Vector3(hLegacy, 0f, vLegacy);

        mouseX = Input.GetAxis("Mouse X");
        mouseY = Input.GetAxis("Mouse Y");
#endif

        if (input.sqrMagnitude > 1f) input.Normalize();

        // Horizontal movement (in local space) using CharacterController to respect collisions
        Move(input);

        // Gravity
        if (_controller.isGrounded && _verticalVelocity.y < 0f)
        {
            // small negative to keep the controller grounded
            _verticalVelocity.y = -1f;
        }

        _verticalVelocity.y -= gravity * Time.deltaTime;
        _controller.Move(_verticalVelocity * Time.deltaTime);

        // Mouse look: yaw the player, pitch the camera
        Rotate(new Vector3(0f, mouseX, 0f));

        if (playerCamera != null)
        {
            _pitch -= mouseY * sensitivity * Time.deltaTime;
            _pitch = Mathf.Clamp(_pitch, -85f, 85f);
            playerCamera.localEulerAngles = new Vector3(_pitch, 0f, 0f);
        }
    }

    public void Move(Vector3 direction)
    {
        // Convert local input to world movement relative to player orientation,
        // then move via CharacterController so collisions are handled.
        Vector3 worldMove = transform.TransformDirection(direction) * speed;
        _controller.Move(worldMove * Time.deltaTime);
    }

    public void Rotate(Vector3 direction)
    {
        // direction.y is expected to be raw mouse X; apply sensitivity once here.
        transform.Rotate(direction * sensitivity * Time.deltaTime);
    }
}

using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;           // Movement speed
    public float rotationSpeed = 720f;    // Rotation speed

    private PlayerControls controls;      // Input actions
    private Vector2 moveInput;            // Left stick input
    private Vector2 lookInput;            // Right stick input

    private Rigidbody rb;

    private void Awake()
    {
        // Initialize the input actions
        controls = new PlayerControls();

        // Bind input actions to methods
        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        controls.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        controls.Player.Look.canceled += ctx => lookInput = Vector2.zero;
    }

    private void OnEnable()
    {
        // Enable the input actions
        controls.Player.Enable();
    }

    private void OnDisable()
    {
        // Disable the input actions
        controls.Player.Disable();
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Freeze rotation on physics to avoid unwanted behavior
        if (rb != null)
        {
            rb.freezeRotation = true;
        }
    }

    private void Update()
    {
        // ** Move Player with Left Stick **
        Vector3 moveDirection = new Vector3(moveInput.x, 0, moveInput.y);
        if (moveDirection.magnitude > 0.1f)
        {
            rb.MovePosition(rb.position + moveDirection * moveSpeed * Time.deltaTime);
        }

        // ** Rotate Player with Right Stick **
        Vector3 lookDirection = new Vector3(lookInput.x, 0, lookInput.y);
        if (lookDirection.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection, Vector3.up);
            rb.rotation = Quaternion.RotateTowards(rb.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}

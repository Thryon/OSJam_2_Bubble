using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;           // Movement speed
    public float rotationSpeed = 720f;    // Rotation speed
    public float jumpForce = 5f;          // Force applied for jumping
    public LayerMask groundLayer;         // Layer mask to check if grounded
    public float dashForce = 100f;
    public ParticleSystem dash_VFX;
    private float dashCooldown = 1f;
    private float lastDashTime;


    private PlayerControls controls;      // Input actions
    private Vector2 moveInput;            // Left stick input
    private Vector2 lookInput;            // Right stick input
    private bool isGrounded;              // Tracks if the player is on the ground
    private bool jumpInput;               // Tracks if jump button is pressed
    private bool dashInput;

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

        controls.Player.Jump.performed += ctx => jumpInput = true;
        controls.Player.Jump.canceled += ctx => jumpInput = false;

        controls.Player.Dash.performed += ctx => dashInput = true;
        controls.Player.Dash.canceled += ctx => dashInput = false;
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


        // ** Jump Logic **
        if (jumpInput && isGrounded)
        {
            Debug.Log("Jump executed: Player is grounded!");
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false; // Prevent double jumps
        }
        if (jumpInput)
        {
            Debug.Log("Jump input detected!");
        }

        // dash logic
        if (dashInput && Time.time > lastDashTime + dashCooldown)
        {
            Debug.Log("Dash input detected!");
            Debug.Log($"LookDirection: {lookDirection}");
            lookDirection = transform.forward;
            rb.AddForce(lookDirection * dashForce, ForceMode.Impulse);
            if (dash_VFX != null && !dash_VFX.isPlaying)
            {
                dash_VFX.Play(); // Trigger the particle effect
            }
            dashInput = false;
            lastDashTime = Time.time;
        }
    }

    private void FixedUpdate()
    {
        // Check if player is grounded using a small spherecast
        isGrounded = Physics.CheckSphere(transform.position + Vector3.up * 0.48f, 0.5f, groundLayer);
    }

    private void OnDrawGizmosSelected()
    {
        // Visualize the ground check in the editor
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position + Vector3.up * 0.48f, 0.5f);
    }
}

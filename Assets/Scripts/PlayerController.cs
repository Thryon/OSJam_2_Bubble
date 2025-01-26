using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;          // Movement speed
    public float rotationSpeed = 720f;    // Rotation speed
    public float jumpForce = 5f;          // Force applied for jumping
    public LayerMask groundLayer;         // Layer mask to check if grounded
    public float dashForce = 100f;
    public ParticleSystem dash_VFX;
    private float dashCooldown = 1f;
    private float lastDashTime;
    private bool isStunned = false;
    private PlayerState playerState;

    private Vector2 moveInput;            // Left stick input
    private Vector2 lookInput;            // Right stick input
    private bool isGrounded;              // Tracks if the player is on the ground
    private bool jumpInput;               // Tracks if jump button is pressed
    private bool dashInput;

    public GunScript Gun;

    private Rigidbody rb;
    private PlayerInput playerInput;

    public void OnMove(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            moveInput = context.ReadValue<Vector2>();
        }
        else if (context.canceled)
        {
            moveInput = Vector2.zero;
        }
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            lookInput = context.ReadValue<Vector2>();
        }
        else if (context.canceled)
        {
            lookInput = Vector2.zero;
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            jumpInput = true;
        }
        else if (context.canceled)
        {
            jumpInput = false;
        }
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            dashInput = true;
        }
        else if (context.canceled)
        {
            dashInput = false;
        }
    }

    public void OnShoot(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Gun.StartShooting();
        }
        else if(context.canceled)
        {
            Gun.ConfirmShoot();
        }
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();
        // Freeze rotation on physics to avoid unwanted behavior
        if (rb != null)
        {
            rb.freezeRotation = true;
        }
    }

    private void Update()
    {
        if (playerState != null)
        {
            isStunned = playerState.currentHealth >= playerState.playerHP;
        }
        
        HandleMovement();

        Vector3 lookDirection;
        if (playerInput.currentControlScheme == "KeyboardMouse" || playerInput.currentControlScheme == "Touch")
        {
            var screenToWorldLookPos = Camera.main.ScreenToWorldPoint(lookInput);;
            lookDirection = (screenToWorldLookPos - transform.position).normalized;
        }
        else
        {
            lookDirection = new Vector3(lookInput.x, 0, lookInput.y);
        }
        // ** Rotate Player with Right Stick **
         
        if (lookDirection.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection, Vector3.up);
            rb.rotation = Quaternion.RotateTowards(rb.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // ** Jump Logic **
        if (jumpInput && isGrounded)
        {
            //Debug.Log("Jump executed: Player is grounded!");
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false; // Prevent double jumps
        }
        if (jumpInput)
        {
            //Debug.Log("Jump input detected!");
        }

        // dash logic
        if (dashInput && Time.time > lastDashTime + dashCooldown)
        {
            //Debug.Log("Dash input detected!");
            //Debug.Log($"LookDirection: {lookDirection}");
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

    private void HandleMovement()
    {
        if (isStunned)
        {
            Debug.Log("Player is stunned and cannot move!");
            return;
        }
        
        // ** Move Player with Left Stick **
        Vector3 moveDirection = new Vector3(moveInput.x, 0, moveInput.y);
        if (moveDirection.magnitude > 0.1f)
        {
            rb.MovePosition(rb.position + moveDirection * (moveSpeed * Time.deltaTime));
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
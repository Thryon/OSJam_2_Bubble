using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using UnityEngine.Rendering;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;          // Movement speed
    public float dashSpeed = 10f;          // Movement speed
    public float dashTurnSpeed = 3.14159f;          
    public float dashDuration = 0.5f;
    public float dashCooldown = 1f;
    public float rotationSpeed = 720f;    // Rotation speed
    public float jumpForce = 5f;          // Force applied for jumping
    public LayerMask groundLayer;         // Layer mask to check if grounded
    public float dashForce = 100f;
    public ParticleSystem dash_VFX;
    public Transform root;  // Timer to track stun duration
    public float bubbledRiseHeight = 1.5f;  // Timer to track stun duration
    public Transform playerCircle;
    public Renderer playerCircleRenderer;
    private float lastDashTime;
    private bool isStunned = false;
    private PlayerState playerState;
    private float stunnedTime = 3f;  // Default stun time (3 seconds)
    private float reducedStunTime = 2f;  // Reduced stun time (2 seconds)
    private float stunTimer = 0f;  // Timer to track stun duration

    private bool isButtonPressed = false;

    private Vector2 moveInput;            // Left stick input
    private Vector2 lookInput;            // Right stick input
    private bool isGrounded;              // Tracks if the player is on the ground
    private bool jumpInput;               // Tracks if jump button is pressed
    private bool dashInput;
    private bool isDashing;
    private Vector3 dashStartDirection;
    private Vector3 dashCurrentDirection;

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
        if(isStunned)
            return;
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
        playerState = GetComponent<PlayerState>();
        // Freeze rotation on physics to avoid unwanted behavior
        if (rb != null)
        {
            rb.freezeRotation = true;
        }
    }

    public bool IsDashing()
    {
        return isDashing;
    }
    
    private void Update()
    {
        HandlePlayerCircle();
        HandleStartStun();
        HandleStun();
        
        if (!isStunned)
        {
            HandleMovement();
        }
        HandleLook();
    }

    private void HandleStartStun()
    {
        if (playerState != null && !isStunned)
        {
            if(playerState.currentHealth >= playerState.playerHP)
            {
                StunPlayer();
            }
        }
    }

    private void HandlePlayerCircle()
    {
        // Find the ground to put the player circle where it belongs
        if (Physics.Raycast(transform.position + Vector3.up * 0.01f, Vector3.down, out RaycastHit hit, 10f, groundLayer))
        {
            playerCircle.transform.position = hit.point + Vector3.up * 0.01f;
            playerCircle.transform.rotation = Quaternion.identity;
        }

        float playerCircleSize = Mathf.Max(1f, playerState.CurrentBubble.transform.localScale.x);
        playerCircle.transform.localScale = new Vector3(playerCircleSize, playerCircleSize, playerCircleSize);
    }

    private void HandleStun()
    {
        if (isStunned)
        {
            // Countdown timer for the stunned state
            stunTimer -= Time.deltaTime;

            // If the player is mashing the button, reduce stun time
            if (isButtonPressed)
            {
                stunTimer = Mathf.Max(stunTimer - Time.deltaTime, reducedStunTime);  // Reduce time, but never below 2 seconds
                isButtonPressed = false;  // Reset the button press after applying reduction
            }

            // If stun time is over, unstun the player
            if (stunTimer <= 0)
            {
                UnstunPlayer();
            }
        }
    }

    private void HandleLook()
    {
        Vector3 lookDirection = Vector3.zero;
        if (playerInput.currentControlScheme == "Keyboard&Mouse" || playerInput.currentControlScheme == "Touch")
        {
            Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
            float rayLength;

            if (groundPlane.Raycast(cameraRay, out rayLength))
            {
                Vector3 pointToLook = cameraRay.GetPoint(rayLength);
                Debug.DrawLine(cameraRay.origin, pointToLook, Color.cyan);
                pointToLook.y = 0f;
                var playerPos = transform.position;
                playerPos.y = 0f;
                lookDirection = (pointToLook - playerPos).normalized;
            }
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
    }

    private void HandleStartDash()
    {
        if (dashInput && Time.time > lastDashTime + dashCooldown)
        {
            if (dash_VFX != null && !dash_VFX.isPlaying)
            {
                dash_VFX.Play(); // Trigger the particle effect
            }
            dashInput = false;
            isDashing = true;
            dashStartDirection = moveInput == Vector2.zero ? transform.forward : new Vector3(moveInput.x, 0, moveInput.y);
            dashCurrentDirection = dashStartDirection;
            lastDashTime = Time.time;
        }
    }
    private void HandleEndDash()
    {
        if (isDashing && Time.time >= lastDashTime + dashDuration)
        {
            isDashing = false;
        }
    }
    
    private void HandleMovement()
    {
        HandleStartDash();
        HandleEndDash();
        if (IsDashing())
        {
            if (moveInput.magnitude > 0.1f)
            {
                var targetDirection = new Vector3(moveInput.x, 0f, moveInput.y);
                // Quaternion targetRotation = Quaternion.LookRotation(targetDirection, Vector3.up);
                dashCurrentDirection = Vector3.RotateTowards(dashCurrentDirection, targetDirection, dashTurnSpeed * Time.deltaTime, 0f);
            }
            rb.MovePosition(rb.position + dashCurrentDirection * (dashSpeed * Time.deltaTime));
        }
        else
        {
            // ** Move Player with Left Stick **
            Vector3 moveDirection = new Vector3(moveInput.x, 0, moveInput.y);
            if (moveDirection.magnitude > 0.1f)
            {
                rb.MovePosition(rb.position + moveDirection * (moveSpeed * Time.deltaTime));
            }
            
            // ** Jump Logic **
            if (jumpInput && isGrounded)
            {
                //Debug.Log("Jump executed: Player is grounded!");
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                isGrounded = false; // Prevent double jumps
            }
        }
    }
    private void FixedUpdate()
    {
        // Check if player is grounded using a small spherecast
        isGrounded = Physics.CheckSphere(transform.position + Vector3.up * 0.48f, 0.5f, groundLayer);
    }

    private float previousLinearDamping = 0f;
    public void StunPlayer()
    {
        isStunned = true;
        stunTimer = stunnedTime;
        rb.linearVelocity = Vector3.zero;// Set the stun timer to the full time (3s)
        previousLinearDamping = rb.linearDamping;
        rb.linearDamping = 0f;
        StopDashing();
        Gun.CancelShot();
        root.DOLocalMoveY(bubbledRiseHeight, 1f).SetEase(Ease.InOutCubic);
        Debug.Log("Player is stunned!");
    }

    private void StopDashing()
    {
        isDashing = false;
    }

    // Method to unstun the player
    private void UnstunPlayer()
    {
        isStunned = false;
        Debug.Log("Player is no longer stunned.");
        var pos = root.transform.localPosition;
        pos.y = 0f;
        root.transform.localPosition = pos;
        rb.linearDamping = previousLinearDamping;
        transform.position += Vector3.up * bubbledRiseHeight;
        // Deactivate the bubble when the player is unstunned
        if (playerState != null)
        {
            // Call the PlayerState method to unstun the player
            playerState.KillBubble();

        }
    }
    private void OnDrawGizmosSelected()
    {
        // Visualize the ground check in the editor
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position + Vector3.up * 0.48f, 0.5f);
    }

}
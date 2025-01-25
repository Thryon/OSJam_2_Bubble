using UnityEngine;

public class GunScript : MonoBehaviour
{
    public GameObject projectilePrefab; // Reference to the projectile prefab
    public Transform muzzle; // Muzzle position where the projectile spawns
    public float projectileSpeed = 20f; // Speed of the projectile
    public float fireRate = 0.5f; // Time between shots
    private float lastFireTime; // Tracks the last time the gun fired

    private bool attackInput;
    private float holdTime; // Tracks how long the fire button is held

    private PlayerControls controls; // Input actions

    public float maxScale = 5f; // Maximum scale multiplier for the projectile
    public float chargeTime = 3f; // Time required to reach max scale

    private void Awake()
    {
        // Initialize the input actions
        controls = new PlayerControls();

        controls.Player.Attack.performed += ctx =>
        {
            attackInput = true;
            holdTime = 0f; // Reset hold time when button is pressed
        };

        controls.Player.Attack.canceled += ctx =>
        {
            attackInput = false;
            Shoot(); // Fire the projectile when the button is released
        };
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

    void Update()
    {
        // If the fire button is held, track the hold time
        if (attackInput)
        {
            holdTime += Time.deltaTime;
        }
    }

    void Shoot()
    {
        // Prevent firing too quickly
        if (Time.time < lastFireTime + fireRate) return;

        // Calculate the scale of the projectile based on hold time
        float scaleMultiplier = Mathf.Lerp(1f, maxScale, holdTime / chargeTime);

        // Instantiate the projectile at the muzzle position and rotation
        GameObject projectile = Instantiate(projectilePrefab, muzzle.position, muzzle.rotation);

        // Apply the scale multiplier to the projectile
        projectile.transform.localScale *= scaleMultiplier;

        // Add force to the projectile to make it move
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = muzzle.up * projectileSpeed;
        }

        // Update the last fire time
        lastFireTime = Time.time;

        // Optionally: Destroy the projectile after a certain time
        Destroy(projectile, 0.25f + holdTime);
    }
}

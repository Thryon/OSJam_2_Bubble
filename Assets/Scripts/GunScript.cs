using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.Serialization;

public class GunScript : MonoBehaviour
{
    public GameObject projectilePrefab; // Reference to the projectile prefab
    public Transform muzzle; // Muzzle position where the projectile spawns
    public float projectileSpeed = 20f; // Speed of the projectile
    public float minBubbleLifetime = 1f; // Speed of the projectile
    public float maxBubbleLifetime = 3f; // Speed of the projectile
    public float fireRate = 0.5f; // Time between shots
    public float baseDamage = 1f;
    private float lastFireTime; // Tracks the last time the gun fired

    private bool attackInput;
    private float holdTime; // Tracks how long the fire button is held

    public float maxScale = 5f; // Maximum scale multiplier for the projectile
    [FormerlySerializedAs("chargeTime")] public float maxChargeTime = 3f; // Time required to reach max scale

    

    private void Awake()
    {
        
    }

    public void StartShooting()
    {
        attackInput = true;
        holdTime = 0f; // Reset hold time when button is pressed
    }

    public void ConfirmShoot()
    {
        attackInput = false;
        Shoot(); // Fire the projectile when the button is released
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
        float scaleMultiplier = Mathf.Lerp(1f, maxScale, holdTime / maxChargeTime);

        // Instantiate the projectile at the muzzle position and rotation
        GameObject projectile = Instantiate(projectilePrefab, muzzle.position + muzzle.up * (scaleMultiplier*0.5f) + muzzle.right * (scaleMultiplier*0.5f), muzzle.rotation);

        // Apply the scale multiplier to the projectile
        Bubble bubble = projectile.GetComponent<Bubble>();
        bubble.SetSize(scaleMultiplier);
        float lifetime = Mathf.Lerp(minBubbleLifetime, maxBubbleLifetime, holdTime / maxChargeTime);
        bubble.SetLifetime(lifetime);
        bubble.AddVelocity(muzzle.up * projectileSpeed);

        // Update the last fire time
        lastFireTime = Time.time;
    }
}

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
    private Bubble bubble;
    private float lastFireTime; // Tracks the last time the gun fired
    public float minScale = 1f; 

    private bool attackInput;
    private float holdTime; // Tracks how long the fire button is held

    public float maxScale = 5f; // Maximum scale multiplier for the projectile
    [FormerlySerializedAs("chargeTime")] public float maxChargeTime = 3f; // Time required to reach max scale

    

    private void Awake()
    {
        
    }

    public void StartShooting()
    {
        // Prevent firing too quickly
        if (Time.time < lastFireTime + fireRate) return;

        attackInput = true;
        holdTime = 0f; // Reset hold time when button is pressed

        // Instantiate the projectile at the muzzle position and rotation
        GameObject projectile = Instantiate(projectilePrefab, muzzle.position + muzzle.up * (minScale * 0.5f) + muzzle.right * (minScale * 0.5f), muzzle.rotation);
        projectile.transform.rotation = Quaternion.identity;
        // Apply the scale multiplier to the projectile
        bubble = projectile.GetComponent<Bubble>();
        bubble.Disable();
        bubble.SetSize(minScale);
    }

    public void ConfirmShoot()
    {
        attackInput = false;
        Shoot(); // Fire the projectile when the button is released
    }

    public void CancelShot()
    {
        attackInput = false;
        Destroy(bubble.gameObject);
        holdTime = 0f;
    }

    void Update()
    {
        // If the fire button is held, track the hold time
        if (attackInput)
        {
            holdTime += Time.deltaTime;
            // Calculate the scale of the projectile based on hold time
            float scaleMultiplier = Mathf.Lerp(minScale, maxScale, holdTime / maxChargeTime);
            bubble.SetSize(scaleMultiplier);
            bubble.transform.position = muzzle.position + muzzle.up * (scaleMultiplier * 0.5f) + muzzle.right * (scaleMultiplier * 0.5f);
        }
    }

    void Shoot()
    {
        

        float lifetime = Mathf.Lerp(minBubbleLifetime, maxBubbleLifetime, holdTime / maxChargeTime);
        bubble.SetLifetime(lifetime);
        bubble.Enable();
        bubble.AddVelocity(muzzle.up * projectileSpeed);

        // Update the last fire time
        lastFireTime = Time.time;
    }
}

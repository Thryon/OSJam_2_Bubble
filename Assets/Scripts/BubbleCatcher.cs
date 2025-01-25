using UnityEngine;

public class BubbleCatcher : MonoBehaviour
{
    public float baseDamage = 1f;

    public bool IsFull;

    private void OnTriggerEnter(Collider collider)
    {



        // Check if the projectile hits a player
        if (collider.gameObject.CompareTag("Bubble"))
        {
            Bubble bubble = collider.GetComponentInParent<Bubble>();
            // Get the player's health component
            PlayerState playerHP = GetComponentInParent<PlayerState>();

            if (playerHP != null)
            {
                // Calculate damage based on projectile size
                float projectileSize = bubble.Size;  // Use scale as size
                float damage = baseDamage * projectileSize;

                // Apply damage to the player
                playerHP.TakeDamage(damage);
            }

            // Destroy the projectile after hitting the player
            bubble.Pop();
        }


    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

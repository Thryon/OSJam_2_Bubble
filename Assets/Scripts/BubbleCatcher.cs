using System;
using UnityEngine;

public class BubbleCatcher : MonoBehaviour
{
    public float baseDamage = 1f;

    private PlayerState playerState;
    private void Awake()
    {
        playerState = GetComponentInParent<PlayerState>();
    }

    private void OnTriggerEnter(Collider collider)
    {
        // Check if the projectile hits a player
        if (collider.gameObject.CompareTag("Bubble"))
        {
            Bubble bubble = collider.GetComponentInParent<Bubble>();

            if (playerState != null && !playerState.Bubbled)
            {
                // Calculate damage based on projectile size
                float projectileVolume = bubble.Volume;  // Use scale as size
                // float damage = baseDamage * projectileSize;

                // Apply damage to the player
                playerState.TakeDamage(projectileVolume);
            }

            // Destroy the projectile after hitting the player
            bubble.MergeIntoAndSelfDestruct(playerState.CurrentBubble.transform);
        }
    }
}

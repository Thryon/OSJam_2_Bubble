using System;
using UnityEngine;

public class BubbleCatcher : MonoBehaviour
{
    public bool Disabled = false;

    private PlayerState playerState;
    private void Awake()
    {
        playerState = GetComponentInParent<PlayerState>();
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (Disabled || (playerState != null && playerState.Bubbled))
            return;
        
        // Check if the projectile hits a player
        if (collider.gameObject.CompareTag("Bubble"))
        {
            Bubble bubble = collider.GetComponentInParent<Bubble>();

            // Calculate damage based on projectile size
            float projectileVolume = bubble.Volume;  // Use scale as size
            // float damage = baseDamage * projectileSize;

            // Apply damage to the player
            playerState.TakeDamage(projectileVolume);

            // Destroy the projectile after hitting the player
            bubble.MergeIntoAndSelfDestruct(playerState.CurrentBubble.transform);
        }
    }
}

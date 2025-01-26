using UnityEngine;

public class PlayerState : MonoBehaviour
{
    public float playerHP = 0f;
    public float currentHealth;

    public GameObject bubblePrefab;
    public Transform bubbleParent;// Bubble prefab to spawn
    private GameObject currentBubble; // Reference to the active bubble

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHealth = 0f;  // Initialize current health to max health
    }

    // Method to take damage
    public void TakeDamage(float damage)
    {
        currentHealth += damage;  // add health by damage amount
        Debug.Log($"Player hit! Current health: {currentHealth}");

        // Check if the player is bubbled
        if (currentHealth >= playerHP)
        {
            Bubbled();
        }
        else
        {
            // Grow bubble when taking damage
            GrowBubble(damage);
        }
    }

    private void GrowBubble(float damage)
    {
        if (currentBubble == null)
        {
            // Instantiate the bubble if it doesn't exist yet
            currentBubble = Instantiate(bubblePrefab, transform.position, Quaternion.identity, bubbleParent);
            currentBubble.transform.localScale = Vector3.zero; // Start with no size
        }

        // Gradually increase the bubble's size based on the damage taken
        float growthFactor = damage * 0.1f; // Adjust the multiplier for growth rate
        currentBubble.transform.localScale += Vector3.one * growthFactor;

        // Keep the bubble around the player
        currentBubble.transform.position = transform.position;
    }

    // Method to handle player stun
    private void Bubbled()
    {
        Debug.Log("Player is stunned!");
        if (currentBubble != null)
        {
            // Optionally: Lock the bubble size when the player is bubbled
            currentBubble.transform.localScale = Vector3.one * 3f; // Example final size
        }
    }
}

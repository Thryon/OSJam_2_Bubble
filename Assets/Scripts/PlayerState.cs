using UnityEngine;

public class PlayerState : MonoBehaviour
{
    public float playerHP = 0f;
    private float currentHealth;

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
    }

    // Method to handle player stun
    private void Bubbled()
    {
        Debug.Log("Player is stunned!");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

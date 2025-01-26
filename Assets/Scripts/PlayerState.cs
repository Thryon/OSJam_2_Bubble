using DG.Tweening;
using UnityEngine;

public class PlayerState : MonoBehaviour
{
    public float playerHP = 20f;
    public float currentHealth;

    public GameObject bubblePrefab;
    public Transform bubbleParent;// Bubble prefab to spawn
    private GameObject currentBubble; // Reference to the active bubble

    private float currentBubbleSize = 0f;
    private bool bubbled = false;
    public bool Bubbled => bubbled;

    public float GetRadius() => Bubble.ComputeRadiusFromVolume(currentHealth);

    public GameObject CurrentBubble
    {
        get
        {
            InstantiateBubbleIfNeeded();
            return currentBubble;
        }
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHealth = 0f;  // Initialize current health to max health
    }
    // Method to take damage
    public void TakeDamage(float damage)
    {
        if(bubbled)
            return;
        currentHealth += damage;  // add health by damage amount
        Debug.Log($"Player hit! Current health: {currentHealth}");
        // Check if the player is bubbled
        if (currentHealth >= playerHP)
        {
            SetBubbled();
        }
        else
        {
            // Grow bubble when taking damage
            GrowBubble();
        }
    }
    private void GrowBubble()
    {
        InstantiateBubbleIfNeeded();
        // Gradually increase the bubble's size based on the damage taken
        // float growthFactor = damage * 0.25f; // Adjust the multiplier for growth rate
        // currentBubbleSize += growthFactor;
        currentBubbleSize = Bubble.ComputeRadiusFromVolume(currentHealth) * 2f;
        currentBubble.transform.DOKill();
        
        currentBubble.transform.DOScale(Vector3.one * currentBubbleSize, 0.25f).SetEase(Ease.OutBounce);
        currentBubble.transform.DOLocalMoveY(Mathf.Lerp(0f, 1f, currentHealth / playerHP), 0.25f).SetEase(Ease.OutBounce);
        // Keep the bubble around the player
        currentBubble.transform.position = transform.position;
    }

    private void InstantiateBubbleIfNeeded()
    {
        if (currentBubble == null)
        {
            // Instantiate the bubble if it doesn't exist yet
            currentBubble = Instantiate(bubblePrefab, transform.position, Quaternion.identity, bubbleParent);
            currentBubble.transform.localScale = Vector3.zero; // Start with no size
        }
    }

    // Method to handle player stun
    private void SetBubbled()
    {
        Debug.Log("Player is stunned!");
        InstantiateBubbleIfNeeded();
        if (currentBubble != null)
        {
            currentBubbleSize = Bubble.ComputeRadiusFromVolume(playerHP) * 2f;
            currentBubble.transform.DOScale(Vector3.one * currentBubbleSize, 0.25f).SetEase(Ease.OutBounce);
            currentBubble.transform.DOLocalMoveY(1f, 0.25f).SetEase(Ease.OutBounce);
        }

        bubbled = true;
    }
    public void KillBubble()
    {
        currentBubble.transform.localScale = Vector3.zero;
        currentHealth = 0f;
        currentBubbleSize = 0f;
        bubbled = false;
    }
}

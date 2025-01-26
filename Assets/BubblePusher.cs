using System;
using System.Collections.Generic;
using UnityEngine;

public class BubblePusher : MonoBehaviour
{
    [SerializeField] private float pushForce = 10f;
    private float ignoreCollidedDuration = 0.1f;
    
    private Dictionary<Rigidbody, float> collided = new();
    
    private void OnTriggerEnter(Collider other)
    {
        var rb = other.GetComponentInParent<Rigidbody>();
        var playerController = other.GetComponentInParent<PlayerController>();
        if(playerController && playerController.bubblePusher != this && !playerController.isStunned)
            return;
        
        if (rb)
        {
            if (collided.TryGetValue(rb, out float value))
            {
                if (Time.time < value + ignoreCollidedDuration)
                {
                    return;
                }
                collided[rb] = Time.time;
            }
            else
            {
                collided.Add(rb, Time.time);
            }
            
            Debug.Log("Pushing " + rb.gameObject.name);
            var direction = other.transform.position - transform.position;
            direction.y = 0f;
            rb.linearVelocity = Vector3.zero;
            rb.AddForce(direction * (pushForce * rb.mass), ForceMode.Impulse);
        }
    }
}

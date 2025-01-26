using System;
using UnityEngine;

public class KillZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        var playerController = other.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.Kill();
        }
    }
}

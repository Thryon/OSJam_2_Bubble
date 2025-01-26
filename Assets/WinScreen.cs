using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class WinScreen : MonoBehaviour
{
    [SerializeField] private TMP_Text winText;
    private void OnEnable()
    {
        int playerIndex = 0;
        foreach (var playerController in GameManager.Instance.PlayerControllers)
        {
            if(playerController != null && !playerController.IsDead)
                playerIndex = playerController.playerInput.playerIndex; 
        }
        Color playerColor = GameManager.Instance.GetPlayerColor(playerIndex);
        
        winText.SetText($"<color=#{playerColor.ToHexString()}>Player {playerIndex + 1}</color> wins !");
    }
}

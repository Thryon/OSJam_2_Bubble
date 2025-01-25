using System;
using System.Collections.Generic;
using UnityEngine;

public class WaitForPlayersScreen : MonoBehaviour
{
    [SerializeField]
    private List<PlayerJoinBoxUI> playerJoinBoxes = new List<PlayerJoinBoxUI>();

    private void OnEnable()
    {
        int playerCount = GameManager.Instance.PlayerInputs.Count;
        for (var index = 0; index < playerJoinBoxes.Count; index++)
        {
            var box = playerJoinBoxes[index];
            box.SetJoined(index < playerCount);
        }
        
        GlobalEvents.OnPlayerJoined.AddListener(OnPlayerJoined);
        GlobalEvents.OnPlayerLeft.AddListener(OnPlayerLeft);
    }

    private void OnDisable()
    {
        GlobalEvents.OnPlayerJoined.RemoveListener(OnPlayerJoined);
        GlobalEvents.OnPlayerLeft.RemoveListener(OnPlayerLeft);
    }

    private void OnPlayerLeft(int playerIndex)
    {
        PlayerJoined(playerIndex);
    }

    private void OnPlayerJoined(int playerIndex)
    {
        PlayerLeft(playerIndex);
    }

    public void PlayerJoined(int playerIndex)
    {
        playerJoinBoxes[playerIndex].SetJoined(true);
    }
    public void PlayerLeft(int playerIndex)
    {
        playerJoinBoxes[playerIndex].SetJoined(false);
    }
}

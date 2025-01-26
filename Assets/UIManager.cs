using System;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] WaitForPlayersScreen waitForPlayersScreen;
    [SerializeField] WinScreen winScreen;
    private void Awake()
    {
        waitForPlayersScreen.gameObject.SetActive(GameManager.Instance.CurrentState == GameManager.State.WaitForPlayers);
        winScreen.gameObject.SetActive(false);
        GlobalEvents.OnWaitForPlayersPhaseStarted.AddListener(OnWaitForPlayersPhaseStarted);
        GlobalEvents.OnWaitForPlayersPhaseEnded.AddListener(OnWaitForPlayersPhaseEnded);
        GlobalEvents.OnGameEnded.AddListener(OnGameEnded);
    }

    private void OnDestroy()
    {
        GlobalEvents.OnWaitForPlayersPhaseStarted.RemoveListener(OnWaitForPlayersPhaseStarted);
        GlobalEvents.OnWaitForPlayersPhaseEnded.RemoveListener(OnWaitForPlayersPhaseEnded);
        GlobalEvents.OnGameEnded.RemoveListener(OnGameEnded);
    }

    private void OnGameEnded()
    {
        winScreen.gameObject.SetActive(true);
    }

    private void OnWaitForPlayersPhaseStarted()
    {
        waitForPlayersScreen.gameObject.SetActive(true);
    }

    private void OnWaitForPlayersPhaseEnded()
    {
        waitForPlayersScreen.gameObject.SetActive(false);
    }
}

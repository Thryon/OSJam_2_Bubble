using System;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] WaitForPlayersScreen waitForPlayersScreen;
    private void Awake()
    {
        waitForPlayersScreen.gameObject.SetActive(GameManager.Instance.CurrentState == GameManager.State.WaitForPlayers);
        GlobalEvents.OnWaitForPlayersPhaseStarted.AddListener(OnWaitForPlayersPhaseStarted);
        GlobalEvents.OnWaitForPlayersPhaseEnded.AddListener(OnWaitForPlayersPhaseEnded);
    }

    private void OnDestroy()
    {
        GlobalEvents.OnWaitForPlayersPhaseStarted.RemoveListener(OnWaitForPlayersPhaseStarted);
        GlobalEvents.OnWaitForPlayersPhaseEnded.RemoveListener(OnWaitForPlayersPhaseEnded);
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

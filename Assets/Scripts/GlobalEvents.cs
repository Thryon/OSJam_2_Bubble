using UnityEngine;
using UnityEngine.Events;

public static class GlobalEvents
{
    public static UnityEvent<int> OnPlayerJoined = new();
    public static UnityEvent<int> OnPlayerLeft = new();
    public static UnityEvent OnWaitForPlayersPhaseStarted = new();
    public static UnityEvent OnWaitForPlayersPhaseEnded = new();
}

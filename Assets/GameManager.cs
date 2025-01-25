using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    [SerializeField] PlayerInputManager playerInputManager;
    [SerializeField] int minAmountOfPlayers = 2;
    
    List<PlayerInput> playerInputs = new List<PlayerInput>();
    
    Dictionary<int, PlayerInput> playerInputsDic = new ();
    public List<PlayerInput> PlayerInputs { get => playerInputs; }
    
    private static GameManager instance;
    public static GameManager Instance {
        get
        {
            return instance;
        }
    }
    public enum State
    {
        Starting,
        WaitForPlayers,
        Playing,
        End,
    }

    public State CurrentState;

    private void Awake()
    {
        if (GameManager.Instance == null)
        {
            instance = this;
        }
        else if(Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        GoToState(State.WaitForPlayers);
    }

    private void Update()
    {
        if (CurrentState == State.WaitForPlayers)
        {
            if (playerInputs.Count >= minAmountOfPlayers)
            {
                if (playerInputs[0].actions["UI/Submit"].IsPressed())
                {
                    GoToState(State.Playing);
                }
            }
        }
    }

    public void GoToState(State newState)
    {
        if (CurrentState == State.WaitForPlayers)
        {
            foreach (var playerInput in playerInputs)
            {
                playerInput.SwitchCurrentActionMap("UI");
            }
            playerInputManager.DisableJoining();
            GlobalEvents.OnWaitForPlayersPhaseEnded.Invoke();
        }

        if (newState == State.WaitForPlayers)
        {
            playerInputManager.EnableJoining();
            GlobalEvents.OnWaitForPlayersPhaseStarted.Invoke();
        }

        if (newState == State.Playing)
        {
            foreach (var playerInput in playerInputs)
            {
                playerInput.SwitchCurrentActionMap("Player");
            }
        }
        
        CurrentState = newState;
    }

    public void OnPlayerJoined(PlayerInput playerInput)
    {
        if (CurrentState == State.WaitForPlayers)
        {
            if (!playerInputs.Contains(playerInput))
            {
                playerInputs.Add(playerInput);
            }
            playerInput.SwitchCurrentActionMap("UI");
        }
        GlobalEvents.OnPlayerJoined?.Invoke(playerInput.playerIndex);
    }

    public void OnPlayerLeft(PlayerInput playerInput)
    {
        if (CurrentState == State.WaitForPlayers)
        {
            playerInputs.Remove(playerInput);
        }
        GlobalEvents.OnPlayerLeft?.Invoke(playerInput.playerIndex);
    }
}

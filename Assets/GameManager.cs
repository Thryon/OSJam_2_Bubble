using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] PlayerInputManager playerInputManager;
    [SerializeField] int minAmountOfPlayers = 2;
    [SerializeField] State startState = State.Playing;
    [SerializeField] List<Transform> spawnPoints = new();
    
    List<PlayerInput> playerInputs = new List<PlayerInput>();
    List<PlayerController> playerControllers = new ();
    
    public List<PlayerInput> PlayerInputs { get => playerInputs; }
    public List<PlayerController> PlayerControllers { get => playerControllers; }
    
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
        GoToState(startState);
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

        if (CurrentState == State.Playing)
        {
            int alivePlayers = 0;
            foreach (var playerController in playerControllers)
            {
                if(playerController == null)
                    return;

                if (!playerController.IsDead)
                {
                    alivePlayers++;
                }
            }

            if (alivePlayers <= 1)
            {
                GoToState(State.End);
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

        if (newState == State.End)
        {
            GlobalEvents.OnGameEnded?.Invoke();
            StartCoroutine(ReloadInSeconds(4f));
        }
        
        CurrentState = newState;
    }

    private IEnumerator ReloadInSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OnPlayerJoined(PlayerInput playerInput)
    {
        if (CurrentState == State.WaitForPlayers)
        {
            if (!playerInputs.Contains(playerInput))
            {
                playerInputs.Add(playerInput);
                int spawnPointIndex = playerInput.playerIndex % spawnPoints.Count;
                playerInput.transform.position = spawnPoints[spawnPointIndex].position;
                var playerController = playerInput.GetComponent<PlayerController>();
                Color color = GetPlayerColor(playerInput.playerIndex);
                playerController.playerCircleRenderer.material.color = color;
            }
            playerInput.SwitchCurrentActionMap("UI");
            GlobalEvents.OnPlayerJoined?.Invoke(playerInput.playerIndex);
        }
    }

    public Color GetPlayerColor(int playerInputPlayerIndex)
    {
        switch (playerInputPlayerIndex)
        {
            case 0:
                return new Color(1f, .4f, .4f);
            case 1:
                return new Color(.4f, .4f, 1f);
            case 2: 
                return new Color(.4f, 1f, .4f);
            case 3: 
                return new Color(1f, 1f, .4f);
            default:
                return Color.white;
        }
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

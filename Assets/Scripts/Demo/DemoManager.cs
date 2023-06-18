using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Tilemaps;


public enum State {
    Init,
    WaitingToStart,
    CountDownToStart,
    GamePlaying,
    GameOver,
    _NONE
}

public class DemoManager : NetworkBehaviour {

    public static DemoManager Instance { get; private set; }

    public event System.EventHandler OnStateChanged;
    public event System.EventHandler OnGamePaused;
    public event System.EventHandler OnGameUnPaused;

    private Camera _cam;
    private PlayerMovement _player;
    private GrapplingScript _grappling;
    public GamePauseUI _gamePauseUI;
    [SerializeField] private TextMeshProUGUI nameText;
    private bool isPausedGame = false;

    // We need to network the state variable.
    public NetworkVariable<State> state = new NetworkVariable<State>();
    private State previousState = State._NONE;
    private float countdownToStartTimer = 3f;
    // This is the total length of the game after the game started.
    public float matchDuration = 5f;
    private NetworkVariable<float> gamePlayingTimer = new NetworkVariable<float>();

    public SceneData SceneData;

    private void Awake() {
        Instance = this;
        _cam = FindObjectOfType<Camera>();
        //_gamePauseUI = GameObject.FindWithTag("GamePauseMenu").GetComponent<GamePauseUI>();
        // Initialize the networked game time.
        gamePlayingTimer.Value = matchDuration;
    }

    private void Start() {
        SetNextStateServerRpc(State.Init);
        SetSceneData(SceneData);
        //OnGamePaused += _gamePauseUI.DemoManager_OnGamePaused;
        //OnGameUnPaused += _gamePauseUI.DemoManager_OnGameUnPaused;
    }

    public void SetSceneData(SceneData data) {
        SceneData = data;

        //Update the camera and tilemap color according to the new data.
        _cam.orthographicSize = data.camSize;
    }


    private void Update() {
        GameObject[] players;
        switch (state.Value) {
            case State.Init:
                previousState = State.Init;
                // We do this only once.
                // Now call the GameStartCountdownUI and tell that we want to change.
                if (NetworkManager.Singleton.IsServer) {
                    SetNextStateServerRpc(State.WaitingToStart);
                }
                break;
            case State.WaitingToStart:
                if (previousState == State.Init) {
                    // Send the event once.
                    OnStateChanged?.Invoke(this, System.EventArgs.Empty);
                    previousState = State.WaitingToStart;
                }
                // We wait for a second player to join.
                // Find all players and their respective movement and grappling and deactivate them 
                // until the game starts.
                // This does every client for himself (only deactivate the movement for your own player),
                // since the movement is networked.
                players = GameObject.FindGameObjectsWithTag("Player");
                foreach (GameObject player in players) {
                    // Only deactivate it for your own player.
                    //NetworkObject playerNetworkObject = player.GetComponent<NetworkObject>();
                    //if (NetworkManager.Singleton.LocalClientId == playerNetworkObject.OwnerClientId) {
                    PlayerMovement playersMovement = player.GetComponent<PlayerMovement>();
                    GrapplingScript grapplingScript = player.GetComponent<GrapplingScript>();
                    playersMovement.enabled = false;
                    grapplingScript.enabled = false;
                    //}
                }
                // Check if we can start. We need two players.
                if (NetworkManager.Singleton.IsServer) {
                    Debug.Log("Connected players: " + NetworkManager.Singleton.ConnectedClientsIds.Count.ToString());
                    if (NetworkManager.Singleton.ConnectedClientsIds.Count > 1) {
                        SetNextStateServerRpc(State.CountDownToStart);
                    }
                }
                break;
            case State.CountDownToStart:
                if (previousState == State.WaitingToStart) {
                    // Send the event once.
                    OnStateChanged?.Invoke(this, System.EventArgs.Empty);
                    previousState = State.CountDownToStart;
                }
                // We still wait until the countdown is down.
                previousState = State.CountDownToStart;
                players = GameObject.FindGameObjectsWithTag("Player");
                foreach (GameObject player in players) {
                    // Only deactivate it for your own player.
                    //NetworkObject playerNetworkObject = player.GetComponent<NetworkObject>();
                    //if (NetworkManager.Singleton.LocalClientId == playerNetworkObject.OwnerClientId) {
                    PlayerMovement playersMovement = player.GetComponent<PlayerMovement>();
                    GrapplingScript grapplingScript = player.GetComponent<GrapplingScript>();
                    playersMovement.enabled = false;
                    grapplingScript.enabled = false;
                    //}
                }
                if (NetworkManager.Singleton.IsServer) {
                    countdownToStartTimer -= Time.deltaTime;
                    if (countdownToStartTimer < 0f) {
                        // Change the state and lets go.
                        SetNextStateServerRpc(State.GamePlaying);
                    }
                }
                break;
            case State.GamePlaying:
                if (previousState == State.CountDownToStart) {
                    // Send the event once.
                    OnStateChanged?.Invoke(this, System.EventArgs.Empty);
                    previousState = State.GamePlaying;
                }
                // To only do it once.
                if (previousState == State.CountDownToStart) {
                    players = GameObject.FindGameObjectsWithTag("Player");
                    foreach (GameObject player in players) {
                        // Only deactivate it for your own player.
                        //NetworkObject playerNetworkObject = player.GetComponent<NetworkObject>();
                        //if (NetworkManager.Singleton.LocalClientId == playerNetworkObject.OwnerClientId) {
                        PlayerMovement playersMovement = player.GetComponent<PlayerMovement>();
                        GrapplingScript grapplingScript = player.GetComponent<GrapplingScript>();
                        playersMovement.enabled = true;
                        grapplingScript.enabled = true;
                        //}
                    }
                    previousState = State.GamePlaying;
                }

                //if (Input.GetKeyDown(KeyCode.P)) {
                //    ToggleGamePaused();
                //}
                
                if (NetworkManager.Singleton.IsServer) {
                    gamePlayingTimer.Value -= Time.deltaTime;
                    if (gamePlayingTimer.Value < 0f) {
                        // The game is over, deactivate movement.
                        players = GameObject.FindGameObjectsWithTag("Player");
                        foreach (GameObject player in players) {
                            PlayerMovement playersMovement = player.GetComponent<PlayerMovement>();
                            GrapplingScript grapplingScript = player.GetComponent<GrapplingScript>();
                            playersMovement.enabled = false;
                            grapplingScript.enabled = false;
                        }
                        // Game over.
                        SetNextStateServerRpc(State.GameOver);
                    }
                }
                break;
            case State.GameOver:
                if (previousState == State.GamePlaying) {
                    // Send the event once.
                    OnStateChanged?.Invoke(this, System.EventArgs.Empty);
                    previousState = State.GameOver;
                }
                break;
        }
    }

    public bool isCountDownToStartActive() {
        return state.Value == State.CountDownToStart;
    }
    public bool isGameOver() {
        return state.Value == State.GameOver;
    }

    public float getCountdownToStartTimer() {
        return countdownToStartTimer;
    }

    public float GetGamePlayingTimer() {
        return gamePlayingTimer.Value;
    }

    [ServerRpc]
    public void SetNextStateServerRpc (State nextState) {
        state.Value = nextState;
    }

    [ServerRpc]
    public void testServerRpc (string message) {
        Debug.Log(message);
    }

    private void ToggleGamePaused() {
        isPausedGame = !isPausedGame;
        if (isPausedGame) {
            OnGamePaused?.Invoke(this, System.EventArgs.Empty);
        }
        else {
            OnGameUnPaused?.Invoke(this, System.EventArgs.Empty);
        }
    }
}


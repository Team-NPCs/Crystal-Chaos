using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DemoManager : MonoBehaviour {

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

    public enum State {
        Init,
        WaitingToStart,
        CountDownToStart,
        GamePlaying,
        GameOver
    }

    public State state;
    private float countdownToStartTimer = 3f;
    // This is the total length of the game after the game started.
    private float gamePlayingTimer = 120f;

    public SceneData SceneData;

    private void Awake() {
        Instance = this;
        state = State.Init;
        _cam = FindObjectOfType<Camera>();
        _player = GameObject.FindWithTag("Player").GetComponent<PlayerMovement>();
        _grappling = GameObject.FindWithTag("Player").GetComponent<GrapplingScript>();
        _gamePauseUI = GameObject.FindWithTag("GamePauseMenu").GetComponent<GamePauseUI>();
    }

    private void Start() {
        SetSceneData(SceneData);
        OnGamePaused += _gamePauseUI.DemoManager_OnGamePaused;
        OnGameUnPaused += _gamePauseUI.DemoManager_OnGameUnPaused;
    }

    public void SetSceneData(SceneData data) {
        SceneData = data;

        //Update the camera and tilemap color according to the new data.
        _cam.orthographicSize = data.camSize;
    }


    private void Update() {
        DemoManager.Instance.isGamePlaying();
        switch (state) {
            case State.Init:
                // We do this only once.
                // Now call the GameStartCountdownUI and tell that we want to change.
                state = State.WaitingToStart;
                // Send the event once.
                OnStateChanged?.Invoke(this, System.EventArgs.Empty);
                break;
            case State.WaitingToStart:
                // We wait for a second player to join.
                if (NetworkManager.Singleton.ConnectedClientsIds.Count > 1) {
                    state = State.CountDownToStart;
                    OnStateChanged?.Invoke(this, System.EventArgs.Empty);
                }
                break;
            case State.CountDownToStart:
                countdownToStartTimer -= Time.deltaTime;
                if (countdownToStartTimer < 0f) {
                    state = State.GamePlaying;
                    OnStateChanged?.Invoke(this, System.EventArgs.Empty);
                }
                break;
            case State.GamePlaying:
                gamePlayingTimer -= Time.deltaTime;
                if (Input.GetKeyDown(KeyCode.P)) {
                    ToggleGamePaused();
                }
                if (gamePlayingTimer < 0f) {
                    state = State.GameOver;
                    OnStateChanged?.Invoke(this, System.EventArgs.Empty);
                }
                break;
            case State.GameOver:
                break;
        }
    }

    public void isGamePlaying() {
        if (state == State.GamePlaying) {
            _player.enabled = _grappling.enabled = true;
        }
        else {
            _player.enabled = _grappling.enabled = false;
        }
    }

    public bool isCountDownToStartActive() {
        return state == State.CountDownToStart;
    }
    public bool isGameOver() {
        return state == State.GameOver;
    }

    public float getCountdownToStartTimer() {
        return countdownToStartTimer;
    }

    public float GetGamePlayingTimer() {
        return gamePlayingTimer;
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


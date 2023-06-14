using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DemoManager : MonoBehaviour {

    public static DemoManager Instance { get; private set; }

    public event System.EventHandler OnStateChanged;
    public event System.EventHandler OnGamePaused;
    public event System.EventHandler OnGameUnPaused;

    private Camera _cam;
    private PlayerMovement _player;
    private Shooting _shooting;
    private GrapplingScript _grappling;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private TextMeshProUGUI nameText;
    private bool isPausedGame = false;

    private enum State {
        WaitingToStart,
        CountDownToStart,
        GamePlaying,
        GameOver
    }

    private State state;
    private float waitingToStartTimer = 1f;
    private float countdownToStartTimer = 3f;
    private float gamePlayingTimer = 120f;

    public SceneData SceneData;

    private void Awake() {
        Instance = this;
        state = State.WaitingToStart;
        _cam = FindObjectOfType<Camera>();
        _player = GameObject.FindWithTag("Player").GetComponent<PlayerMovement>();
        _shooting = GameObject.FindWithTag("ShootingPoint").GetComponent<Shooting>();
        _grappling = GameObject.FindWithTag("GrapplingGun").GetComponent<GrapplingScript>();
    }

    private void Start() {
        SetSceneData(SceneData);
    }

    public void SetSceneData(SceneData data) {
        SceneData = data;

        //Update the camera and tilemap color according to the new data.
        _cam.orthographicSize = data.camSize;
    }


    private void Update() {
        DemoManager.Instance.isGamePlaying();
        switch (state) {
            case State.WaitingToStart:
                waitingToStartTimer -= Time.deltaTime;
                if (waitingToStartTimer < 0f) {
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
            _player.enabled = _grappling.enabled = _shooting.enabled = true;
        }
        else {
            _player.enabled = _grappling.enabled = _shooting.enabled = false;
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


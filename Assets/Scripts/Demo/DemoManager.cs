using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DemoManager : MonoBehaviour {

    public static DemoManager Instance { get; private set; }

    public event System.EventHandler OnStateChanged;

    private Camera _cam;
    private PlayerMovement _player;
    private Shooting _shooting;
    private GrapplingScript _grappling;
    [SerializeField] private PlayerData[] playerTypes;
    [SerializeField] private Tilemap[] levels;
    [SerializeField] private Transform spawnPoint;

    [SerializeField] private TextMeshProUGUI nameText;

    private int _currentPlayerTypeIndex;
    private int _currentTilemapIndex;

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
        SwitchLevel(0);
        SwitchPlayerType(0);
    }

    public void SetSceneData(SceneData data) {
        SceneData = data;

        //Update the camera and tilemap color according to the new data.
        _cam.orthographicSize = data.camSize;
    }

    public void SwitchPlayerType(int index) {
        _player.Data = playerTypes[index];
        _currentPlayerTypeIndex = index;
    }

    public void SwitchLevel(int index) {
        //Switch tilemap active and apply color.
        levels[_currentTilemapIndex].gameObject.SetActive(false);
        levels[index].gameObject.SetActive(true);
        levels[_currentTilemapIndex] = levels[index];

        _player.transform.position = spawnPoint.position;

        _currentTilemapIndex = index;
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

    public float getCountdownToStartTimer() {
        return countdownToStartTimer;
    }

    public float GetGamePlayingTimer() {
        return gamePlayingTimer;
    }
}


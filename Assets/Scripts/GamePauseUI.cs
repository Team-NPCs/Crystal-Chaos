using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GamePauseUI : MonoBehaviour
{
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private TextMeshProUGUI numberOfKill;
    [SerializeField] private TextMeshProUGUI numberOfDeath;

    private void Awake() {
        DemoManager.Instance.OnGamePaused += DemoManager_OnGamePaused;
        DemoManager.Instance.OnGameUnPaused += DemoManager_OnGameUnPaused;
        mainMenuButton.onClick.AddListener(() => {
            NetworkManager.Singleton.Shutdown();
            NetworkManager networkManager = GameObject.FindObjectOfType<NetworkManager>();
            Destroy(networkManager.gameObject);
            Loader.Load(Loader.Scene.MainMenu);
        });
    }
    private void Start() {
        Hide();
    }

    public void DemoManager_OnGameUnPaused(object sender, EventArgs e) {
        Hide();
    }

    public void DemoManager_OnGamePaused(object sender, EventArgs e) {
        Show();
    }

    public void Show() {
        gameObject.SetActive(true);
    }

    public void Hide() {
        gameObject.SetActive(false);
    }

    // We do not count the number of kills each player does but only the number of deaths of each player.
    // Since we implement a 1v1 this is equal to the inversed kills.
    // So the number of my deaths are the number of kills of the enemy and vice versa.
    public void AdjustNumberOfKill(int numberOfDeathEnemy) {
        numberOfKill.text = numberOfDeathEnemy.ToString();
    }

    public void AdjustNumberOfDeath(int numberOfDeathMyself) {
        numberOfDeath.text = numberOfDeathMyself.ToString();
    }
}

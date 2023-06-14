using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GamePauseUI : MonoBehaviour
{
    [SerializeField] private Button mainMenuButton;

    private void Awake() {
        mainMenuButton.onClick.AddListener(() => {
            Loader.Load(Loader.Scene.MainMenu);
        });
    }
    private void Start() {
        DemoManager.Instance.OnGamePaused += DemoManager_OnGamePaused;
        DemoManager.Instance.OnGameUnPaused += DemoManager_OnGameUnPaused;
        Hide();
    }

    private void DemoManager_OnGameUnPaused(object sender, EventArgs e) {
        Hide();
    }

    private void DemoManager_OnGamePaused(object sender, EventArgs e) {
        Show();
    }

    private void Show() {
        gameObject.SetActive(true);
    }

    private void Hide() {
        gameObject.SetActive(false);
    }
}

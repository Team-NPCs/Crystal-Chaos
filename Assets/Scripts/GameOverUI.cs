using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private Button goToMenuButton;
    private void Awake() {
        goToMenuButton.onClick.AddListener(() => {
            Loader.Load(Loader.Scene.MainMenu);
        });
    }

    private void Start() {
        DemoManager.Instance.OnStateChanged += DemoManager_OnStateChanged;

        Hide();
    }

    private void DemoManager_OnStateChanged(object sender, EventArgs e) {
        if (DemoManager.Instance.isGameOver()) {
            Show();
        }
        else {
            Hide();
        }
    }
    private void Show() {
        gameObject.SetActive(true);
    }
    private void Hide() {
        gameObject.SetActive(false);
    }
}

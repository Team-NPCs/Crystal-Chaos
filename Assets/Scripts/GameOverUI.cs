using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private Button goToMenuButton;
    [SerializeField] private TextMeshProUGUI numberOfKill;
    [SerializeField] private TextMeshProUGUI numberOfDeath;
    [SerializeField] private TextMeshProUGUI numberOfKillReference;
    [SerializeField] private TextMeshProUGUI numberOfDeathReference;

    private void Awake() {
        goToMenuButton.onClick.AddListener(() => {
            NetworkManager.Singleton.Shutdown();
            NetworkManager networkManager = GameObject.FindObjectOfType<NetworkManager>();
            Destroy(networkManager.gameObject);
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
        numberOfKill.text = numberOfKillReference.text;
        numberOfDeath.text = numberOfDeathReference.text;
        gameObject.SetActive(true);
    }
    private void Hide() {
        gameObject.SetActive(false);
    }
}

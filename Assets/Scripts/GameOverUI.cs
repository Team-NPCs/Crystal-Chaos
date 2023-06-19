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
    [SerializeField] private TextMeshProUGUI result;

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
        // Determine the winner.
        int numberOfKillValue = Int32.Parse(numberOfKillReference.text);
        int numberOfDeathValue = Int32.Parse(numberOfDeathReference.text);
        if (numberOfKillValue > numberOfDeathValue) {
            result.text = "you won!";
        }
        else if (numberOfKillValue < numberOfDeathValue) {
            result.text = "you lost ...";
        }
        else {
            result.text = "tie!";
        }
        gameObject.SetActive(true);
    }
    private void Hide() {
        gameObject.SetActive(false);
    }
}

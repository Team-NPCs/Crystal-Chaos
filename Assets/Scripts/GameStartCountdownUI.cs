using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameStartCountdownUI : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI countDownText;
    [SerializeField] private TextMeshProUGUI joinInformationText;

    private void Start() {
        // Add a callback for the state of the game so we know when we should do something.
        DemoManager.Instance.OnStateChanged += DemoManager_OnStateChanged;
        // Hide for now.
        Hide();
    }

    private void DemoManager_OnStateChanged(object sender, EventArgs e) {
        // If the countdown is currently active we show it, afterwards hide it.
        if ((DemoManager.Instance.state == DemoManager.State.WaitingToStart) || (DemoManager.Instance.state == DemoManager.State.CountDownToStart)) {
            Show();
        }
        else {
            Hide();
        }
    }
    private void Update() {
        // Set the time (seconds) value.
        if (DemoManager.Instance.state == DemoManager.State.WaitingToStart) {
            joinInformationText.text = "waiting for the other player to join ...";
            countDownText.text = "";
        }
        else if (DemoManager.Instance.state == DemoManager.State.CountDownToStart) {
            joinInformationText.text = "";
            countDownText.text = Math.Ceiling(DemoManager.Instance.getCountdownToStartTimer()).ToString();
        }
    }
    private void Show() {
        gameObject.SetActive(true);
    }
    private void Hide() {
        gameObject.SetActive(false);
    }
}

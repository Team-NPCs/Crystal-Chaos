using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameStartCountdownUI : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI countDownText;
    [SerializeField] private TextMeshProUGUI joinInformationText;

    private State demoManagerState;

    private void Awake() {
        // Add a callback for the state of the game so we know when we should do something.
        DemoManager.Instance.OnStateChanged += DemoManager_OnStateChanged;
        demoManagerState = State._NONE;
        // Hide for now.
        Hide();
    }

    private void DemoManager_OnStateChanged(object sender, EventArgs e) {
        demoManagerState = DemoManager.Instance.state.Value;
        // If the countdown is currently active we show it, afterwards hide it.
        if ((demoManagerState == State.WaitingToStart) || (demoManagerState == State.CountDownToStart)) {
            Show();
        }
        else {
            Hide();
        }
    }
    private void Update() {
        // Set the time (seconds) value.
        if (demoManagerState == State.WaitingToStart) {
            joinInformationText.text = "waiting for the other player to join ...";
            countDownText.text = "";
        }
        else {
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

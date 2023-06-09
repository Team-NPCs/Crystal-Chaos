using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameplayClockUI : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI gameplayTimer;

    private TimeSpan timeSpan;

    private void Update() {
        timeSpan = TimeSpan.FromSeconds((double)(DemoManager.Instance.GetGamePlayingTimer()));
        gameplayTimer.text = string.Format("{0:00}:{1:00}", timeSpan.Minutes, timeSpan.Seconds);

        if (Input.GetKeyDown(KeyCode.T)) {
            DemoManager.Instance.testServerRpc("tested from the client");
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameStartCountdownUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI countDownText;

    private void Start()
    {
        DemoManager.Instance.OnStateChanged += DemoManager_OnStateChanged;

        Hide();
    }

    private void DemoManager_OnStateChanged(object sender, EventArgs e)
    {
        if(DemoManager.Instance.isCountDownToStartActive())
        {
            Show();
        } else
        {
            Hide();
        }
    }
    private void Update()
    {
        countDownText.text = Math.Ceiling(DemoManager.Instance.getCountdownToStartTimer()).ToString();
    }
    private void Show()
    {
        gameObject.SetActive(true);
    }
    private void Hide()
    {
        gameObject.SetActive(false);
    }
}

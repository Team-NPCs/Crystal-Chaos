using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    [SerializeField] private Button volumeButton;
    [SerializeField] private Button goToMenu;
    [SerializeField] private TextMeshProUGUI volumeValue;

    private void Awake() {
        volumeButton.onClick.AddListener(() => {
            MusicManager.Instance.ChangeVolume();
            UpdateVisual();
        });
        goToMenu.onClick.AddListener(() => {
            Hide();
        });
    }
    private void Start() {
        UpdateVisual();
    }

    public void Show() {
        gameObject.SetActive(true);
    }
    public void Hide() {
        gameObject.SetActive(false);
    }
    private void UpdateVisual() {
        volumeValue.text = "volume: " + Mathf.Round(MusicManager.Instance.GetVolume() * 10f);
    }

}

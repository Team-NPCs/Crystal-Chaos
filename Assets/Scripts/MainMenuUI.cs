using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour {
    [SerializeField] private Button playButton;
    [SerializeField] private Button howToPlayButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;

    [SerializeField] private HowToPlayUI howToPlay;
    [SerializeField] private SettingsUI settings;


    private void Awake() {
        playButton.onClick.AddListener(() => {
            Loader.Load(Loader.Scene.MainStage);
        });

        howToPlayButton.onClick.AddListener(() => {
            howToPlay.Show();
        });

        settingsButton.onClick.AddListener(() => {
            settings.Show();
        });

        quitButton.onClick.AddListener(() => {
            Application.Quit();
        });

        Time.timeScale = 1f;
    }

}

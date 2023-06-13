using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManagerUI : MonoBehaviour {

    //private bool isDemoManagerActive = false;
    [SerializeField] private Button hostBtn;
    [SerializeField] private GameObject _demoManager;
    [SerializeField] private GameObject _hud;
    [SerializeField] private Button clientBtn;

    private void Awake() {
        hostBtn.onClick.AddListener(() => {
            NetworkManager.Singleton.StartHost();
            _demoManager.SetActive(true);
            _hud.SetActive(true);
            gameObject.SetActive(false);
        });

        clientBtn.onClick.AddListener(() => {
            NetworkManager.Singleton.StartClient();
        });
    }

    //private void Update() {
    //    if (!isDemoManagerActive && true) {
    //        _demoManager.SetActive(true);
    //        _hud.SetActive(true);
    //        gameObject.SetActive(false);
    //        isDemoManagerActive = true;
    //    }
    //}
}

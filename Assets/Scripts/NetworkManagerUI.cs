using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManagerUI : MonoBehaviour {
    private bool isAlreadyActive;
    [SerializeField] private Button hostBtn;
    [SerializeField] private GameObject _demoManager;
    [SerializeField] private GameObject _hud;
    [SerializeField] private Button clientBtn;

    private void Awake() {
        hostBtn.onClick.AddListener(() => {
            // Loader.Load(Loader.Scene.MainStage);
            Debug.Log("Starting Host");
            NetworkManager.Singleton.StartHost();

            _demoManager.SetActive(true);
            _hud.SetActive(true);
            gameObject.SetActive(false);
        });
        clientBtn.onClick.AddListener(() => {
            // Loader.Load(Loader.Scene.MainStage);
            Debug.Log("Starting Client");
            NetworkManager.Singleton.StartClient();
        });
    }
    // private void Update() {
    //     if (true  /*when client connects && isAlreadyActive*/) {
    //         _demoManager.SetActive(true);
    //         _hud.SetActive(true);
    //         gameObject.SetActive(false);
    //         isAlreadyActive = true;
    //     }
    // }
}

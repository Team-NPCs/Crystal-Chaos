using System;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManagerUI : NetworkBehaviour {

    //private bool isDemoManagerActive = false;
    [SerializeField] private Button hostBtn;
    [SerializeField] private GameObject _demoManager;
    [SerializeField] private GameObject _hud;
    [SerializeField] private Button clientBtn;

    [SerializeField] TextMeshProUGUI joinCodeText;
    [SerializeField] TMP_InputField joinCodeInputField;

    [SerializeField] UnityTransport transport;

    private void Awake() {
        hostBtn.onClick.AddListener(() => {
            createRelay();
        });

        clientBtn.onClick.AddListener(() => {
            JoinRelay(joinCodeInputField.text);
        });
    }

    private async void Start() {

        await UnityServices.InitializeAsync();


        AuthenticationService.Instance.SignedIn += () => {
            Debug.Log("Signed in");
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    private async void createRelay() {

        try {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(1);

            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            Debug.Log($"Join code: {joinCode}");

            RelayServerData relayServerData = new RelayServerData(allocation, "dtls");

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartHost();

            joinCodeText.text = joinCode;

            _hud.SetActive(true);
            _demoManager.SetActive(true);
            gameObject.SetActive(false);
        }
        catch (RelayServiceException e) {
            Debug.Log(e);
        }
    }


    private async void JoinRelay(string joinCode) {
        try {

            Debug.Log($"Joining relay with code: {joinCode}");

            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartClient();

            _demoManager.SetActive(true);
            _hud.SetActive(true);
            gameObject.SetActive(false);
        }
        catch (RelayServiceException e) {
            Debug.Log(e);
        }
    }

}

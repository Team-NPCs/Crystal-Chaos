using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManagerUI : MonoBehaviour {

    //private bool isDemoManagerActive = false;
    [SerializeField] private Button hostBtn;
    [SerializeField] private GameObject _demoManager;
    [SerializeField] private GameObject _hud;
    [SerializeField] private Button clientBtn;

    [SerializeField] TextMeshProUGUI ipAddressText;
    [SerializeField] TMP_InputField ip;

    [SerializeField] string ipAddress;
    [SerializeField] UnityTransport transport;


    private void Awake() {
        hostBtn.onClick.AddListener(() => {
            NetworkManager.Singleton.StartHost();
            GetLocalIPAddress();
            _hud.SetActive(true);
            _demoManager.SetActive(true);
            gameObject.SetActive(false);
        });

        clientBtn.onClick.AddListener(() => {
            ipAddress = ip.text;
            SetIpAddress();
            NetworkManager.Singleton.StartClient();
            _demoManager.SetActive(true);
            _hud.SetActive(true);
            gameObject.SetActive(false);
        });
    }

    private void Start() {
        ipAddress = "127.0.0.1";
        SetIpAddress(); // Set the Ip to the above address.
    }


    /* Gets the Ip Address of your connected network and
	shows on the screen in order to let other players join
	by inputing that Ip in the input field */
    // ONLY FOR HOST SIDE
    public string GetLocalIPAddress() {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList) {
            if (ip.AddressFamily == AddressFamily.InterNetwork) {
                ipAddressText.text = ip.ToString();
                ipAddress = ip.ToString();
                return ip.ToString();
            }
        }
        throw new System.Exception("No network adapters with an IPv4 address in the system!");
    }

    /* Sets the Ip Address of the Connection Data in Unity Transport
	to the Ip Address which was input in the Input Field */
    // ONLY FOR CLIENT SIDE
    public void SetIpAddress() {
        transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.ConnectionData.Address = ipAddress;
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

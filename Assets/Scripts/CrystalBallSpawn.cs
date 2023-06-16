using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CrystalBallSpawn : NetworkBehaviour {
    private float respawnTimeCrystalBall = 5f;

    // For the initial setting of the potion type.
    private bool isFirstRun = true;

    private SpriteRenderer crystalRenderer;

    // Networked variables.
    [SerializeField] public NetworkVariable<CrystalType> crystalType = new NetworkVariable<CrystalType>();
    

    private void Start() {
        crystalRenderer = GetComponent<SpriteRenderer>();
        if (crystalRenderer == null)
        {
            Debug.LogError("Crystal renderer is not assigned to the CyrstalBallSpawn object.");
        }
        // We want to update the color of the crystal ball with each change.
        crystalType.OnValueChanged += UpdateCrystalColor;
    }

    private void Update() {
        // Initialize the crystal ball with a random type. If we put it to Start() it does not work.
        // So with isFirstRun we create a little workaround.
        if (isFirstRun && NetworkManager.Singleton.IsHost) {
            InitializeCrystalTypeServerRpc();
            isFirstRun = false;
        } 
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            if (NetworkManager.Singleton.IsHost == false) {
                // Only the host is allowed to check for the collision.
                return;
            }
            // Get the target player's NetworkObject. We need this to also access its inventory later.
            NetworkObject targetNetworkObject = other.GetComponent<NetworkObject>();
            if (targetNetworkObject == null) {
                Debug.Log("The colliding player has no network object!");
                return;
            }
            // Tell the server to handle this instance.
            CrystalBallGotPickedUpServerRpc(targetNetworkObject.NetworkObjectId);
        }
    }

    [ServerRpc]
    private void InitializeCrystalTypeServerRpc() {
        crystalType.Value = GenerateCrystalType();
        Debug.Log("Initial crystal type: " + crystalType.Value.ToString());
    }

    // A function that runs on the server.
    // If a crystal ball got picked up, we need to know it. The client sends this information to the server.
    // The server then tells all clients to hide the cyrstal ball.
    [ServerRpc]
    private void CrystalBallGotPickedUpServerRpc(ulong targetPlayerNetworkObjectId) {
        // Find the target player's NetworkObject using the network object ID and the player stats.
        NetworkObject targetPlayerNetworkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[targetPlayerNetworkObjectId];
        PlayerInventory targetPlayerInventory = targetPlayerNetworkObject.GetComponent<PlayerInventory>();
        // Check if the inventory is already full or if the crystal ball can get picked up.
        if (targetPlayerInventory.AddCrystal(crystalType.Value) == true) {
            RpcSetCrystalBallActiveClientRpc(false);
            Invoke(nameof(RespawnCrystalBallServerRpc), respawnTimeCrystalBall);  
        }
    }

    // A function that runs on the server.
    // The server determines the next crystal ball type, and sends it to all clients.
    // The clients then update their crystal ball type and their visualization.
    [ServerRpc]
    private void RespawnCrystalBallServerRpc() {
        if (NetworkManager.Singleton.IsServer == false) {
            // Just curios.
            Debug.Log("This is not the server!");
            return;
        }
        // Determine the next crystal ball type on the server.
        crystalType.Value = GenerateCrystalType();
        // Make the crystals on all clients active. We need to set them active before
        // changing the color otherwise the color change will take no effect.
        RpcSetCrystalBallActiveClientRpc(true);
    }

    // Tell the clients to hide / unhide the crystal ball.
    [ClientRpc]
    private void RpcSetCrystalBallActiveClientRpc(bool isActive)
    {
        gameObject.SetActive(isActive);
    }

    private CrystalType GenerateCrystalType() {
        float randomValue = Random.value;
        // 23 % chance for fire, water, earth, air. remaining 8% for void.
        float chanceNormalTypes = 0.23f;
        if (randomValue < 1 * chanceNormalTypes) {
            return CrystalType.Fire;
        }
        else if (randomValue < 2 * chanceNormalTypes) {
            return CrystalType.Water;
        }
        else if (randomValue < 3 * chanceNormalTypes) {
            return CrystalType.Earth;
        }
        else if (randomValue < 4 * chanceNormalTypes) {
            return CrystalType.Air;
        }
        else {
            return CrystalType.Void;
        }
    }

    private void UpdateCrystalColor(CrystalType oldValue, CrystalType newValue) {
        // Assign the appropriate material based on the crystal type
        switch (newValue) {
            case CrystalType.Fire:
                crystalRenderer.sprite = Resources.Load<Sprite>("CrystalBalls/crystal-ball-fire");
                break;
            case CrystalType.Water:
                crystalRenderer.sprite = Resources.Load<Sprite>("CrystalBalls/crystal-ball-water");
                break;
            case CrystalType.Earth:
                crystalRenderer.sprite = Resources.Load<Sprite>("CrystalBalls/crystal-ball-earth");
                break;
            case CrystalType.Air:
                crystalRenderer.sprite = Resources.Load<Sprite>("CrystalBalls/crystal-ball-air");
                break;
            case CrystalType.Void:
                crystalRenderer.sprite = Resources.Load<Sprite>("CrystalBalls/crystal-ball-void");
                break;
        }
    }
}

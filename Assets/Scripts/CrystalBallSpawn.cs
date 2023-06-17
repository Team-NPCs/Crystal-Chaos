using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CrystalBallSpawn : NetworkBehaviour {
    private float respawnTimeCrystalBall = 5f;

    // For the initial setting of the crystal type.
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
        crystalType.OnValueChanged += UpdateCrystalColorEvent;
    }

    private void Update() {
        // Initialize the crystal ball with a random type. If we put it to Start() it does not work.
        // So with isFirstRun we create a little workaround.
        if (isFirstRun && NetworkManager.Singleton.IsHost) {
            InitializeCrystalTypeServerRpc();
            isFirstRun = false;
        } 
        else if (isFirstRun && NetworkManager.Singleton.IsClient) {
            // Update it if this is not the host. Because then the initial colors are already set and we just need to 
            // draw the correct crystal. If we do it immediately the new colors are not synced, so give it a little
            // bit of time.
            Invoke(nameof(UpdateCrystalColor), 0.5f);
            isFirstRun = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            if (NetworkManager.Singleton.IsServer == false) {
                // Only the host is allowed to check for the collision.
                // Because the networked objects (crystal balls) are owned by the host, only
                // the host can send the serverRPCs to the server.
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
        if (targetPlayerInventory.CanAddCrystal(crystalType.Value) == true) {
            // Ok the player who touched the crystal ball can pick it up.
            // Tell the server of this players inventory that it has to add a crystal ball of the given type.
            // The server then tells the respective client to add the crystal ball.
            // Here is the problem. The players inventory is owned by the player. In order to access it,
            // the local player needs to call it for his own inventory. But since the crystal balls are owned by
            // the host we have a dilemma.

            targetPlayerInventory.AddCrystal(crystalType.Value);
            // Hide the crystal ball but respawn it after a specific time.
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

    public static CrystalType GenerateCrystalType() {
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

    // Used for the event handler.
    private void UpdateCrystalColorEvent(CrystalType oldValue, CrystalType newValue) {
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

    // Used for manual updating.
    private void UpdateCrystalColor() {
        // Assign the appropriate material based on the crystal type
        switch (crystalType.Value) {
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

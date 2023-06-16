using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PotionSpawn : NetworkBehaviour {
    public int healthIncreaseAmount = 10;
    public float healthSpawnProbability = 0.6f;
    public float movementSpawnProbability = 0.4f;
    public float respawnTimePotion = 5.0f;

    private Animator potionAnimator;

    public PotionType potionType;

    private void Start() {
        potionAnimator = GetComponent<Animator>();
        if (potionAnimator == null)
        {
            Debug.LogError("Potion Animator is not assigned to the PotionSpawn object.");
        }
        RespawnPotionServerRpc();
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            if (NetworkManager.Singleton.IsHost == false) {
                // Only the host is allowed to check for the collision.
                return;
            }
            // Get the target player's NetworkObject. We need this to also access its player stats later.
            NetworkObject targetNetworkObject = other.GetComponent<NetworkObject>();
            if (targetNetworkObject == null) {
                Debug.Log("The colliding player has no network object!");
                return;
            }
            // Tell the server to handle this instance.
            PotionGotPickedUpServerRpc(targetNetworkObject.NetworkObjectId);
        }
    }

    // A function that runs on the server.
    // If a potion got picked up, we need to know it. The client sends this information to the server.
    // The server then tells all clients to hide the potion.
    [ServerRpc]
    private void PotionGotPickedUpServerRpc(ulong targetPlayerNetworkObjectId) {
        // Find the target player's NetworkObject using the network object ID and the player stats.
        NetworkObject targetPlayerNetworkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[targetPlayerNetworkObjectId];
        PlayerStats targetPlayerStats = targetPlayerNetworkObject.GetComponent<PlayerStats>();
        // Apply potion effects based on type.
        bool potionGotPickedUp = false;
        switch (potionType)
        {
            case PotionType.Health:
                potionGotPickedUp = targetPlayerStats.IncreaseHealth(healthIncreaseAmount);
                break;
            case PotionType.Movement:
                targetPlayerStats.ActivateFastSpeed();
                potionGotPickedUp = true;
                break;
        }
        if (potionGotPickedUp == true) {
            // Make the potions on all clients inactive.
            RpcSetPotionActiveClientRpc(false);
            // Make the potions appear again.
            Invoke(nameof(RespawnPotionServerRpc), respawnTimePotion);  
        }
    }

    // A function that runs on the server.
    // The server determines the next potion type, and sends it to all clients.
    // The clients then update their potion type and their visualization.
    [ServerRpc]
    private void RespawnPotionServerRpc() {
        if (NetworkManager.Singleton.IsServer == false) {
            // Just curios.
            Debug.Log("This is not the server!");
        }
        // Determine the next potion type on the server.
        PotionType nextPotionType = GeneratePotionType();
        // Make the potions on all clients active. We need to set them active before
        // changing the color otherwise the color change will take no effect.
        RpcSetPotionActiveClientRpc(true);
        // Send the information to all clients. They will also load the new animation.
        RpcSetPotionTypeClientRpc(nextPotionType);
    }

    private PotionType GeneratePotionType() {
        float totalSpawnProbability = healthSpawnProbability + movementSpawnProbability;
        float randomValue = Random.value * totalSpawnProbability;
        PotionType nextPotionType;
        if (randomValue <= healthSpawnProbability) {
            nextPotionType = PotionType.Health;
        }
        else {
            nextPotionType = PotionType.Movement;
        }
        return nextPotionType;
    }

    // This loads the new animation (depending on the potion type).
    private void UpdatePotionColor() {
        switch (potionType) {
            case PotionType.Health:
                potionAnimator.Play("red_potion");
                break;
            case PotionType.Movement:
                Debug.Log("I am here.");
                potionAnimator.Play("blue_potion");
                break;
        }
    }

    // Tell the client the new potion type.
    [ClientRpc]
    private void RpcSetPotionTypeClientRpc(PotionType type)
    {
        Debug.Log("Got information about the new potion type:" + type.ToString());
        if (type != potionType) {
            potionType = type;
            Debug.Log("Update color.");
            UpdatePotionColor();
        }
    }

    // Tell the client to hide / unhide the potion.
    [ClientRpc]
    private void RpcSetPotionActiveClientRpc(bool isActive)
    {
        gameObject.SetActive(isActive);
    }
}
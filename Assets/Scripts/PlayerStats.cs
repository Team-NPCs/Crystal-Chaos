using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerStats : NetworkBehaviour {
    public float initialSpeedFactor;
    public float fastSpeedFactor;
    public float speedIncreaseDuration;
    public int initialHealth;
    public int maxHealth;

    // Networked variables.
    [SerializeField] public NetworkVariable<float> speedFactor = new NetworkVariable<float>();
    [SerializeField] public NetworkVariable<int> numberOfSpeedIncreasements = new NetworkVariable<int>();
    [SerializeField] public NetworkVariable<int> health = new NetworkVariable<int>();
    [SerializeField] public NetworkVariable<int> deathCount = new NetworkVariable<int>();
    
    // Health Bar.
    [SerializeField] private HealthBar localPlayerHealthBar;

    private void Start() {
        // Find and assign the local player's health bar.
        localPlayerHealthBar = GameObject.FindWithTag("HealthBar").GetComponent<HealthBar>();
        // Add the event listeners to the healthbar.
        health.OnValueChanged += UpdateHealthBar;
        // Initialize the values.
        speedFactor.Value = initialSpeedFactor;
        health.Value = initialHealth;
        // Update the health bar at the start.
        UpdateHealthBar(0, health.Value);
        numberOfSpeedIncreasements.Value = 0;
        deathCount.Value = 0;
    }

    void Update () {
        // Just a way to debug the health decreasement and respawn logic.
        if (Input.GetKeyDown(KeyCode.G) && IsLocalPlayer()) {
            DecreaseHealthServerRpc(gameObject.GetComponent<NetworkObject>().NetworkObjectId);
        }
    }

    // An example function that runs on the server that decreases the health of the player.
    // We can not do it locally, the server has to do it for us.
    [ServerRpc]
    public void DecreaseHealthServerRpc(ulong targetPlayerNetworkObjectId) {
        // Find the target player's NetworkObject using the network object ID and the player stats.
        NetworkObject targetPlayerNetworkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[targetPlayerNetworkObjectId];
        PlayerStats targetPlayerStats = targetPlayerNetworkObject.GetComponent<PlayerStats>();
        targetPlayerStats.DecreaseHealth(20);
    }

    // The thing is, that we do not allow the server to set the position of the client using the Client Network Transform.
    // Therefore, the client has to respawn himself. This function is executed on the client but triggered by the server.
    // So we handle the health logic on the server (increasing, decreasing, death) but the respawn position on the client.
    [ClientRpc]
    private void RespawnClientRpc()
    {
        PlayerSpawn playerSpawn = GameObject.FindGameObjectWithTag("SpawnPointHandler").GetComponent<PlayerSpawn>();
        transform.position = playerSpawn.GetRespawnPosition();
    }

    public void ActivateFastSpeed() {
        numberOfSpeedIncreasements.Value++;
        speedFactor.Value = fastSpeedFactor;
        Invoke(nameof(DeactivateFastSpeed), speedIncreaseDuration);
        Debug.Log("Increase speed. Number of increasements: " + numberOfSpeedIncreasements.Value.ToString());
    }

    public void DeactivateFastSpeed() {
        numberOfSpeedIncreasements.Value--;
        Debug.Log("Decrease speed. Number of increasements: " + numberOfSpeedIncreasements.Value.ToString());
        if (numberOfSpeedIncreasements.Value == 0) {
            Debug.Log("Reset speed.");
            ResetSpeed();
        }
        else if (numberOfSpeedIncreasements.Value < 0) {
            Debug.Log("Programming error using numberOfSpeedPotionsInUse.");
        }
    }

    public void ResetSpeed() {
        speedFactor.Value = initialSpeedFactor;
    }

    public bool IncreaseHealth(int amount) {
        // We use a bool function to return false if the players health is already full,
        // so the potion gets not picked up.
        if (health.Value < maxHealth)
        {
            health.Value += amount;
            if (health.Value > maxHealth)
            {
                health.Value = maxHealth;
            }
            return true;
        }
        return false;
    }

    public void DecreaseHealth(int amount) {
        health.Value -= amount;
        if (health.Value < 0) {
            health.Value = 0;
        }
        if (health.Value == 0) {
            RespawnClientRpc();
            health.Value = maxHealth;
            deathCount.Value++;
        }
    }

    // Check if the player that this script is assigned to is the local player.
    // The thing is, that we have multiple players at the end within the game. Each player has the 
    // playerStats and it does not matter what playerstats is changed, the bar will change if we do
    // not check which stats were changed. So only visualize the health of the own player / local player.
    private bool IsLocalPlayer()
    {
        // Replace this with your own logic to determine if this instance is the local player
        // For example, you can compare the NetworkClientId with the local client's NetworkClientId
        return NetworkManager.Singleton.LocalClientId == GetComponent<NetworkObject>().OwnerClientId;
    }

    // Event handler for updating the health bar
    private void UpdateHealthBar(int oldValue, int newValue)
    {
        // Only update the health bar for the local player
        if (IsLocalPlayer())
        {
            localPlayerHealthBar.setHealth(newValue);
        }
    }
}
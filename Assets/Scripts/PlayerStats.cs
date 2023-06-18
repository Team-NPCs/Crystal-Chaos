using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerStats : NetworkBehaviour {
    public float initialSpeedFactor;
    public float fastSpeedFactor;
    public float speedIncreaseDuration;
    public int maxHealth;

    // Networked variables.
    [SerializeField] public NetworkVariable<float> speedFactor = new NetworkVariable<float>();
    [SerializeField] public NetworkVariable<int> numberOfSpeedIncreasements = new NetworkVariable<int>();
    [SerializeField] public NetworkVariable<int> health = new NetworkVariable<int>();
    [SerializeField] public NetworkVariable<int> deathCount = new NetworkVariable<int>();
    
    // Health Bar and Pause Menu (kill count).
    [SerializeField] private HealthBar localPlayerHealthBar;
    [SerializeField] private GamePauseUI localGamePauseUI;

    private void Start() {
        // Find and assign the local player's health bar and the game pause menu.
        localPlayerHealthBar = GameObject.FindWithTag("HealthBar").GetComponent<HealthBar>();
        // It is a little bit difficult to access the gamepauseUI if it is inactive. So we stored it
        // in the demomangager (our object manager).
        // Find the demo manager GameObject in the scene
        GameObject demoManagerGO = GameObject.FindWithTag("DemoManagerTag");
        // Access the script attached to the demo managers GameObject.
        DemoManager demoManager = demoManagerGO.GetComponent<DemoManager>();
        // Access the gamePauseMenuObject reference
        localGamePauseUI = demoManager._gamePauseUI;
        // Add the event listener to the healthbar.
        health.OnValueChanged += UpdateHealthBar;
        // Add the event listener to the deathcount.
        deathCount.OnValueChanged += UpdateScore;
        // Initialize the values.
        speedFactor.Value = initialSpeedFactor;
        health.Value = maxHealth;
        // Update the health bar at the start.
        UpdateHealthBar(0, health.Value);
        numberOfSpeedIncreasements.Value = 0;
        deathCount.Value = 0;
    }

    void Update () {
        // Just a way to debug the health decreasement and respawn logic.
        if (Input.GetKeyDown(KeyCode.G) && IsLocalPlayer()) {
            DecreaseHealthServerRpc(gameObject.GetComponent<NetworkObject>().NetworkObjectId, 20);
        }
    }

    // An function that runs on the server that decreases the health of the player.
    // We can not do it locally, the server has to do it for us.
    [ServerRpc]
    public void DecreaseHealthServerRpc(ulong targetPlayerNetworkObjectId, int amount) {
        // Find the target player's NetworkObject using the network object ID and the player stats.
        NetworkObject targetPlayerNetworkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[targetPlayerNetworkObjectId];
        PlayerStats targetPlayerStats = targetPlayerNetworkObject.GetComponent<PlayerStats>();
        bool playerDied = targetPlayerStats.DecreaseHealth(amount);
        // If the player died, reset the inventory and add a random crystal ball.
        if (playerDied) {
            // Reset the inventory. The player loses all when he dies.
            PlayerInventory targetPlayerInventory = targetPlayerNetworkObject.GetComponent<PlayerInventory>();
            targetPlayerInventory.ResetCrystal();
            // Add a new random crystal ball to the inventory.
            targetPlayerInventory.AddRandomCrystal();
        }
    }

    // The thing is, that we do not allow the server to set the position of the client using the Client Network Transform.
    // Therefore, the client has to respawn himself. This function is executed on the client but triggered by the server.
    // So we handle the health logic on the server (increasing, decreasing, death) but the respawn position on the client.
    [ClientRpc]
    private void RespawnClientRpc()
    {
        // Get the new position and move there.
        PlayerSpawn playerSpawn = GameObject.FindGameObjectWithTag("SpawnPointHandler").GetComponent<PlayerSpawn>();
        transform.position = playerSpawn.GetRespawnPosition();
        UpdateHealthBar(0, maxHealth);
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

    public bool DecreaseHealth(int amount) {
        // This function returns true if the player died.
        health.Value -= amount;
        if (health.Value < 0) {
            health.Value = 0;
        }
        if (health.Value == 0) {
            health.Value = maxHealth;
            deathCount.Value++;
            RespawnClientRpc();
            return true;
        }
        else {
            return false;
        }
    }

    // Event handler for updating the health bar
    private void UpdateHealthBar(int oldValue, int newValue) {
        // Only update the health bar for the local player
        if (IsLocalPlayer())
        {
            localPlayerHealthBar.setHealth(newValue);
        }
    }

    // Event handler for the score.
    private void UpdateScore (int oldValue, int newValue) {
        // The value we get is the death count of a player.
        // First check if the value changed for the local or for the other player.
        // If the death count of the local player changed, it is equal to the kill count of the other player.
        // So this is then for the enemy.
        // If the death count of the other player changed, this is then for my own player.
        if (IsLocalPlayer()) {
            // I died. This is a death for me.
            localGamePauseUI.AdjustNumberOfDeath(newValue);
        }
        else {
            // The enemy died. This is a kill for me.
            localGamePauseUI.AdjustNumberOfKill(newValue);
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
}
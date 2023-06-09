using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PotionSpawn : NetworkBehaviour {
    private readonly int healthIncreaseAmount = 60;
    private readonly float healthSpawnProbability = 0.6f;
    private readonly float movementSpawnProbability = 0.4f;
    private readonly float respawnTimePotion = 10.0f;
    // For the initial setting of the potion type.
    private bool isFirstRun = true;
    private Animator potionAnimator;
    // Audio.
    private SfxScript sfxScript;

    // Networked variables.
    [SerializeField] public NetworkVariable<PotionType> potionType = new NetworkVariable<PotionType>();

    private void Start() {
        potionAnimator = GetComponent<Animator>();
        if (potionAnimator == null) {
            Debug.LogError("Potion Animator is not assigned to the PotionSpawn object.");
        }
        // We want to update the color of the potion with each change.
        potionType.OnValueChanged += UpdatePotionColorEvent;
        // Audio.
        sfxScript = GameObject.FindGameObjectWithTag("SfxManager").GetComponent<SfxScript>();
    }

    private void Update() {
        // Initialize the potion with a random type. If we put it to Start() it does not work.
        // So with isFirstRun we create a little workaround.
        if (isFirstRun && NetworkManager.Singleton.IsHost) {
            InitializePotionTypeServerRpc();
            isFirstRun = false;
        }
    }

    // When the client spawns on the network, he needs to update the potion colors.
    public override void OnNetworkSpawn() {
        if (IsClient) {
            // Object is spawned on the client, so set the bullet sprite
            UpdatePotionColor();
        }
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

    [ServerRpc]
    private void InitializePotionTypeServerRpc() {
        potionType.Value = GeneratePotionType();
        Debug.Log("Initial potion type: " + potionType.Value.ToString());
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
        switch (potionType.Value) {
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
            return;
        }
        // Make the potions on all clients active. We need to set them active before
        // changing the color otherwise the color change will take no effect.
        RpcSetPotionActiveClientRpc(true);
        // Determine the next potion type on the server.
        potionType.Value = GeneratePotionType();
        // Update the potion color on the server.
        UpdatePotionColor();
    }

    // Tell the client to hide / unhide the potion.
    [ClientRpc]
    private void RpcSetPotionActiveClientRpc(bool isActive) {
        gameObject.SetActive(isActive);
        // If it is set to false, it means the potion was drunk so play the sound.
        if (isActive == false) {
            AudioSource audioSource = sfxScript.potionAudio[potionType.Value];
            audioSource.volume = sfxScript.soundVolume;
            audioSource.Play();
        }
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
    // It will be used for the event handler.
    private void UpdatePotionColorEvent(PotionType oldValue, PotionType newValue) {
        switch (newValue) {
            case PotionType.Health:
                potionAnimator.Play("red_potion");
                break;
            case PotionType.Movement:
                potionAnimator.Play("blue_potion");
                break;
        }
    }

    // This loads the new animation (depending on the potion type).
    // It will be used for manual updating the color.
    private void UpdatePotionColor() {
        switch (potionType.Value) {
            case PotionType.Health:
                potionAnimator.Play("red_potion");
                break;
            case PotionType.Movement:
                potionAnimator.Play("blue_potion");
                break;
        }
    }
}
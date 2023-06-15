using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PotionSpawn : NetworkBehaviour {
    public PotionType potionType;
    public int healthIncreaseAmount = 10;
    public float healthSpawnProbability = 0.6f;
    public float movementSpawnProbability = 0.4f;
    public float respawnTimePotion = 5.0f;

    private Animator potionAnimator;

    private void Start() {
        potionAnimator = GetComponent<Animator>();
        if (potionAnimator == null)
        {
            Debug.LogError("Potion Animator is not assigned to the PotionSpawn object.");
        }
        RespawnPotion();
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            PlayerStats playerStats = other.GetComponent<PlayerStats>();
            if (playerStats != null) {
                // Apply potion effects based on type
                bool getsPickedUp = false;
                switch (potionType) {
                    case PotionType.Health:
                        // Check if the health potion can get used by the player.
                        getsPickedUp = playerStats.IncreaseHealth(healthIncreaseAmount);
                        break;
                    case PotionType.Movement:
                        ActivateSpeedBoost(playerStats);
                        getsPickedUp = true;
                        break;
                }
                if (getsPickedUp == true) {
                    gameObject.SetActive(false);
                    Invoke(nameof(RespawnPotion), respawnTimePotion);
                }
            }
            else {
                Debug.Log("Can not find players stats.");
            }
        }
    }

    private void ActivateSpeedBoost(PlayerStats playerStats) {
        playerStats.ActivateFastSpeed();
    }

    private void RespawnPotion() {
        gameObject.SetActive(true);
        if (NetworkManager.Singleton.IsHost)
        {
            // Generate the potions type on the server.
            GeneratePotionType();
            RpcSetPotionTypeClientRpc(potionType);
        }
        UpdatePotionColor();
    }

    private void GeneratePotionType() {
        float totalSpawnProbability = healthSpawnProbability + movementSpawnProbability;
        float randomValue = Random.value * totalSpawnProbability;
        if (randomValue <= healthSpawnProbability) {
            potionType = PotionType.Health;
        }
        else {
            potionType = PotionType.Movement;
        }
    }

    private void UpdatePotionColor() {
        switch (potionType) {
            case PotionType.Health:
                //potionRenderer.color = Color.red;
                potionAnimator.Play("red_potion");
                break;
            case PotionType.Movement:
                //potionRenderer.color = Color.blue;
                potionAnimator.Play("blue_potion");
                //potionRenderer.material = Resources.Load<Material>("PotionMovement");
                break;
        }
    }

    [ClientRpc]
    private void RpcSetPotionTypeClientRpc(PotionType type)
    {
        potionType = type;
    }
}
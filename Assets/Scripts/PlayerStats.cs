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

    public float speedFactor;
    public int health;
    public int deathCount = 0;
    [SerializeField] private HealthBar bar;

    private int numberOfSpeedIncreasements = 0;

    private void Start() {
        bar = GameObject.FindWithTag("HealthBar").GetComponent<HealthBar>();
        speedFactor = initialSpeedFactor;
        health = initialHealth;
        bar.setHealth(health);
    }

    void Update () {
        if (NetworkObject.IsLocalPlayer && Input.GetKeyDown(KeyCode.G)) {
            Debug.Log("Pressed Key G.");
            this.DecreaseHealth(20);
        }
    }

    public void ActivateFastSpeed() {
        if (NetworkObject.IsLocalPlayer) {
            numberOfSpeedIncreasements++;
            speedFactor = fastSpeedFactor;
            Invoke(nameof(DeactivateFastSpeed), speedIncreaseDuration);
            Debug.Log("Increase speed. Number of increasements: " + numberOfSpeedIncreasements.ToString());
        }
    }

    public void DeactivateFastSpeed() {
        if (NetworkObject.IsLocalPlayer) {
            numberOfSpeedIncreasements--;
            Debug.Log("Decrease speed. Number of increasements: " + numberOfSpeedIncreasements.ToString());
            if (numberOfSpeedIncreasements == 0) {
                Debug.Log("Reset speed.");
                ResetSpeed();
            }
            else if (numberOfSpeedIncreasements < 0) {
                Debug.Log("Programming error using numberOfSpeedPotionsInUse.");
            }
        }
    }

    public void ResetSpeed() {
        speedFactor = initialSpeedFactor;
    }

    public bool IncreaseHealth(int amount) {
        // We use a bool function to return false if the players health is already full,
        // so the potion gets not picked up.
        if (NetworkObject.IsLocalPlayer && health < maxHealth)
        {
            health += amount;
            if (health > maxHealth)
            {
                health = maxHealth;
            }
            bar.setHealth(health);
            return true;
        }
        return false;
    }

    public void DecreaseHealth(int amount) {
        if (NetworkObject.IsLocalPlayer) {
            health -= amount;
            if (health < 0) {
                health = 0;
            }
            if (health == 0) {
                // Reset the health.
                health = this.maxHealth;
                // Respawn the player.
                Debug.Log("Player was killed. Respawn.");
                deathCount++;
                PlayerSpawn playerSpawn = GetComponent<PlayerSpawn>();
                playerSpawn.Respawn();
            }
            bar.setHealth(health);
        }
    }
}
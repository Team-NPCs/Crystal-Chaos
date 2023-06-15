using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour {
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
        if (Input.GetKeyDown(KeyCode.G)) {
            Debug.Log("Pressed Key G.");
            this.DecreaseHealth(20);
        }
    }

    public void ActivateFastSpeed() {
        numberOfSpeedIncreasements++;
        speedFactor = fastSpeedFactor;
        Invoke("DeactivateFastSpeed", speedIncreaseDuration);
        Debug.Log("Increase speed. Number of increasements: " + numberOfSpeedIncreasements.ToString());
    }

    public void DeactivateFastSpeed() {
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

    public void ResetSpeed() {
        speedFactor = initialSpeedFactor;
    }

    public bool IncreaseHealth(int amount) {
        // We use a bool function to return false if the players health is already full,
        // so the potion gets not picked up.
        if (health >= maxHealth) {
            return false;
        }
        else {
            health += amount;
            if (health > maxHealth) {
                health = maxHealth;
            }
            bar.setHealth(health);
            return true;
        }
    }

    public void DecreaseHealth(int amount) {
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
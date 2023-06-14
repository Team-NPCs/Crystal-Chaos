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

    private int numberOfSpeedIncreasements = 0;

    private void Start() {
        speedFactor = initialSpeedFactor;
        health = initialHealth;
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
            return true;
        }
    }

    public void DecreaseHealth(int amount) {
        health -= amount;
        if (health < 0) {
            health = 0;
        }
    }
}
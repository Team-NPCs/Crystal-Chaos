using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour {
    public float initialSpeed = 5f;
    public float fastSpeed = 10f;
    private int numberOfSpeedPotionsInUse = 0;
    public int initialHealth = 100;
    public int maxHealth = 100;

    private float speed;
    private int health;

    private void Start() {
        speed = initialSpeed;
        health = initialHealth;
    }

    public void IncreaseSpeed(float amount) {
        speed += amount;
    }

    public void DecreaseSpeed(float amount) {
        speed -= amount;
        if (speed < 0f) {
            speed = 0f;
        }
    }

    public void ActivateFastSpeed() {
        numberOfSpeedPotionsInUse += 1;
        speed = fastSpeed;
    }

    public void DeactivateFastSpeed() {
        numberOfSpeedPotionsInUse -= 1;
        if (numberOfSpeedPotionsInUse == 0) {
            ResetSpeed();
        }
        else if (numberOfSpeedPotionsInUse < 0) {
            Debug.Log("Programming error using numberOfSpeedPotionsInUse.");
        }
    }

    public void ResetSpeed() {
        speed = initialSpeed;
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
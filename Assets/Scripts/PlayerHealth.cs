using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerHealth : MonoBehaviour {
    public int maxHealth = 100;
    public int currentHealth;
    public TextMeshProUGUI healthNumber;

    public HealthBar healthBar;
    // Start is called before the first frame update
    void Start() {
        currentHealth = maxHealth;
        healthBar.SetMaxHealth(maxHealth);

    }

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(KeyCode.G)) {
            TakeDamage(10);
        }
    }

    private void TakeDamage(int damage) {
        currentHealth -= damage;
        healthBar.setHealth(currentHealth);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PotionSpawn : MonoBehaviour {
    public PotionType potionType;
    public int healthIncreaseAmount = 40;
    public float speedIncreaseAmount = 2.5f;
    public float speedIncreaseDuration = 10f;
    public float healthSpawnProbability = 0.6f;
    public float movementSpawnProbability = 0.4f;
    public float respawnTimePotion = 5.0f;

    private Animator potionAnimator;

    private void Start() {
        potionAnimator = GetComponent<Animator>();
        GeneratePotionType();
        UpdatePotionColor();
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
                        StartCoroutine(ActivateSpeedBoost(playerStats));
                        getsPickedUp = true;
                        break;
                }
                if (getsPickedUp == true) {
                    gameObject.SetActive(false);
                    Invoke("RespawnPotion", respawnTimePotion);
                }
            }
            else {
                Debug.Log("Can not find players stats.");
            }
        }
    }

    private System.Collections.IEnumerator ActivateSpeedBoost(PlayerStats playerStats) {
        playerStats.ActivateFastSpeed();
        yield return new WaitForSeconds(speedIncreaseDuration);
        playerStats.DeactivateFastSpeed();
    }

    private void RespawnPotion() {
        gameObject.SetActive(true);
        GeneratePotionType();
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
}
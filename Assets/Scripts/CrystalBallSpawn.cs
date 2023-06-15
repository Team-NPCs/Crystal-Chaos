using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CrystalBallSpawn : NetworkBehaviour {
    private CrystalType crystalType;
    private float respawnTime = 5f;

    private SpriteRenderer crystalRenderer;

    private void Start() {
        crystalRenderer = GetComponent<SpriteRenderer>();
        if (crystalRenderer == null)
        {
            Debug.LogError("Crystal renderer is not assigned to the CyrstalBallSpawn object.");
        }
        RespawnCrystal();
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            PlayerInventory inventory = other.GetComponent<PlayerInventory>();
            if (inventory != null) {
                // Check if the inventory is already full or if the crystal ball can get picked up.
                if (inventory.AddCrystal(crystalType) == true) {
                    gameObject.SetActive(false);
                    Invoke(nameof(RespawnCrystal), respawnTime);
                }
            }
            else {
                Debug.Log("Can not find players inventory.");
            }
        }
    }

    private void RespawnCrystal() {
        gameObject.SetActive(true);
        if (NetworkManager.Singleton.IsHost)
        {
            // Generate the potions type on the server.
            GenerateCrystalType();
            RpcSetCrystalTypeClientRpc(crystalType);
        }
        UpdateCrystalColor();
    }

    private void GenerateCrystalType() {
        float randomValue = Random.value;
        // 23 % chance for fire, water, earth, air. remaining 8% for void.
        float chanceNormalTypes = 0.23f;
        if (randomValue < 1 * chanceNormalTypes) {
            crystalType = CrystalType.Fire;
        }
        else if (randomValue < 2 * chanceNormalTypes) {
            crystalType = CrystalType.Water;
        }
        else if (randomValue < 3 * chanceNormalTypes) {
            crystalType = CrystalType.Earth;
        }
        else if (randomValue < 4 * chanceNormalTypes) {
            crystalType = CrystalType.Air;
        }
        else {
            crystalType = CrystalType.Void;
        }
    }

    private void UpdateCrystalColor() {
        // Assign the appropriate material based on the crystal type
        switch (crystalType) {
            case CrystalType.Fire:
                crystalRenderer.sprite = Resources.Load<Sprite>("CrystalBalls/crystal-ball-fire");
                break;
            case CrystalType.Water:
                crystalRenderer.sprite = Resources.Load<Sprite>("CrystalBalls/crystal-ball-water");
                break;
            case CrystalType.Earth:
                crystalRenderer.sprite = Resources.Load<Sprite>("CrystalBalls/crystal-ball-earth");
                break;
            case CrystalType.Air:
                crystalRenderer.sprite = Resources.Load<Sprite>("CrystalBalls/crystal-ball-air");
                break;
            case CrystalType.Void:
                crystalRenderer.sprite = Resources.Load<Sprite>("CrystalBalls/crystal-ball-void");
                break;
        }
    }

    [ClientRpc]
    private void RpcSetCrystalTypeClientRpc(CrystalType type)
    {
        crystalType = type;
    }
}

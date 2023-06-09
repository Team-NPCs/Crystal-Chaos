using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CrystalType
{
    Fire,
    Water,
    Earth,
    Air,
    Void
}

public class CrystalBall : MonoBehaviour
{
    private CrystalType crystalType;
    private float respawnTime = 5f;

    private SpriteRenderer crystalRenderer;

    private void Start()
    {
        crystalRenderer = GetComponent<SpriteRenderer>();
        GenerateCrystalType();
        UpdateCrystalColor();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            gameObject.SetActive(false);
            Invoke("RespawnCrystal", respawnTime);
            /*
            PlayerInventory inventory = other.GetComponent<PlayerInventory>();
            if (inventory != null)
            {
                inventory.AddCrystal(crystalType);
                gameObject.SetActive(false);
                Invoke("RespawnCrystal", respawnTime);
            }
            */
        }
    }

    private void RespawnCrystal()
    {
        gameObject.SetActive(true);
        GenerateCrystalType();
        UpdateCrystalColor();
    }

    private void GenerateCrystalType()
    {
        float randomValue = Random.value;
         // 23 % chance for fire, water, earth, air. remaining 8% for void.
        float chance_normal_types = 0.23f;
        if (randomValue < 1 * chance_normal_types) 
        {
            crystalType = CrystalType.Fire;
        }
        else if (randomValue < 2 * chance_normal_types)
        {
            crystalType = CrystalType.Water;
        }
        else if (randomValue < 3 * chance_normal_types)
        {
            crystalType = CrystalType.Earth;
        }
        else if (randomValue < 4 * chance_normal_types)
        {
            crystalType = CrystalType.Air; 
        }
        else
        {
            crystalType = CrystalType.Void;
        }
    }

    private void UpdateCrystalColor()
    {
        // Assign the appropriate material based on the crystal type
        switch (crystalType)
        {
            case CrystalType.Fire:
                crystalRenderer.color = Color.red; 
                //crystalRenderer.material = Resources.Load<Material>("CrystalFire");
                break;
            case CrystalType.Water:
                crystalRenderer.color = Color.blue;
                //crystalRenderer.material = Resources.Load<Material>("CrystalWater");
                break;
            case CrystalType.Earth:
                crystalRenderer.color = Color.gray;
                //crystalRenderer.material = Resources.Load<Material>("CrystalEarth");
                break;
            case CrystalType.Air:
                crystalRenderer.color = Color.magenta;
                //crystalRenderer.material = Resources.Load<Material>("CrystalAir");
                break;
            case CrystalType.Void:
                crystalRenderer.color = Color.black;
                //crystalRenderer.material = Resources.Load<Material>("CrystalVoid");
                break;
        }
    }
}

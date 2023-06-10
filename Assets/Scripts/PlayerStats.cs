using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public float initialSpeed = 5f;
    public int initialHealth = 100;
    public int maxHealth = 100;

    private float speed;
    private int health;

    private void Start()
    {
        speed = initialSpeed;
        health = initialHealth;
    }

    public void IncreaseSpeed(float amount)
    {
        speed += amount;
    }

    public void DecreaseSpeed(float amount)
    {
        speed -= amount;
        if (speed < 0f)
        {
            speed = 0f;
        }
    }

    public void ResetSpeed ()
    {
        speed = initialSpeed;
    }

    public void IncreaseHealth(int amount)
    {
        health += amount;
        if (health > maxHealth)
        {
            health = maxHealth;
        }
    }

    public void DecreaseHealth(int amount)
    {
        health -= amount;
        if (health < 0)
        {
            health = 0;
        }
    }
}
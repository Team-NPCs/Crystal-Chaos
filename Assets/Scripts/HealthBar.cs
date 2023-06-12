using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Slider slider;
    public TextMeshProUGUI number;

    public void SetMaxHealth(int health)
    {
        slider.maxValue = health;
        slider.value = health;
        number.text = health.ToString();
    }

    public void setHealth(int health)
    {
        slider.value = health;
        number.text = health.ToString();
    }
}

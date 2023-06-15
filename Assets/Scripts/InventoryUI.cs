using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI fireAmmunition;
    [SerializeField] private TextMeshProUGUI waterAmmunition;
    [SerializeField] private TextMeshProUGUI earthAmmunition;
    [SerializeField] private TextMeshProUGUI airAmmunition;
    [SerializeField] private TextMeshProUGUI voidAmmunition;

    [SerializeField] private TextMeshProUGUI fireCrystalBall;
    [SerializeField] private TextMeshProUGUI waterCrystalBall;
    [SerializeField] private TextMeshProUGUI earthCrystalBall;
    [SerializeField] private TextMeshProUGUI airCrystalBall;
    [SerializeField] private TextMeshProUGUI voidCrystalBall;

    public void adjustInventory(CrystalType crystalBall, int numberAmmunition, int maxAmmunition, int numberCrystalBall) {
        if(numberAmmunition == 0 && numberCrystalBall == 1) {
            numberCrystalBall--;
        }
        if (numberAmmunition == 0 && numberCrystalBall > 1) {
            numberAmmunition = maxAmmunition;
            numberCrystalBall--;
        }
        switch (crystalBall) {
            case CrystalType.Fire:
                fireAmmunition.text = numberAmmunition.ToString();
                fireCrystalBall.text = numberCrystalBall.ToString();
                break;
            case CrystalType.Water:
                waterAmmunition.text = numberAmmunition.ToString();
                waterCrystalBall.text = numberCrystalBall.ToString();
                break;
            case CrystalType.Earth:
                earthAmmunition.text = numberAmmunition.ToString();
                earthCrystalBall.text = numberCrystalBall.ToString();
                break;
            case CrystalType.Air:
                airAmmunition.text = numberAmmunition.ToString();
                airCrystalBall.text = numberCrystalBall.ToString();
                break;
            case CrystalType.Void:
                voidAmmunition.text = numberAmmunition.ToString();
                voidCrystalBall.text = numberCrystalBall.ToString();
                break;
            default:
                break;
        }
    }
}

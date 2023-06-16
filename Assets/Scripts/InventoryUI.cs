using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    [SerializeField] private Image fireSelection;
    [SerializeField] private Image waterSelection;
    [SerializeField] private Image earthSelection;
    [SerializeField] private Image airSelection;
    [SerializeField] private Image voidSelection;

    [SerializeField] private Color selectionColor;
    [SerializeField] private Color noSelectedColor;

    public void AdjustInventory(CrystalType crystalBall, int numberAmmunition, int numberCrystalBall) {
        if(numberAmmunition == 0) {
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

    public void adjustColorSelection(CrystalType crystalBall) {
        switch (crystalBall) {
            case CrystalType.Fire:
                fireSelection.color = selectionColor; //
                waterSelection.color = noSelectedColor;
                earthSelection.color = noSelectedColor;
                airSelection.color = noSelectedColor;
                voidSelection.color = noSelectedColor;
                break;
            case CrystalType.Water:
                fireSelection.color = noSelectedColor;
                waterSelection.color = selectionColor; //
                earthSelection.color = noSelectedColor;
                airSelection.color = noSelectedColor;
                voidSelection.color = noSelectedColor;
                break;
            case CrystalType.Earth:
                fireSelection.color = noSelectedColor;
                waterSelection.color = noSelectedColor;
                earthSelection.color = selectionColor; //
                airSelection.color = noSelectedColor;
                voidSelection.color = noSelectedColor;
                break;
            case CrystalType.Air:
                fireSelection.color = noSelectedColor;
                waterSelection.color = noSelectedColor;
                earthSelection.color = noSelectedColor;
                airSelection.color = selectionColor; //
                voidSelection.color = noSelectedColor;
                break;
            case CrystalType.Void:
                fireSelection.color = noSelectedColor;
                waterSelection.color = noSelectedColor;
                earthSelection.color = noSelectedColor;
                airSelection.color = noSelectedColor;
                voidSelection.color = selectionColor; //
                break;
            default:
                fireSelection.color = noSelectedColor;
                waterSelection.color = noSelectedColor;
                earthSelection.color = noSelectedColor;
                airSelection.color = noSelectedColor;
                voidSelection.color = noSelectedColor;
                break;
        }
    }
}

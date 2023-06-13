using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrystalBall : MonoBehaviour {
    // What properties does a crystal ball have (including their attacks).
    private CrystalType crystalType;
    private int numberOfUsagesNormalAttack;
    private float cooldownTimeNormalAttack;
    private bool isInCooldown;
    private bool usedStrongAttack;
    private int damageNormalAttackBody;
    private int damageNormalAttackHead;
    // Define here the general settings for each crystal ball type.
    // FIRE.
    public int numberOfUsagesNormalAttackFire = 12;
    public float cooldownTimeNormalAttackFire = 0.5f;
    public int damageNormalAttackBodyFire = 10;
    public int damageNormalAttackHeadFire = 20;
    // WATER.
    public int numberOfUsagesNormalAttackWater = 20;
    public float cooldownTimeNormalAttackWater = 0.2f;
    public int damageNormalAttackBodyWater = 5;
    public int damageNormalAttackHeadWater = 10;
    // EARTH.
    public int numberOfUsagesNormalAttackEarth = 5;
    public float cooldownTimeNormalAttackEarth = 2.0f;
    public int damageNormalAttackBodyEarth = 10;
    public int damageNormalAttackHeadEarth = 10;
    // AIR.
    public int numberOfUsagesNormalAttackAir = 3;
    public float cooldownTimeNormalAttackAir = 3.0f;
    public int damageNormalAttackBodyAir = 40;
    public int damageNormalAttackHeadAir = 100;
    // VOID.
    public int numberOfUsagesNormalAttackVoid = 15;
    public float cooldownTimeNormalAttackVoid = 0.2f;
    public int damageNormalAttackBodyVoid = 12;
    public int damageNormalAttackHeadVoid = 24;

    // The constructor. When a crystal ball is generated in the inventory we need to know
    // what kind of crystal ball it is.
    public CrystalBall(CrystalType crystalType)
    {
        this.crystalType = crystalType;
        this.isInCooldown = false;
        this.usedStrongAttack = false;
        this.InitializeProperties();
    }

    // This function assign to the current object the initial values
    // depending on its crystal ball type.
    private void InitializeProperties ()
    {
        switch (this.crystalType) {
            case CrystalType.Fire:
                this.numberOfUsagesNormalAttack = numberOfUsagesNormalAttackFire;
                this.cooldownTimeNormalAttack = cooldownTimeNormalAttackFire;
                this.damageNormalAttackBody = damageNormalAttackBodyFire;
                this.damageNormalAttackHead = damageNormalAttackHeadFire;
                break;
            case CrystalType.Water:
                this.numberOfUsagesNormalAttack = numberOfUsagesNormalAttackWater;
                this.cooldownTimeNormalAttack = cooldownTimeNormalAttackWater;
                this.damageNormalAttackBody = damageNormalAttackBodyWater;
                this.damageNormalAttackHead = damageNormalAttackHeadWater;
                break;
            case CrystalType.Earth:
                this.numberOfUsagesNormalAttack = numberOfUsagesNormalAttackEarth;
                this.cooldownTimeNormalAttack = cooldownTimeNormalAttackEarth;
                this.damageNormalAttackBody = damageNormalAttackBodyEarth;
                this.damageNormalAttackHead = damageNormalAttackHeadEarth;
                break;
            case CrystalType.Air:
                this.numberOfUsagesNormalAttack = numberOfUsagesNormalAttackAir;
                this.cooldownTimeNormalAttack = cooldownTimeNormalAttackAir;
                this.damageNormalAttackBody = damageNormalAttackBodyAir;
                this.damageNormalAttackHead = damageNormalAttackHeadAir;
                break;
            case CrystalType.Void:
                this.numberOfUsagesNormalAttack = numberOfUsagesNormalAttackVoid;
                this.cooldownTimeNormalAttack = cooldownTimeNormalAttackVoid;
                this.damageNormalAttackBody = damageNormalAttackBodyVoid;
                this.damageNormalAttackHead = damageNormalAttackHeadVoid;
                break;
            default:
                Debug.Log("Unknown Crystal Ball Type in CrystalBall.cs");
                break;
        }
    }

    // This function is called when the crystal ball is equipped and the user wants to 
    // attack with a normal attack.
    public void UseCrystalBallNormalAttack ()
    {
        // Check if it is still in cooldown.
        if (this.isInCooldown == true) {
            return;
        }
        // If not, then spawn the attack.

        // Decrease the ammunition.
        this.numberOfUsagesNormalAttack--;
        // Set the cooldown time.
        this.isInCooldown = true;
        Invoke("ResetCoolDown", this.cooldownTimeNormalAttack);
    }

    //
    public void UseCrystalBallStrongAttack ()
    {
        // Do we also check here for cooldown or does this only apply to the normal attack?
        
        // Spawn the strong attack.
        
        // Then let the inventory see that the crystal ball is not usable anymore.
        this.usedStrongAttack = true;
    }

    private void ResetCoolDown () 
    {
        this.isInCooldown = false;
    }

    // This function has to be called from the inventory after every attack to check if the
    // crystal ball ran out of ammo and if the crystal ball has to be removed from the inventory.
    public bool IsStillUsable ()
    {
        // We have two cases. Either the ammo of the normal attack ran out or the strong attack was used.
        if ((this.numberOfUsagesNormalAttack <= 0) || (this.usedStrongAttack == true)) {
            return false;
        }
        else {
            // The crystal ball still has ammunition.
            return true;
        }
    }

    // Returns the current number of available ammunition.
    public int GetNumberOfAmmunition ()
    {
        return this.numberOfUsagesNormalAttack;
    }
}

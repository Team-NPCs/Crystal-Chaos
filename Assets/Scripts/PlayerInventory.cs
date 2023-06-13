using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour {
    // Maximum number of crystal balls per type.
    public int maxCrystalBallsPerType = 3; 

    private CrystalType currentEquippedCrystalType = CrystalType._NONE;
    private Dictionary<CrystalType, List<CrystalBall>> collectedCrystals = new Dictionary<CrystalType, List<CrystalBall>>();
    private Dictionary<CrystalType, bool> isInCoolDownCrystals = new Dictionary<CrystalType, bool>();

    // A function that adds the crystal ball to the inventory. If the max. number of crystal balls of a
    // specific type is already reached, the crystal ball will not be added.
    // The returned bool variable tells the crystal ball if it got picked up or not.
    // (so if not then the crystal ball does not vanish from the map)
    public bool AddCrystal(CrystalType crystalType) {
        if (this.collectedCrystals.ContainsKey(crystalType)) {
            if (this.collectedCrystals[crystalType].Count >= this.maxCrystalBallsPerType) {
                // We are full.
                Debug.Log("Reached maximum limit for crystal ball type: " + crystalType.ToString());
                return false;
            }
            else {
                // We can add it to the existing list.
                this.collectedCrystals[crystalType].Add(new CrystalBall(crystalType));
            }
        }
        else {
            // There is no crystal ball with the same type in the inventory. So create the list for it and add it.
            this.collectedCrystals.Add(crystalType, new List<CrystalBall>());
            this.collectedCrystals[crystalType].Add(new CrystalBall(crystalType));
            // Also add the cooldown time.
            this.isInCoolDownCrystals.Add(crystalType, false);
        }
        // If we did not had any crystal balls at all before, then equip the new one.
        if (this.currentEquippedCrystalType == CrystalType._NONE) {
            Debug.Log("We had no crystal balls before. Now we equipped one.");
            this.SwitchCrystalOneStep();
        }
        return true;
    }

    // The player wants to use a normal attack with the currently equipped crystal ball.
    public void UseCrystalBallNormalAttack ()
    {
        // Just to be sure.
        if (this.HasCrystalBall(this.currentEquippedCrystalType) == false) {
            Debug.Log("The players equipped crystal ball type has no entries in the inventory. (or inventory is empty.)");
            return;
        }
        // Check if the crystal ball is in cooldown.
        if (this.isInCoolDownCrystals[this.currentEquippedCrystalType] == true) {
            Debug.Log("Cannot spawn the attack since it is still in cooldown.");
            return;
        }
        // Ok we can use the crystal ball. We always use the first one in the inventory.
        this.collectedCrystals[this.currentEquippedCrystalType][0].UseCrystalBallNormalAttack();
        // Set the cooldown.
        this.isInCoolDownCrystals[this.currentEquippedCrystalType] = true;
        // Reset the cooldown in the defined number of seconds.
        // Create a lambda expression that calls the function with parameters
        System.Action LambdaFunctionCoolDownReset = () => this.ResetCooldown(this.currentEquippedCrystalType);
        // Invoke the lambda expression after a delay of the cooldown time of the crystal ball type.
        Invoke("LambdaFunctionCoolDownReset", this.collectedCrystals[this.currentEquippedCrystalType][0].cooldownTimeNormalAttack);
        // Check if the crystal ball needs to get destroyed.
        if (this.collectedCrystals[this.currentEquippedCrystalType][0].IsStillUsable() == false) {
            // Crystal balls ammunition ran out. Destroy it.
            this.collectedCrystals[this.currentEquippedCrystalType].RemoveAt(0);
            // Check if there are still crystal balls of the same type. If not move to the next one.
            if (this.HasCrystalBall(this.currentEquippedCrystalType) == false) {
                // We need the next one.
                this.SwitchCrystalOneStep();
                // The problem is that we can now also be completely out of ammunition. So if the
                // new crystal type is _NONE, we are out of ammo :-(
                if (this.currentEquippedCrystalType == CrystalType._NONE) {
                    Debug.Log("We ran completely out of ammo after this spell. There is no other crystal ball in the inventory.");
                }
                else {
                    Debug.Log("Destroyed the crystal ball since it ran out of ammo and switched to another type since the original type was not in the inventory anymore.");
                }
            }
            else {
                Debug.Log("Destroyed crystal ball since it ran out of ammo but the player has still crystal balls of the same type.");
            }
        }
    }

    // This function resets the cooldown.
    private void ResetCooldown (CrystalType crystal_type) {
        this.isInCoolDownCrystals[crystal_type] = false;
    }


    // Switch the equipped crystal ball in the forward of backward direction.
    // The forward direction is the following (backwards is as the name indicates backwards):
    // FIRE > WATER > EARTH > AIR > VOID > FIRE ... 
    public void SwitchCrystalOneStep (bool directionForward = true)
    {
        // Note that we need to check if the next crystal type is within the inventory and also if the 
        // crystal types list has entries.
        // Get the next one. Repeat it until we have an equipped crystal ball type.
        CrystalType nextCrystalType = this.GetNextCrystalType(this.currentEquippedCrystalType, directionForward);
        // Note that we do not want to run the round multiple / infinite times if we only have one equipped crystal ball.
        while (nextCrystalType != this.currentEquippedCrystalType) {
            if (this.HasCrystalBall(nextCrystalType) == true) {
                // We found the next one.
                break;
            }
            else {
                // Unfortunately this crystal ball type is not in the inventory. Get the next one in the defined direction.
                nextCrystalType = this.GetNextCrystalType(nextCrystalType, directionForward);
            }
        }
        // Now we either have a new crystal ball type or are still at the old position in the inventory.
        if ((nextCrystalType == this.currentEquippedCrystalType) && (this.HasCrystalBall(nextCrystalType) == false)) {
            // This is the case if we came from _NONE or if we removed the last crystal ball since it ran out of ammo and now
            // we can not go anywhere. Go to _NONE.
            this.currentEquippedCrystalType = CrystalType._NONE;
        }
        else if (nextCrystalType == this.currentEquippedCrystalType) {
            // We do not have other crystal balls but the currently equipped type has still crystal balls.
            Debug.Log("Cannot switch the crystal ball since the player has no other ones equipped.");
        }
        else {
            // Equip it.
            this.currentEquippedCrystalType = nextCrystalType;
        }
    }

    // A helper function for the function above.
    private CrystalType GetNextCrystalType (CrystalType crystalTypeToComeFrom, bool directionForward)
    {
        switch (crystalTypeToComeFrom) {
            case CrystalType.Fire:
                if (directionForward == true) { return CrystalType.Water; }
                else { return CrystalType.Void; }
            case CrystalType.Water:
                if (directionForward == true) { return CrystalType.Earth; }
                else { return CrystalType.Fire; }
            case CrystalType.Earth:
                if (directionForward == true) { return CrystalType.Air; }
                else { return CrystalType.Water; }
            case CrystalType.Air:
                if (directionForward == true) { return CrystalType.Void; }
                else { return CrystalType.Earth; }
            case CrystalType.Void:
                if (directionForward == true) { return CrystalType.Fire; }
                else { return CrystalType.Air; }
            default:
                Debug.Log("Unimplemented crystalTypeToComeFrom in getNextCrystalType.");
                return CrystalType._NONE;
        }
    }

    // A function that tells if a player has at least one crystal ball of the specified type.
    public bool HasCrystalBall(CrystalType crystalType) {
        return this.collectedCrystals.ContainsKey(crystalType) && this.collectedCrystals[crystalType].Count > 0;
    }

    // A function that can be used for the interface to get and visualize the number of crystal balls per type.
    public int GetNumberOfCrystalBalls(CrystalType crystalType) {
        if (this.collectedCrystals.ContainsKey(crystalType) == false) {
            return 0;
        }
        else return this.collectedCrystals[crystalType].Count;
    }

    // A function that can be used for the interface to get and visualize the number of available ammunition for 
    // the specified crystal ball type. It only looks into the first crystal ball of each type (so if there are
    // other balls with the same type, they have full ammo, this has to be requested by the function above).
    public int GetNumberOfAmmunition(CrystalType crystalType) {
        if (this.collectedCrystals.ContainsKey(crystalType) == false) {
            return 0;
        }
        else return this.collectedCrystals[crystalType][0].GetNumberOfAmmunition();
    }
}
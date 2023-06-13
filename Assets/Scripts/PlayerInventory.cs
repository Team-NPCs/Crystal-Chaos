using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour {
    // Maximum number of crystal balls per type.
    public int maxCrystalBallsPerType = 3; 

    private CrystalType currentEquippedCrystalType = CrystalType._NONE;
    private Dictionary<CrystalType, List<CrystalBall>> collectedCrystals = new Dictionary<CrystalType, List<CrystalBall>>();

    // A function that adds the crystal ball to the inventory. If the max. number of crystal balls of a
    // specific type is already reached, the crystal ball will not be added.
    // The returned bool variable tells the crystal ball if it got picked up or not.
    // (so if not then the crystal ball does not vanish from the map)
    public bool AddCrystal(CrystalType crystalType) {
        if (collectedCrystals.ContainsKey(crystalType)) {
            if (collectedCrystals[crystalType].Count >= maxCrystalBallsPerType) {
                // We are full.
                Debug.Log("Reached maximum limit for crystal ball type: " + crystalType.ToString());
                return false;
            }
            else {
                // We can add it to the existing list.
                collectedCrystals[crystalType].Add(new CrystalBall(crystalType));
                return true;
            }
        }
        else {
            // There is no crystal ball with the same type in the inventory. So create the list for it and add it.
            collectedCrystals.Add(crystalType, new List<CrystalBall>());
            collectedCrystals[crystalType].Add(new CrystalBall(crystalType));
            return true;
        }
    }

    // The player wants to use a normal attack with the currently equipped crystal ball.
    public void UseCrystalBallNormalAttack ()
    {
        // Just to be sure.
        if (this.HasCrystalBall(this.currentEquippedCrystalType) == false) {
            Debug.Log("The players equipped crystal ball type has no entries in the inventory. (or inventory is empty.)");
            return;
        }
        // Ok we can use the crystal ball. We always use the first one in the inventory.
        this.collectedCrystals[this.currentEquippedCrystalType][0].UseCrystalBallNormalAttack();
        // Check if the crystal ball needs to get destroyed.
        if (this.collectedCrystals[this.currentEquippedCrystalType][0].IsStillUsable() == false) {
            // Crystal balls ammunition ran out. Destroy it.
            this.collectedCrystals[this.currentEquippedCrystalType].RemoveAt(0);
            // Check if there are still crystal balls of the same type. If not move to the next one.
            if (this.HasCrystalBall(this.currentEquippedCrystalType) == false) {
                // We need the next one.
                this.switchCrystalOneStep();
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


    // Switch the equipped crystal ball in the forward of backward direction.
    // The forward direction is the following (backwards is as the name indicates backwards):
    // FIRE > WATER > EARTH > AIR > VOID > FIRE ... 
    public void switchCrystalOneStep (bool directionForward = true)
    {
        // Note that we need to check if the next crystal type is within the inventory and also if the 
        // crystal types list has entries.
        // Get the next one. Repeat it until we have an equipped crystal ball type.
        CrystalType nextCrystalType = getNextCrystalType(this.currentEquippedCrystalType, directionForward);
        // Note that we do not want to run the round multiple / infinite times if we only have one equipped crystal ball.
        while (nextCrystalType != this.currentEquippedCrystalType) {
            if (this.HasCrystalBall(nextCrystalType) == true) {
                // We found the next one.
                break;
            }
            else {
                // Unfortunately this crystal ball type is not in the inventory. Get the next one in the defined direction.
                nextCrystalType = getNextCrystalType(nextCrystalType, directionForward);
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
    private CrystalType getNextCrystalType (CrystalType crystalTypeToComeFrom, bool directionForward)
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
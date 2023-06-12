using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour {
    public int maxCrystalBallsPerType = 3; // Maximum number of crystal balls per type

    private Dictionary<CrystalType, int> collectedCrystals = new Dictionary<CrystalType, int>();

    // A function that adds the crystal ball to the inventory. If the max. number of crystal balls of a
    // specific type is already reached, the crystal ball will not be added.
    // The returned bool variable tells the crystal ball if it got picked up or not.
    // (so if not then the crystal ball does not vanish from the map)
    public bool AddCrystal(CrystalType crystalType) {
        if (collectedCrystals.ContainsKey(crystalType)) {
            if (collectedCrystals[crystalType] >= maxCrystalBallsPerType) {
                Debug.Log("Reached maximum limit for crystal ball type: " + crystalType.ToString());
                return false;
            }
            else {
                collectedCrystals[crystalType]++;
                return true;
            }
        }
        else {
            collectedCrystals.Add(crystalType, 1);
            return true;
        }
    }

    // A function that tells if a player has at least one crystal ball of the specified type.
    public bool HasCrystal(CrystalType crystalType) {
        return collectedCrystals.ContainsKey(crystalType) && collectedCrystals[crystalType] > 0;
    }
}
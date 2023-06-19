using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerInventory : NetworkBehaviour {
    // Maximum number of crystal balls per type.
    public int maxCrystalBallsPerType = 3; 
    public float scrollSensitivity = 0.1f;

    // Just for the record if in the future problems arise and we discuss how to implement it.
    // The dictionaries beneath cannot be used as networked variables. So we cannot network them /
    // we cannot have the inventory logic on the server.
    // Another problem is, that the owner of the spawned crystal balls is the host / server but the 
    // owner of the player stats can also be a client that is not the host. So the calls to ServerRPCs are difficult.
    /*
    private Dictionary<CrystalType, List<CrystalBall>> collectedCrystals = new Dictionary<CrystalType, List<CrystalBall>>();
    private Dictionary<CrystalType, bool> isInCoolDownCrystals = new Dictionary<CrystalType, bool>();
    */
    // So for this purpose we need multiple networked variables. But since we cannot use the dictionary directly, we need a workaround.
    [SerializeField] public NetworkVariable<int> crystalCountFire = new NetworkVariable<int>(0);
    [SerializeField] public NetworkVariable<int> crystalCountWater = new NetworkVariable<int>(0);
    [SerializeField] public NetworkVariable<int> crystalCountEarth = new NetworkVariable<int>(0);
    [SerializeField] public NetworkVariable<int> crystalCountAir = new NetworkVariable<int>(0);
    [SerializeField] public NetworkVariable<int> crystalCountVoid = new NetworkVariable<int>(0);
    // We will also have a local version of this. The dilemma we have is the following: Adding a crystal ball is done by the server
    // since the host checks for the collision. Removing a crystal ball is initiated by the client himself since he uses the crystal
    // ball and removes it if it ran out of ammo. But the client cannot change the networked variables so he has to ask the server for
    // it. But we also have some things (e.g. changing the equipped crystal ball after a crystal ball ran out of ammunition and there
    // are no other crystal balls of the same type left) that have to be done in real time and we cannot wait until the server responded.
    // So we keep a copy of these variables locally that will also be updated when the server changes the network variables but we operate
    // the inventory selecting on this local dictionary.
    // To sum up: only use this for decreasing but never for increasing.
    private Dictionary<CrystalType, int> crystalCount = new();
    // Keep track of the ammunition.
    private Dictionary<CrystalType, int> ammunitionCount = new();
    private Dictionary<CrystalType, int> maxAmmunitionCount = new();
    public int numberOfUsagesNormalAttackFire = 12;
    public int numberOfUsagesNormalAttackWater = 20;
    public int numberOfUsagesNormalAttackEarth = 5;
    public int numberOfUsagesNormalAttackAir = 3;
    public int numberOfUsagesNormalAttackVoid = 15;
    public CrystalType currentEquippedCrystalType = CrystalType._NONE;
    
    // This does not apply to the cooldown, since only the local player will shoot on the local machine so the information
    // about this is always here.
    private Dictionary<CrystalType, bool> isInCoolDownCrystals = new();
    private Dictionary<CrystalType, float> coolDownTime = new();
    public float cooldownTimeNormalAttackFire = 0.5f;
    public float cooldownTimeNormalAttackWater = 0.2f;
    public float cooldownTimeNormalAttackEarth = 2.0f;
    public float cooldownTimeNormalAttackAir = 3.0f;
    public float cooldownTimeNormalAttackVoid = 0.2f;

    [SerializeField] public InventoryUI inventoryUI;

    // What shall at the start happen? At start I want to add to my own inventory (not to that of another player)
    // a random crystal ball.
    private void Start() {
        // Get the inventory.
        inventoryUI = GameObject.FindWithTag("Inventory").GetComponent<InventoryUI>();
        // Initialize the dictionary that holds information about the number of crystal balls (local copy of the networked variables).
        crystalCount.Add(CrystalType.Fire, 0);
        crystalCount.Add(CrystalType.Water, 0);
        crystalCount.Add(CrystalType.Earth, 0);
        crystalCount.Add(CrystalType.Air, 0);
        crystalCount.Add(CrystalType.Void, 0);
        // Initialize the dictionary that holds information about the current ammuntion.
        ammunitionCount.Add(CrystalType.Fire, 0);
        ammunitionCount.Add(CrystalType.Water, 0);
        ammunitionCount.Add(CrystalType.Earth, 0);
        ammunitionCount.Add(CrystalType.Air, 0);
        ammunitionCount.Add(CrystalType.Void, 0);
        // Intialize the dictionary that holds information about the max ammunition for each crystal type.
        maxAmmunitionCount.Add(CrystalType.Fire, numberOfUsagesNormalAttackFire);
        maxAmmunitionCount.Add(CrystalType.Water, numberOfUsagesNormalAttackWater);
        maxAmmunitionCount.Add(CrystalType.Earth, numberOfUsagesNormalAttackEarth);
        maxAmmunitionCount.Add(CrystalType.Air, numberOfUsagesNormalAttackAir);
        maxAmmunitionCount.Add(CrystalType.Void, numberOfUsagesNormalAttackVoid);
        // Initialize the is in cooldown dictionary.
        isInCoolDownCrystals.Add(CrystalType.Fire, false);
        isInCoolDownCrystals.Add(CrystalType.Water, false);
        isInCoolDownCrystals.Add(CrystalType.Earth, false);
        isInCoolDownCrystals.Add(CrystalType.Air, false);
        isInCoolDownCrystals.Add(CrystalType.Void, false);
        // Initialize the cooldown time dictionary.
        coolDownTime.Add(CrystalType.Fire, cooldownTimeNormalAttackFire);
        coolDownTime.Add(CrystalType.Water, cooldownTimeNormalAttackWater);
        coolDownTime.Add(CrystalType.Earth, cooldownTimeNormalAttackEarth);
        coolDownTime.Add(CrystalType.Air, cooldownTimeNormalAttackAir);
        coolDownTime.Add(CrystalType.Void, cooldownTimeNormalAttackVoid);
        // We need eventlisteners for the change of the number of crystal balls because they are given by the 
        // host / the server that handles the collision detection.
        crystalCountFire.OnValueChanged += receivedUpdateCrystalCountFire;
        crystalCountWater.OnValueChanged += receivedUpdateCrystalCountWater;
        crystalCountEarth.OnValueChanged += receivedUpdateCrystalCountEarth;
        crystalCountAir.OnValueChanged += receivedUpdateCrystalCountAir;
        crystalCountVoid.OnValueChanged += receivedUpdateCrystalCountVoid;

        // Only the host can call a server rpc so the host will add the crystal ball at the start.
        if (IsLocalPlayer() == true) {
            // At start we assign a random crystal ball.
            CrystalType crystalType = CrystalBallSpawn.GenerateCrystalType();
            // Add it to the inventory. This will not happen directly but over the server.
            // The server will get the information that the number of crystal balls increases (so the server can modify
            // the networkvariables) and then the server will tell the client to add the crystal ball.
            Debug.Log("Try to add " + crystalType.ToString());
            // The owner of this is the local player, so he can call the server RPC.
            AddCrystalServerRpc(crystalType);
        }
    }

    private void Update() {
        // Input only applies to the local players inventory.
        if (IsLocalPlayer()) {
            // Check if the mouse scroll was used. Change the crystal ball based on 
            float scrollInput = Input.GetAxis("Mouse ScrollWheel");
            if (scrollInput > scrollSensitivity)
            {
                // Mouse scroll wheel scrolled up.
                Debug.Log("Scrolled Up");
                SwitchCrystalOneStep(false);
            }
            else if (scrollInput < 0f)
            {
                // Mouse scroll wheel scrolled down.
                Debug.Log("Scrolled Down");
                SwitchCrystalOneStep();
            }
            // Check if the number keys were pressed. 1 for fire, 2 for water, 3 for earth, 4 for air, 5 for void.
            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) {
                Debug.Log("Pressed Key 1.");
                SwitchCrystalTo(CrystalType.Fire);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) {
                Debug.Log("Pressed Key 2.");
                SwitchCrystalTo(CrystalType.Water);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3)) {
                Debug.Log("Pressed Key 3.");
                SwitchCrystalTo(CrystalType.Earth);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4)) {
                Debug.Log("Pressed Key 4.");
                SwitchCrystalTo(CrystalType.Air);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.Keypad5)) {
                Debug.Log("Pressed Key 5.");
                SwitchCrystalTo(CrystalType.Void);
            }
        }
    }
    
    // A function that checks if the crystal ball can be added to the inventory. 
    // The variables it uses for this are all networked, so the server will always know if the 
    // new crystal ball can be added or not.
    public bool CanAddCrystal(CrystalType crystalType) {
        if (crystalType == CrystalType.Fire) {
            return crystalCountFire.Value < maxCrystalBallsPerType;
        }
        if (crystalType == CrystalType.Water) {
            return crystalCountWater.Value < maxCrystalBallsPerType;
        }
        if (crystalType == CrystalType.Earth) {
            return crystalCountEarth.Value < maxCrystalBallsPerType;
        }
        if (crystalType == CrystalType.Air) {
            return crystalCountAir.Value < maxCrystalBallsPerType;
        }
        if (crystalType == CrystalType.Void) {
            return crystalCountVoid.Value < maxCrystalBallsPerType;
        }
        else {
            Debug.Log("Tried to add an unimplemented crystal type: " + crystalType.ToString());
            return false;
        }
    }

    // A function that adds the crystal ball to the inventory.
    // The check, whether it can be added or not has to be done before since the 
    // serverRpc function cannot return a value which we could use for determining if a 
    // crystal ball has been picked up or not.
    [ServerRpc]
    public void AddCrystalServerRpc(CrystalType crystalType) {
        // Keep track of the number of equipped crystal balls.
        if (crystalType == CrystalType.Fire) {
            crystalCountFire.Value++;
        }
        else if (crystalType == CrystalType.Water) {
            crystalCountWater.Value++;
        }
        else if (crystalType == CrystalType.Earth) {
            crystalCountEarth.Value++;
        }
        else if (crystalType == CrystalType.Air) {
            crystalCountAir.Value++;
        }
        else if (crystalType == CrystalType.Void) {
            crystalCountVoid.Value++;
        }
    }

    // A function that adds a random crystal ball to the inventory.
    public void AddRandomCrystal() {
        AddCrystal(CrystalBallSpawn.GenerateCrystalType());
    }


    public bool AddCrystal(CrystalType crystalType) {
        // Check if the crystal ball can be added.
        if (CanAddCrystal(crystalType) == false) {
            return false;
        }
        // Now add the crystal ball.
        if (crystalType == CrystalType.Fire) {
            crystalCountFire.Value++;
        }
        else if (crystalType == CrystalType.Water) {
            crystalCountWater.Value++;
        }
        else if (crystalType == CrystalType.Earth) {
            crystalCountEarth.Value++;
        }
        else if (crystalType == CrystalType.Air) {
            crystalCountAir.Value++;
        }
        else if (crystalType == CrystalType.Void) {
            crystalCountVoid.Value++;
        }
        if (IsLocalPlayer()) {
            inventoryUI.AdjustInventory(crystalType, GetNumberOfAmmunition(crystalType), GetNumberOfCrystalBalls(crystalType));
        }
        return true;
    }

    // We need the same logic for removing an crystal ball after it got no ammunition anymore.
    [ServerRpc]
    public void RemoveCrystalServerRpc(CrystalType crystalType) {
        // Keep track of the number of equipped crystal balls.
        if (crystalType == CrystalType.Fire) {
            crystalCountFire.Value--;
        }
        else if (crystalType == CrystalType.Water) {
            crystalCountWater.Value--;
        }
        else if (crystalType == CrystalType.Earth) {
            crystalCountEarth.Value--;
        }
        else if (crystalType == CrystalType.Air) {
            crystalCountAir.Value--;
        }
        else if (crystalType == CrystalType.Void) {
            crystalCountVoid.Value--;
        }
        // Just for debug reasons.
        if ((crystalCountFire.Value < 0) || (crystalCountWater.Value < 0) || (crystalCountEarth.Value < 0) || 
            (crystalCountAir.Value < 0) || (crystalCountVoid.Value < 0)) {
                Debug.Log("Error syncing the numbers of crystal balls. A value is below zero.");
        }
    }

    // This sets all crystals to zero. It is used when a player dies.
    public void ResetCrystal() {
        crystalCountFire.Value = 0;
        crystalCountWater.Value = 0;
        crystalCountEarth.Value = 0;
        crystalCountAir.Value = 0;
        crystalCountVoid.Value = 0;
    }

    // The player wants to use a normal attack with the currently equipped crystal ball.
    // This function runs on the local client. 
    public bool UseCrystalBallNormalAttack ()
    {
        if (currentEquippedCrystalType == CrystalType._NONE) {
            // If no crystal ball is equipped, it means there is not a single one in the inventory.
            return false;
        }
        // Just to be sure.
        if (HasCrystalBall(currentEquippedCrystalType) == false) {
            Debug.Log("The players equipped crystal ball type has no entries in the inventory. (or inventory is empty.)");
            return false;
        }
        // Check if the crystal ball is in cooldown.
        if (isInCoolDownCrystals[currentEquippedCrystalType] == true) {
            Debug.Log("Cannot spawn the attack since it is still in cooldown.");
            return false;
        }
        // We can use it. Therefore decrease the number of available spells.
        ammunitionCount[currentEquippedCrystalType]--;
        // Set the cooldown.
        isInCoolDownCrystals[currentEquippedCrystalType] = true;
        // Reset the cooldown in the defined number of seconds.
        switch (currentEquippedCrystalType) {
            case CrystalType.Fire:
                Invoke(nameof(ResetCoolDownFire), coolDownTime[CrystalType.Fire]);
                break;
            case CrystalType.Water:
                Invoke(nameof(ResetCoolDownWater), coolDownTime[CrystalType.Water]);
                break;
            case CrystalType.Earth:
                Invoke(nameof(ResetCoolDownEarth), coolDownTime[CrystalType.Earth]);
                break;
            case CrystalType.Air:
                Invoke(nameof(ResetCoolDownAir), coolDownTime[CrystalType.Air]);
                break;
            case CrystalType.Void:
                Invoke(nameof(ResetCoolDownVoid), coolDownTime[CrystalType.Void]);
                break;
            default:
                Debug.Log("Unsupported crystal type in UseNormalAttack.");
                break;
        }
        // Check if the crystal ball needs to get destroyed.
        // Save what crystal type we used for the attack since the currentEquippedCrystalType can
        // change if the crystal ball got deleted.
        CrystalType usedCrystalType = currentEquippedCrystalType;
        if (ammunitionCount[currentEquippedCrystalType] <= 0) {
            Debug.Log("CrystalBall ran out of ammo.");
            // Crystal balls ammunition ran out. Destroy it.
            // Do we have another one?
            bool hasAnotherOne;
            if (currentEquippedCrystalType == CrystalType.Fire) {
                hasAnotherOne = crystalCountFire.Value > 1;
            }
            else if (currentEquippedCrystalType == CrystalType.Water) {
                hasAnotherOne = crystalCountWater.Value > 1;
            }
            else if (currentEquippedCrystalType == CrystalType.Earth) {
                hasAnotherOne = crystalCountEarth.Value > 1;
            }
            else if (currentEquippedCrystalType == CrystalType.Air) {
                hasAnotherOne = crystalCountAir.Value > 1;
            }
            else if (currentEquippedCrystalType == CrystalType.Void) {
                hasAnotherOne = crystalCountVoid.Value > 1;
            }
            else {
                Debug.Log("Unsupported crystal type in UseNormalAttack.");
                hasAnotherOne = false;
            }
            if (hasAnotherOne == true) {
                // We can reset the number of ammunition.
                ammunitionCount[currentEquippedCrystalType] = maxAmmunitionCount[currentEquippedCrystalType];
            }
            // Remove it yourself.
            crystalCount[currentEquippedCrystalType]--;
            // Tell the server to delete the destroyed crystal.
            RemoveCrystalServerRpc(currentEquippedCrystalType);
            // Check if there are still crystal balls of the same type. If not move to the next one.
            // But only on the local player.
            if (hasAnotherOne == false) {
                // We need the next one.
                SwitchCrystalOneStep();
                // The problem is that we can now also be completely out of ammunition. So if the
                // new crystal type is _NONE, we are out of ammo :-(
                if (currentEquippedCrystalType == CrystalType._NONE) {
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
        // Update the number of ammunition and number of crystal balls for the used one.
        if (IsLocalPlayer()) {
            inventoryUI.AdjustInventory(usedCrystalType, GetNumberOfAmmunition(usedCrystalType), GetNumberOfCrystalBalls(usedCrystalType));
        }
        return true;
    }

    // This function resets the cooldown.
    private void ResetCooldown (CrystalType crystal_type) {
        isInCoolDownCrystals[crystal_type] = false;
    }
    // We need some helper functions since the invoke call does not allow functions with parameters.
    // Unfortunatly lambda functions did not work.
    private void ResetCoolDownFire () {
        isInCoolDownCrystals[CrystalType.Fire] = false;
    }
    private void ResetCoolDownWater () {
        isInCoolDownCrystals[CrystalType.Water] = false;
    }
    private void ResetCoolDownEarth () {
        isInCoolDownCrystals[CrystalType.Earth] = false;
    }
    private void ResetCoolDownAir () {
        isInCoolDownCrystals[CrystalType.Air] = false;
    }
    private void ResetCoolDownVoid () {
        isInCoolDownCrystals[CrystalType.Void] = false;
    }


    // Switch the equipped crystal ball in the forward of backward direction.
    // The forward direction is the following (backwards is as the name indicates backwards):
    // FIRE > WATER > EARTH > AIR > VOID > FIRE ... 
    public void SwitchCrystalOneStep (bool directionForward = true)
    {
        // Note that we need to check if the next crystal type is within the inventory and also if the 
        // crystal types list has entries.
        // Get the next one. Repeat it until we have an equipped crystal ball type.
        CrystalType nextCrystalType = GetNextCrystalType(currentEquippedCrystalType, directionForward);
        // Note that we do not want to run the round multiple / infinite times if we only have one equipped crystal ball.
        while (nextCrystalType != currentEquippedCrystalType) {
            if (HasCrystalBall(nextCrystalType) == true) {
                // We found the next one.
                break;
            }
            else {
                // Unfortunately this crystal ball type is not in the inventory. Get the next one in the defined direction.
                nextCrystalType = GetNextCrystalType(nextCrystalType, directionForward);
            }
        }
        // Now we either have a new crystal ball type or are still at the old position in the inventory.
        if ((nextCrystalType == currentEquippedCrystalType) && (HasCrystalBall(nextCrystalType) == false)) {
            // This is the case if we came from _NONE or if we removed the last crystal ball since it ran out of ammo and now
            // we can not go anywhere. Go to _NONE.
            currentEquippedCrystalType = CrystalType._NONE;
        }
        else if (nextCrystalType == currentEquippedCrystalType) {
            // We do not have other crystal balls but the currently equipped type has still crystal balls.
            Debug.Log("Cannot switch the crystal ball since the player has no other ones equipped.");
        }
        else {
            // Equip it.
            Debug.Log("New crystal ball type:" + nextCrystalType.ToString());
            currentEquippedCrystalType = nextCrystalType;
        }
        if (IsLocalPlayer()) {
            inventoryUI.adjustColorSelection(currentEquippedCrystalType);
        }
    }

    // This function switches the equipped crystal ball to the specified type. Note that this only will work if the 
    // type is really within the inventory. If not, the old type is kept.
    public void SwitchCrystalTo (CrystalType crystal_type) {
        if (HasCrystalBall(crystal_type)) {
            Debug.Log("Equipped crystal ball type " + crystal_type.ToString());
            currentEquippedCrystalType = crystal_type;
        }
        if (IsLocalPlayer()) {
            inventoryUI.adjustColorSelection(currentEquippedCrystalType);
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
                Debug.Log("Unimplemented OR _NONE crystalTypeToComeFrom in getNextCrystalType.");
                return CrystalType._NONE;
        }
    }

    // A function that tells if a player has at least one crystal ball of the specified type.
    // We operate on the local copy of the number of crystal balls since we call this function immediately
    // after deleting a crystal ball and we do not know if the server response already reached this client.
    public bool HasCrystalBall(CrystalType crystalType) {
        if (crystalType == CrystalType._NONE) return false;
        if (crystalCount.ContainsKey(crystalType) == true) {
            return crystalCount[crystalType] > 0;
        }
        else {
            return false;
        }
    }

    // A function that can be used for the interface to get and visualize the number of crystal balls per type.
    public int GetNumberOfCrystalBalls(CrystalType crystalType) {
        if (crystalType == CrystalType._NONE) return 0;
        if (crystalCount.ContainsKey(crystalType) == true) {
            return crystalCount[crystalType];
        }
        else {
            return 0;
        }
    }

    // A function that can be used for the interface to get and visualize the number of available ammunition for 
    // the specified crystal ball type. It only looks into the first crystal ball of each type (so if there are
    // other balls with the same type, they have full ammo, this has to be requested by the function above).
    public int GetNumberOfAmmunition(CrystalType crystalType) {
        if (crystalType == CrystalType._NONE) return 0;
        if (ammunitionCount.ContainsKey(crystalType) == true) {
            return ammunitionCount[crystalType];
        }
        else {
            return 0;
        }
    }

    // Check if the player that this script is assigned to is the local player.
    // The thing is, that we have multiple players at the end within the game. Each player has the 
    // playerStats and it does not matter what playerstats is changed, the bar will change if we do
    // not check which stats were changed. So only visualize the health of the own player / local player.
    private bool IsLocalPlayer()
    {
        // Replace this with your own logic to determine if this instance is the local player
        // For example, you can compare the NetworkClientId with the local client's NetworkClientId
        return NetworkManager.Singleton.LocalClientId == GetComponent<NetworkObject>().OwnerClientId;
    }


    // Event listeners for the change of the networked crystal count variables. We need a function for each
    // networked variable, but they are basically the same (only changing the crystal type).
    // Note that the removing of a crystal ball is already known by the player since he himself initiated the
    // removal and therefore this is already displayed in the local copy of the crystal count. We only need to add.
    // FIRE.
    private void receivedUpdateCrystalCountFire(int oldValue, int newValue) {
        // Did the number decrease?
        if (newValue < crystalCount[CrystalType.Fire]) {
            // This was in the most cases already handled by the local client using the useAttadck method.
            // But this can also happen when the server takes away crystal balls e.g., when the player dies.
            if (newValue == 0) {
                // Take away everything.
                crystalCount[CrystalType.Fire] = 0;
                ammunitionCount[CrystalType.Fire] = 0;
                // If this crystal ball was selected before, check for the next one.
                if (currentEquippedCrystalType == CrystalType.Fire) {
                    SwitchCrystalOneStep();
                }
            }
            else {
                // Only one crystal ball was taken away. Set the new amount of crystal balls and the ammunition to max.
                crystalCount[CrystalType.Fire] = newValue;
                ammunitionCount[CrystalType.Fire] = maxAmmunitionCount[CrystalType.Fire];
            }
        }
        // Did the number increase? Do not check against the oldValue but the local one.
        else if (newValue > crystalCount[CrystalType.Fire]) {
            // The number of crystal balls increased. Save it to the local copy. If we currently have no crystal ball
            // equipped, it means that this is the only one we have, so switch to it.
            crystalCount[CrystalType.Fire] = newValue;
            if (currentEquippedCrystalType == CrystalType._NONE) {
                SwitchCrystalTo(CrystalType.Fire);
            }
            // If the new value is now one, it means, that the ammunition of the this type was before at zero.
            // So reset this too. If not, the ammunition refers to another crystal ball of the same type in the inventory.
            if (newValue == 1) {
                ammunitionCount[CrystalType.Fire] = maxAmmunitionCount[CrystalType.Fire];
            }
        }
        else {
            // The new value is the same as the local one. This happens when the crystal balls get removed after the shooting
            // function. In this case we have to do nothing, not even update the inventory ui since this happened already locally.
            return;
        }
        // Update the inventory UI.
        if (IsLocalPlayer()) {
            inventoryUI.AdjustInventory(CrystalType.Fire, GetNumberOfAmmunition(CrystalType.Fire), GetNumberOfCrystalBalls(CrystalType.Fire));
        }
    }
    // WATER.
    private void receivedUpdateCrystalCountWater(int oldValue, int newValue) {
        // Did the number decrease?
        if (newValue < crystalCount[CrystalType.Water]) {
            // This was in the most cases already handled by the local client using the useAttadck method.
            // But this can also happen when the server takes away crystal balls e.g., when the player dies.
            if (newValue == 0) {
                // Take away everything.
                crystalCount[CrystalType.Water] = 0;
                ammunitionCount[CrystalType.Water] = 0;
                // If this crystal ball was selected before, check for the next one.
                if (currentEquippedCrystalType == CrystalType.Water) {
                    SwitchCrystalOneStep();
                }
            }
            else {
                // Only one crystal ball was taken away. Set the new amount of crystal balls and the ammunition to max.
                crystalCount[CrystalType.Water] = newValue;
                ammunitionCount[CrystalType.Water] = maxAmmunitionCount[CrystalType.Water];
            }
        }
        // Did the number increase? Do not check against the oldValue but the local one.
        else if (newValue > crystalCount[CrystalType.Water]) {
            // The number of crystal balls increased. Save it to the local copy. If we currently have no crystal ball
            // equipped, it means that this is the only one we have, so switch to it.
            crystalCount[CrystalType.Water] = newValue;
            if (currentEquippedCrystalType == CrystalType._NONE) {
                SwitchCrystalTo(CrystalType.Water);
            }
            // If the new value is now one, it means, that the ammunition of the this type was before at zero.
            // So reset this too. If not, the ammunition refers to another crystal ball of the same type in the inventory.
            if (newValue == 1) {
                ammunitionCount[CrystalType.Water] = maxAmmunitionCount[CrystalType.Water];
            }
        }
        else {
            // The new value is the same as the local one. This happens when the crystal balls get removed after the shooting
            // function. In this case we have to do nothing, not even update the inventory ui since this happened already locally.
            return;
        }
        // Update the inventory UI.
        if (IsLocalPlayer()) {
            inventoryUI.AdjustInventory(CrystalType.Water, GetNumberOfAmmunition(CrystalType.Water), GetNumberOfCrystalBalls(CrystalType.Water));
        }
    }
    // EARTH.
    private void receivedUpdateCrystalCountEarth(int oldValue, int newValue) {
        // Did the number decrease?
        if (newValue < crystalCount[CrystalType.Earth]) {
            // This was in the most cases already handled by the local client using the useAttadck method.
            // But this can also happen when the server takes away crystal balls e.g., when the player dies.
            if (newValue == 0) {
                // Take away everything.
                crystalCount[CrystalType.Earth] = 0;
                ammunitionCount[CrystalType.Earth] = 0;
                // If this crystal ball was selected before, check for the next one.
                if (currentEquippedCrystalType == CrystalType.Earth) {
                    SwitchCrystalOneStep();
                }
            }
            else {
                // Only one crystal ball was taken away. Set the new amount of crystal balls and the ammunition to max.
                crystalCount[CrystalType.Earth] = newValue;
                ammunitionCount[CrystalType.Earth] = maxAmmunitionCount[CrystalType.Earth];
            }
        }
        // Did the number increase? Do not check against the oldValue but the local one.
        else if (newValue > crystalCount[CrystalType.Earth]) {
            // The number of crystal balls increased. Save it to the local copy. If we currently have no crystal ball
            // equipped, it means that this is the only one we have, so switch to it.
            crystalCount[CrystalType.Earth] = newValue;
            if (currentEquippedCrystalType == CrystalType._NONE) {
                SwitchCrystalTo(CrystalType.Earth);
            }
            // If the new value is now one, it means, that the ammunition of the this type was before at zero.
            // So reset this too. If not, the ammunition refers to another crystal ball of the same type in the inventory.
            if (newValue == 1) {
                ammunitionCount[CrystalType.Earth] = maxAmmunitionCount[CrystalType.Earth];
            }
        }
        else {
            // The new value is the same as the local one. This happens when the crystal balls get removed after the shooting
            // function. In this case we have to do nothing, not even update the inventory ui since this happened already locally.
            return;
        }
        // Update the inventory UI.
        if (IsLocalPlayer()) {
            inventoryUI.AdjustInventory(CrystalType.Earth, GetNumberOfAmmunition(CrystalType.Earth), GetNumberOfCrystalBalls(CrystalType.Earth));
        }
    }
    // AIR.
    private void receivedUpdateCrystalCountAir(int oldValue, int newValue) {
        // Did the number decrease?
        if (newValue < crystalCount[CrystalType.Air]) {
            // This was in the most cases already handled by the local client using the useAttadck method.
            // But this can also happen when the server takes away crystal balls e.g., when the player dies.
            if (newValue == 0) {
                // Take away everything.
                crystalCount[CrystalType.Air] = 0;
                ammunitionCount[CrystalType.Air] = 0;
                // If this crystal ball was selected before, check for the next one.
                if (currentEquippedCrystalType == CrystalType.Air) {
                    SwitchCrystalOneStep();
                }
            }
            else {
                // Only one crystal ball was taken away. Set the new amount of crystal balls and the ammunition to max.
                crystalCount[CrystalType.Air] = newValue;
                ammunitionCount[CrystalType.Air] = maxAmmunitionCount[CrystalType.Air];
            }
        }
        // Did the number increase? Do not check against the oldValue but the local one.
        else if (newValue > crystalCount[CrystalType.Air]) {
            // The number of crystal balls increased. Save it to the local copy. If we currently have no crystal ball
            // equipped, it means that this is the only one we have, so switch to it.
            crystalCount[CrystalType.Air] = newValue;
            if (currentEquippedCrystalType == CrystalType._NONE) {
                SwitchCrystalTo(CrystalType.Air);
            }
            // If the new value is now one, it means, that the ammunition of the this type was before at zero.
            // So reset this too. If not, the ammunition refers to another crystal ball of the same type in the inventory.
            if (newValue == 1) {
                ammunitionCount[CrystalType.Air] = maxAmmunitionCount[CrystalType.Air];
            }
        }
        else {
            // The new value is the same as the local one. This happens when the crystal balls get removed after the shooting
            // function. In this case we have to do nothing, not even update the inventory ui since this happened already locally.
            return;
        }
        // Update the inventory UI.
        if (IsLocalPlayer()) {
            inventoryUI.AdjustInventory(CrystalType.Air, GetNumberOfAmmunition(CrystalType.Air), GetNumberOfCrystalBalls(CrystalType.Air));
        }
    }
    // VOID.
    private void receivedUpdateCrystalCountVoid(int oldValue, int newValue) {
        // Did the number decrease?
        if (newValue < crystalCount[CrystalType.Void]) {
            // This was in the most cases already handled by the local client using the useAttadck method.
            // But this can also happen when the server takes away crystal balls e.g., when the player dies.
            if (newValue == 0) {
                // Take away everything.
                crystalCount[CrystalType.Void] = 0;
                ammunitionCount[CrystalType.Void] = 0;
                // If this crystal ball was selected before, check for the next one.
                if (currentEquippedCrystalType == CrystalType.Void) {
                    SwitchCrystalOneStep();
                }
            }
            else {
                // Only one crystal ball was taken away. Set the new amount of crystal balls and the ammunition to max.
                crystalCount[CrystalType.Void] = newValue;
                ammunitionCount[CrystalType.Void] = maxAmmunitionCount[CrystalType.Void];
            }
        }
        // Did the number increase? Do not check against the oldValue but the local one.
        else if (newValue > crystalCount[CrystalType.Void]) {
            // The number of crystal balls increased. Save it to the local copy. If we currently have no crystal ball
            // equipped, it means that this is the only one we have, so switch to it.
            crystalCount[CrystalType.Void] = newValue;
            if (currentEquippedCrystalType == CrystalType._NONE) {
                SwitchCrystalTo(CrystalType.Void);
            }
            // If the new value is now one, it means, that the ammunition of the this type was before at zero.
            // So reset this too. If not, the ammunition refers to another crystal ball of the same type in the inventory.
            if (newValue == 1) {
                ammunitionCount[CrystalType.Void] = maxAmmunitionCount[CrystalType.Void];
            }
        }
        else {
            // The new value is the same as the local one. This happens when the crystal balls get removed after the shooting
            // function. In this case we have to do nothing, not even update the inventory ui since this happened already locally.
            return;
        }
        // Update the inventory UI.
        if (IsLocalPlayer()) {
            inventoryUI.AdjustInventory(CrystalType.Void, GetNumberOfAmmunition(CrystalType.Void), GetNumberOfCrystalBalls(CrystalType.Void));
        }
    }
}
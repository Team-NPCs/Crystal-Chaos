using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Shooting : NetworkBehaviour {

    private Camera mainCamera;
    private Vector3 mousePosition;
    private PlayerInventory playerInventory;
    public GameObject bullet;
    public Transform bulletTransform;
    public float distanceBulletPointToPlayer = 1.2f;

    // For the different crystal ball attacks we need different properties. 
    // This include: the damage this spell does, the speed the spell has and also if it should
    // get destroyed after a specific time (e.g. the earth shards do not have the big reachability).
    // Set this here. This will then be initiated into a dictionary in the Start() function.
    // Damage on the body of the normal attack.
    public int spellDamageNormalAttackBodyFire = 10;
    public int spellDamageNormalAttackBodyWater = 5;
    public int spellDamageNormalAttackBodyEarth = 10;
    public int spellDamageNormalAttackBodyAir = 40;
    public int spellDamageNormalAttackBodyVoid = 12;
    private Dictionary<CrystalType, int> spellDamageNormalAttackBody = new();
    // Damage on the head of the normal attack.
    public int spellDamageNormalAttackHeadFire = 20;
    public int spellDamageNormalAttackHeadWater = 10;
    public int spellDamageNormalAttackHeadEarth = 10;
    public int spellDamageNormalAttackHeadAir = 100;
    public int spellDamageNormalAttackHeadVoid = 24;
    private Dictionary<CrystalType, int> spellDamageNormalAttackHead = new();
    // Speed.
    public int spellSpeedNormalAttackFire = 30;
    public int spellSpeedNormalAttackWater = 30;
    public int spellSpeedNormalAttackEarth = 30;
    public int spellSpeedNormalAttackAir = 40;
    public int spellSpeedNormalAttackVoid = 30;
    private Dictionary<CrystalType, int> spellSpeeds = new();
    // Further settings.
    // The normal attack of the earth crystal is like a shotgun. It spawns mulitple shards that do not have
    // a long lifetime (they get erased after a specific distance / or a specific time). Define the settings here.
    public float spellLifetimeNormalAttackEarth = 0.5f;
    public int numberOfShardsNormalAttackEarth = 10;
    public float inaccuracyAngleNormalAttackEarth = 25.0f;

    // Start is called before the first frame update
    void Start() {
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        playerInventory = GetComponent<PlayerInventory>();
        // Initiate the dictionaries.
        // Damage on the body of the normal attack.
        spellDamageNormalAttackBody.Add(CrystalType.Fire, spellDamageNormalAttackBodyFire);
        spellDamageNormalAttackBody.Add(CrystalType.Water, spellDamageNormalAttackBodyWater);
        spellDamageNormalAttackBody.Add(CrystalType.Earth, spellDamageNormalAttackBodyEarth);
        spellDamageNormalAttackBody.Add(CrystalType.Air, spellDamageNormalAttackBodyAir);
        spellDamageNormalAttackBody.Add(CrystalType.Void, spellDamageNormalAttackBodyVoid);
        // Damage on the head of the normal attack.
        spellDamageNormalAttackHead.Add(CrystalType.Fire, spellDamageNormalAttackHeadFire);
        spellDamageNormalAttackHead.Add(CrystalType.Water, spellDamageNormalAttackHeadWater);
        spellDamageNormalAttackHead.Add(CrystalType.Earth, spellDamageNormalAttackHeadEarth);
        spellDamageNormalAttackHead.Add(CrystalType.Air, spellDamageNormalAttackHeadAir);
        spellDamageNormalAttackHead.Add(CrystalType.Void, spellDamageNormalAttackHeadVoid);
        // Speed.
        spellSpeeds.Add(CrystalType.Fire, spellSpeedNormalAttackFire);
        spellSpeeds.Add(CrystalType.Water, spellSpeedNormalAttackWater);
        spellSpeeds.Add(CrystalType.Earth, spellSpeedNormalAttackEarth);
        spellSpeeds.Add(CrystalType.Air, spellSpeedNormalAttackAir);
        spellSpeeds.Add(CrystalType.Void, spellSpeedNormalAttackVoid);
    }

    // The update function gets called every frame. We want to do two things:
    // 1. Get the mouse position and rotate the players shooting point. 
    //    We will also use the mouse potion for the bullets direction and initial position.
    // 2. Check if the player wants to shoot. If so, check if he can (has a crystal ball?) and 
    //    if so, spawn the bullet.
    void Update() {
        // This should only be performed on the local player.
        if (NetworkManager.Singleton.LocalClientId != GetComponent<NetworkObject>().OwnerClientId) {
            return;
        }
        // Get the current mouse position.
        mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        // Get the look vector.
        Vector3 look_vector = (mousePosition - transform.position).normalized;
        float rotZ = Mathf.Atan2(look_vector.y, look_vector.x) * Mathf.Rad2Deg;

        // Here we do not face the player but we change the position of the shooting point depending
        // on the mouse position.
        bulletTransform.SetPositionAndRotation(
            transform.position + Quaternion.Euler(0, 0, rotZ) * Vector3.right * distanceBulletPointToPlayer, 
            Quaternion.Euler(0, 0, rotZ));

        // If the left mouse button was pressed, shoot.
        if (Input.GetMouseButtonDown(0) == true) {
            Debug.Log("Shoot request.");
            // Give the request to the inventory. The inventory checks if there is a crystal ball
            // or not and also if the currently equipped crystal ball is still in cooldown.
            // We need to save the to be used crystal type since the UseAttack funtion eventually sees
            // that the crystal ball runs out and then changes the currentEquippedCrystalType before we
            // can spawn the new crystal attack. So we use our own variable.
            CrystalType crystalTypeToBeUsed = playerInventory.currentEquippedCrystalType;
            if (playerInventory.UseCrystalBallNormalAttack() == true) {
                // We can shoot. The server has to initiate it.
                // Note that the earth crystals normal attack spawns multiple shards, so differ this here.
                if (crystalTypeToBeUsed == CrystalType.Earth) {
                    // Spawn multiple shots.
                    for (int i = 0; i < numberOfShardsNormalAttackEarth; i++) {
                        float inaccuracyAngle = Random.Range(-inaccuracyAngleNormalAttackEarth, inaccuracyAngleNormalAttackEarth);
                        Quaternion inaccuracyRotation = Quaternion.Euler(0, 0, inaccuracyAngle);
                        // Note that we have to add this rotation to the look vector for the flying direction but also to the
                        // rotation itself because this value tells the program how to rotate the bullets visuals so that the 
                        // bullet itself also looks like it flies into this direction and not lies "horizontally" to it.
                        SpawnBulletServerRpc(crystalTypeToBeUsed, inaccuracyRotation * look_vector, bulletTransform.position, inaccuracyRotation * bulletTransform.rotation);
                    }
                }
                else {
                    // Just spawn one shot.
                    SpawnBulletServerRpc(crystalTypeToBeUsed, look_vector, bulletTransform.position, bulletTransform.rotation);
                }
                // Note that we do not have to set the cooldown here, since the inventory is handling the cooldown.
            }
            else {
                Debug.Log("Cannot fulfill shoot request.");
            }
        }
    }

    // The server needs to spawn the bullets.
    // IMPORTANT NOTE: In order to also be able to spawn these objects on the clients, the prefab 
    // of the bullet has to be registered at the NetworkManager.
    [ServerRpc]
    private void SpawnBulletServerRpc(CrystalType crystalType, Vector3 look_vector, Vector3 position, Quaternion rotation) {
        // Create the object.
        GameObject bulletInstance = Instantiate(bullet, position, rotation);
        // Set the velocity to the rigid body.
        Vector3 bulletVelocity = look_vector * spellSpeeds[crystalType];
        bulletInstance.GetComponent<Rigidbody2D>().velocity = (Vector2)bulletVelocity;
        // Set the damage information within the bullet.
        BulletScript bulletScript = bulletInstance.GetComponent<BulletScript>();
        bulletScript.spellDamageNormalAttackBody = spellDamageNormalAttackBody[crystalType];
        bulletScript.spellDamageNormalAttackHead = spellDamageNormalAttackHead[crystalType];


        // Get the NetworkObject component of the bullet instance.
        NetworkObject networkObject = bulletInstance.GetComponent<NetworkObject>();
        // Spawn the bullet at the server / host using the NetworkObject's Spawn method.
        networkObject.Spawn();

        // Check if the bullet is of type CrystalType.Earth. If so, destroy it after a specified time.
        if (crystalType == CrystalType.Earth)
        {
            // Start a coroutine to destroy the bullet after the specified lifetime
            StartCoroutine(DestroyBulletAfterLifetime(networkObject));
        }
    }

    // If the spell has to be deleted after a specific time, this function is used for it.
    private IEnumerator DestroyBulletAfterLifetime(NetworkObject networkObject) {
        // Wait for the specified lifetime.
        yield return new WaitForSeconds(spellLifetimeNormalAttackEarth);

        // Destroy the bullet across the network. But first check if the bullet was already
        // destroyed (e.g. by colliding with the map or hitting a player).
        if (networkObject.IsSpawned) {
            networkObject.Despawn(true);
        }
    }
}

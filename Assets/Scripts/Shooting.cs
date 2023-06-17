using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Shooting : NetworkBehaviour {

    #region Properties
    private Camera mainCamera;
    private Vector3 mousePosition;
    private PlayerInventory playerInventory;
    public GameObject bullet;
    public Transform bulletTransform;
    public float distanceBulletPointToPlayer = 1.2f;
    private int bulletSpeed = 50;
    #endregion


    // Start is called before the first frame update
    void Start() {
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        playerInventory = GetComponent<PlayerInventory>();
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
            if (playerInventory.UseCrystalBallNormalAttack() == true) {
                // We can shoot. The server has to initiate it.
                SpawnBulletServerRpc(look_vector, bulletTransform.position, bulletTransform.rotation);
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
    private void SpawnBulletServerRpc(Vector3 look_vector, Vector3 position, Quaternion rotation) {
        GameObject bulletInstance = Instantiate(bullet, position, rotation);
        Vector3 bulletVelocity = look_vector * bulletSpeed;
        bulletInstance.GetComponent<Rigidbody2D>().velocity = (Vector2)bulletVelocity;

        // Get the NetworkObject component of the bullet instance.
        NetworkObject networkObject = bulletInstance.GetComponent<NetworkObject>();
        // Spawn the bullet at the server / host using the NetworkObject's Spawn method.
        networkObject.Spawn();
    }
}
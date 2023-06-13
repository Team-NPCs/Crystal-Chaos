using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooting : MonoBehaviour {

    #region Properties
    private CrystalType crystalType;
    private Camera mainCam;
    private Vector3 mousePos;
    private PlayerMovement player;
    private PlayerInventory playerInventory;
    public GameObject bullet;
    public Transform bulletTransform;
    [HideInInspector] public float bulletForce = 20;
    #endregion


    // Start is called before the first frame update
    void Start() {
        mainCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();
        playerInventory = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerInventory>();
    }

    // Update is called once per frame
    void Update() {
        // Get the current mouse position.
        mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        Vector3 rotation = mousePos - transform.position;
        float rotZ = Mathf.Atan2(rotation.y, rotation.x) * Mathf.Rad2Deg;

        // Let the player face into the direction the mouse is indicating.
        if (player.IsFacingRight) {
            transform.rotation = Quaternion.Euler(0, 0, rotZ);
        }
        else {
            transform.rotation = Quaternion.Euler(0, 0, rotZ + 180);
        }

        // If the left mouse button was pressed, shoot!
        if (Input.GetMouseButton(0) == true) {
            Debug.Log("Shoot request.");
            // Give the request to the inventory. The inventory checks if there is a crystal ball
            // or not and also if the currently equipped crystal ball is still in cooldown.
            if (playerInventory.UseCrystalBallNormalAttack() == true) {
                // We can shoot. So do it.
                Instantiate(bullet, bulletTransform.position, Quaternion.identity);
                // Note that we do not have to set the cooldown here, since the inventory is handling the cooldown.
            }
        }
    }
}

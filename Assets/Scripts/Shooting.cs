using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooting : MonoBehaviour {

    #region Properties
    private CrystalType crystalType;
    private Camera mainCam;
    private Vector3 mousePos;
    private PlayerMovement player;
    public GameObject bullet;
    public Transform bulletTransform;
    private bool canFire;
    private float timer;
    public float timeBetweenFiringFire, bulletForceFire;
    public float timeBetweenFiringAir, bulletForceAir;
    public float timeBetweenFiringEarth, bulletForceEarth;
    public float timeBetweenFiringWater, bulletForceWater;
    public float timeBetweenFiringVoid, bulletForceVoid;
    private float timeBetweenFiring = 0.25f;
    [HideInInspector] public float bulletForce;
    #endregion


    // Start is called before the first frame update
    void Start() {
        mainCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();
    }

    // Update is called once per frame
    void Update() {
        switch (crystalType) {
            case CrystalType.Fire:
                timeBetweenFiring = timeBetweenFiringFire;
                bulletForce = bulletForceFire;
                break;
            case CrystalType.Air:
                timeBetweenFiring = timeBetweenFiringAir;
                bulletForce = bulletForceAir;
                break;
            case CrystalType.Water:
                timeBetweenFiring = timeBetweenFiringWater;
                bulletForce = bulletForceWater;
                break;
            case CrystalType.Earth:
                timeBetweenFiring = timeBetweenFiringEarth;
                bulletForce = bulletForceEarth;
                break;
            case CrystalType.Void:
                timeBetweenFiring = timeBetweenFiringVoid;
                bulletForce = bulletForceVoid;
                break;

        }
        mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        Vector3 rotation = mousePos - transform.position;
        float rotZ = Mathf.Atan2(rotation.y, rotation.x) * Mathf.Rad2Deg;

        if (player.IsFacingRight) {
            transform.rotation = Quaternion.Euler(0, 0, rotZ);
        }
        else {
            transform.rotation = Quaternion.Euler(0, 0, rotZ + 180);
        }

        if (!canFire) {
            timer += Time.deltaTime;
            if (timer > timeBetweenFiring) {
                canFire = true;
                timer = 0;
            }
        }

        if (Input.GetMouseButton(0) && canFire) {
            canFire = false;
            Instantiate(bullet, bulletTransform.position, Quaternion.identity);
        }
    }
}

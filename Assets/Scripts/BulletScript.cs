using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletScript : MonoBehaviour {
    private Shooting shooting;
    private Vector3 mousePos;
    private Camera mainCam;
    private Rigidbody2D rb;
    private float force;

    private void OnCollisionEnter2D(Collision2D collision) {
        Destroy(gameObject);
    }

    private void OnCollisionStay2D(Collision2D collision) {
        Destroy(gameObject);
    }

    // Start is called before the first frame update
    void Start() {
        shooting = GameObject.FindGameObjectWithTag("ShootingPoint").GetComponent<Shooting>();
        mainCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        force = shooting.bulletForce;
        // Debug.Log(shooting.bulletForce);
        rb = GetComponent<Rigidbody2D>();

        mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        Vector3 direction = mousePos - transform.position;
        Vector3 rotation = transform.position - mousePos;
        rb.velocity = new Vector2(direction.x, direction.y).normalized * force;
        float rot = Mathf.Atan2(rotation.y, rotation.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, rot);
    }

    // Update is called once per frame
    void Update() {

    }
}

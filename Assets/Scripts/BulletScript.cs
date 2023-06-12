using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletScript : MonoBehaviour {
    private Shooting _shooting;
    private Vector3 mousePos;
    private Camera _mainCam;
    private Rigidbody2D rb;
    public float force;

    private void OnCollisionEnter2D(Collision2D collision) {
        Destroy(gameObject);
    }

    private void OnCollisionStay2D(Collision2D collision) {
        Destroy(gameObject);
    }

    // Start is called before the first frame update
    void Start() {
        _shooting = GameObject.FindGameObjectWithTag("ShootingPoint").GetComponent<Shooting>();
        _mainCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        force = _shooting.bulletForce;
        Debug.Log("Force" + force);
        rb = GetComponent<Rigidbody2D>();

        mousePos = _mainCam.ScreenToWorldPoint(Input.mousePosition);
        Vector3 direction = mousePos - transform.position;
        Vector3 rotation = transform.position - mousePos;
        rb.velocity = new Vector2(direction.x, direction.y).normalized * force;
        float rot = Mathf.Atan2(rotation.y, rotation.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, rot);
    }

}

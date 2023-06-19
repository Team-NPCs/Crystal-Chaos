using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cloud : MonoBehaviour
{
    public float minSpeed = 1f;
    public float maxSpeed = 2f;
    private float speed;
    public float xMin = -40f;
    public float xMax = 40f;

    Vector3 movementDirection;

    void Start () {
        // Calculate the movement direction based on the cloud's rotation
        movementDirection = (Quaternion.Euler(0f, 0f, -transform.eulerAngles.z) * Vector2.right).normalized;
        // The speed value will be random.
        speed = Random.Range(minSpeed, maxSpeed);
    }

    void Update () {
        // Move the cloud
        transform.Translate(speed * Time.deltaTime * movementDirection);

        // Reset the cloud position when it goes off the screen.
        if (transform.position.x > xMax)
        {
            Vector3 newPosition = transform.position;
            newPosition.x = xMin;
            transform.position = newPosition;
        }
    }
}

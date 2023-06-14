using Unity.Netcode;
using UnityEngine;

public class CameraFollow : MonoBehaviour {
    public float FollowSpeed = 2f;
    public float yOffset = 1f;
    public Transform playerTarget;

    public void Update() {
        if (playerTarget != null) {
            Vector3 newPos = new(playerTarget.position.x, playerTarget.position.y + yOffset, -10f);
            transform.position = Vector3.Slerp(transform.position, newPos, FollowSpeed * Time.deltaTime);
        }
    }
    public void setTarget(Transform target) {
        playerTarget = target;
    }
}
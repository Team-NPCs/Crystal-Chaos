using Unity.Netcode;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using UnityEngine.UIElements;

public class CameraFollow : MonoBehaviour
{
    public float FollowSpeed = 2f;
    public float yOffset = 1f;
    public Transform playerTarget;

    private void Update()
    {
        if (playerTarget != null)
        {
            Vector3 newPos = new(playerTarget.position.x, playerTarget.position.y + yOffset, -10f);
            transform.position = Vector3.Slerp(transform.position, newPos, FollowSpeed * Time.deltaTime);
        }
    }
    public void setTarget(Transform target)
    {
        playerTarget = target;
    }
}

//public class CameraFollow : MonoBehaviour
//{
//    public Transform playerTransform;
//    public int depth = -20;

//    // Update is called once per frame
//    void Update()
//    {
//        if (playerTransform != null)
//        {
//            transform.position = playerTransform.position + new Vector3(0, 10, depth);
//        }
//    }

//public void setTarget(Transform target)
//{
//    playerTransform = target;
//}
//}
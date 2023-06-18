using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GrapplingScript : NetworkBehaviour {
    private PlayerMovement player;
    [Header("Scripts Ref:")]
    public RopeScript grappleRope;

    [Header("Layers Settings:")]
    [SerializeField] private bool grappleToAll = false;
    [SerializeField] private int grappableLayerNumber = 9;

    [Header("Main Camera:")]
    public Camera m_camera;

    [Header("Transform Ref:")]
    public Transform gunHolder;
    public Transform gunPivot;
    public Transform firePoint;
    private readonly float distanceFirePointToPlayer = 1.0f;

    [Header("Physics Ref:")]
    public SpringJoint2D m_springJoint2D;
    public Rigidbody2D m_rigidbody;

    [Header("Rotation:")]
    [SerializeField] private bool rotateOverTime = true;
    [Range(0, 60)][SerializeField] private float rotationSpeed = 4;

    [Header("Distance:")]
    [SerializeField] private bool hasMaxDistance = false;
    [SerializeField] private float maxDistnace = 20;

    private enum LaunchType {
        Transform_Launch,
        Physics_Launch
    }

    [Header("Launching:")]
    [SerializeField] private bool launchToPoint = true;
    [SerializeField] private LaunchType launchType = LaunchType.Physics_Launch;
    [SerializeField] private float launchSpeed = 1;

    [Header("No Launch To Point")]
    [SerializeField] private bool autoConfigureDistance = false;
    [SerializeField] private float targetDistance = 3;
    [SerializeField] private float targetFrequncy = 1;

    [HideInInspector] public Vector2 grappleDistanceVector;

    // Our networked variable for enabling the grappling.
    [HideInInspector] public NetworkVariable<bool> grapplingEnabled = new(false);
    // Our networked variable for the target position.
    [HideInInspector] public Vector2 grapplePoint;
    [HideInInspector] public NetworkVariable<Vector2> grapplePointNetworked = new();

    private void Awake() {
        m_camera = FindObjectOfType<Camera>();
    }

    private void Start() {
        player = GetComponent<PlayerMovement>();
        // No rope at the start.
        grappleRope.enabled = false;
        // No force to the player because of the rope at the start.
        m_springJoint2D.enabled = false;
        // Our callback for a value change.
        grapplingEnabled.OnValueChanged += grapplingEnabledValueChange;
    }

    private void Update() {
        // We have to keep in mind that this function is called depending on the number of clients.
        // What is the local client allowed to do?
        // - activating and deactivating the rope
        // - getting forces from the rope that apply to his position
        // - to draw the rope
        // What is the non-local client allowed to do?
        // - to draw the rope (this is the rope of the other player but we want to see it)
        // So what does the non-local client needs to know about this client?
        // - if the rope is enabled
        // - the position of the player (already synced due to the network transform of the player)
        // - the position where the grappling hook is attached 

        // The following code does activate / deactivate / apply forces to the player / set the reference point of the rope.
        // We need to network two variables: the enable-bool and the target vec2.
        // We do not need to network the position of the rope on the players side (so where the reference point is) because
        // we will make this easier: in the rope script we will check whether its the local player or not. If its not the local
        // player we will simply use the players position so the rope starts in the middle of the player.
        if (IsLocalPlayer()) {
            // If the mouse key goes down, activate the grapple rope.
            // But only if the desired grapple point is in reachable distance.
            if (Input.GetKeyDown(KeyCode.Mouse1)) {
                bool result = SetGrapplePoint();
                if (result == true) {
                    // Always set the grapple point first before visualizing. 
                    // The time difference between these server updates could introduce artifacts.
                    SetGrapplePointServerRpc(grapplePoint);
                    // The grapple rope can be activated.
                    grappleRope.enabled = true;
                    SetGrapplingEnabledServerRpc(true);
                }
            }
            // If the mouse key goes up again, deactivate the grapple rope.
            else if (Input.GetKeyUp(KeyCode.Mouse1)) {
                // Deactivate the rope.
                grappleRope.enabled = false;
                SetGrapplingEnabledServerRpc(false);
                // Deactivate the forces / velocities the rope introduces onto the player.
                // These values do not need to be networked since they operate on the players 
                // position and the position is already networked. We only need to network the rope.
                m_springJoint2D.enabled = false;
                m_rigidbody.gravityScale = 1;
            }
            // If the mouse is still pressed, we need to update the position of the player based on the distance
            // and the desired launch speed.
            // Note that this if statement does NOT update the ropes line. It just updates the position of the player.
            else if (Input.GetKey(KeyCode.Mouse1)) {
                if (launchToPoint && grappleRope.isGrappling) {
                    if (launchType == LaunchType.Transform_Launch) {
                        Vector2 firePointDistance = firePoint.position - gunHolder.localPosition;
                        Vector2 targetPos = grapplePoint - firePointDistance;
                        gunHolder.position = Vector2.Lerp(gunHolder.position, targetPos, Time.deltaTime * launchSpeed);
                    }
                }
            }
            // In each case we need to rotate the grapple point (the green one next to the player so that the position
            // either matches the mouse direction if no grappling is active or the rope direction if the grappling is active).
            if (grappleRope.enabled) {
                // Grappling is enabled, so the direction is given by the grapple point.
                RotateGun(grapplePoint, false);
            }
            else {
                // Grappling is currently not enabled, so the green dot looks into the direction of the mouse.
                // Therefore get the mouse position.
                Vector3 mousePos = m_camera.ScreenToWorldPoint(Input.mousePosition);
                RotateGun(mousePos, true);
            }
        }
    }

    // This function rotates the little firepoint next to the player depending on the lookPoint.
    // The lookpoint can ever be in the direction of the mouse position (if grappling is currently not enabled)
    // or into the direction of the point where the grappling hook is attached to (if grappling is currently enabled).
    void RotateGun(Vector3 lookPoint, bool allowRotationOverTime) {
        Vector3 distanceVector = lookPoint - gunPivot.position;

        float angle = Mathf.Atan2(distanceVector.y, distanceVector.x) * Mathf.Rad2Deg;
        if (rotateOverTime && allowRotationOverTime) {
            gunPivot.rotation = Quaternion.Lerp(gunPivot.rotation, Quaternion.AngleAxis(angle, Vector3.forward), 
                Time.deltaTime * rotationSpeed);
        }
        else {
            firePoint.SetPositionAndRotation(
                transform.position + Quaternion.Euler(0, 0, angle) * Vector3.right * distanceFirePointToPlayer, 
                Quaternion.Euler(0, 0, angle));
            gunPivot.SetPositionAndRotation(
                transform.position + Quaternion.Euler(0, 0, angle) * Vector3.right * distanceFirePointToPlayer, 
                Quaternion.Euler(0, 0, angle));
        }
    }

    // This function creates based on the players fire point position (the dot next to the player) and the mouses direction
    // a fire direction of the rope and finds the first hitted object (the surroundings). Depending if the object is near enough 
    // to be used by the rope, the grapple point is set.
    // The bool return value tells us if we can grap this point or not (if its too far away).
    bool SetGrapplePoint() {
        Vector2 distanceVector = m_camera.ScreenToWorldPoint(Input.mousePosition) - gunPivot.position;
        if (Physics2D.Raycast(firePoint.position, distanceVector.normalized)) {
            // On a later version we should check what it grabbed. Because currently it also grabs items and players.
            RaycastHit2D _hit = Physics2D.Raycast(firePoint.position, distanceVector.normalized);
            if (_hit.transform.gameObject.layer == grappableLayerNumber || grappleToAll) {
                if (Vector2.Distance(_hit.point, firePoint.position) <= maxDistnace || !hasMaxDistance) {
                    grapplePoint = _hit.point;
                    grappleDistanceVector = grapplePoint - (Vector2)gunPivot.position;
                    return true;
                }
            }
        }
        return false;
    }

    [ServerRpc]
    private void SetGrapplePointServerRpc (Vector2 _grapplePoint) {
        grapplePointNetworked.Value = _grapplePoint;
    }

    [ServerRpc]
    private void SetGrapplingEnabledServerRpc (bool _enabled) {
        grapplingEnabled.Value = _enabled;
    }

    public void Grapple() {
        m_springJoint2D.autoConfigureDistance = false;
        if (!launchToPoint && !autoConfigureDistance) {
            m_springJoint2D.distance = targetDistance;
            m_springJoint2D.frequency = targetFrequncy;
        }
        if (!launchToPoint) {
            if (autoConfigureDistance) {
                m_springJoint2D.autoConfigureDistance = true;
                m_springJoint2D.frequency = 0;
            }

            m_springJoint2D.connectedAnchor = grapplePoint;
            m_springJoint2D.enabled = true;
        }
        else {
            switch (launchType) {
                case LaunchType.Physics_Launch:
                    m_springJoint2D.connectedAnchor = grapplePoint;

                    Vector2 distanceVector = firePoint.position - gunHolder.position;

                    m_springJoint2D.distance = distanceVector.magnitude;
                    m_springJoint2D.frequency = launchSpeed;
                    m_springJoint2D.enabled = true;
                    break;
                case LaunchType.Transform_Launch:
                    m_rigidbody.gravityScale = 0;
                    m_rigidbody.velocity = Vector2.zero;
                    break;
            }
        }
    }

    private void OnDrawGizmosSelected() {
        if (firePoint != null && hasMaxDistance) {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(firePoint.position, maxDistnace);
        }
    }

    private bool IsLocalPlayer()
    {
        return NetworkManager.Singleton.LocalClientId == GetComponent<NetworkObject>().OwnerClientId;
    }

    private void grapplingEnabledValueChange (bool oldValue, bool newValue) {
        grappleRope.enabled = newValue;
    }
}

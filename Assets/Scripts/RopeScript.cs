using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RopeScript : NetworkBehaviour {
    [Header("General Refernces:")]
    public GrapplingScript grapplingGun;
    public LineRenderer m_lineRenderer;

    [Header("General Settings:")]
    [SerializeField] private int percision = 40;
    [Range(0, 20)][SerializeField] private float straightenLineSpeed = 5;

    [Header("Rope Animation Settings:")]
    public AnimationCurve ropeAnimationCurve;
    [Range(0.01f, 4)][SerializeField] private float StartWaveSize = 2;
    float waveSize = 0;

    [Header("Rope Progression:")]
    public AnimationCurve ropeProgressionCurve;
    [SerializeField][Range(1, 50)] private float ropeProgressionSpeed = 1;

    float moveTime = 0;

    [HideInInspector] public bool isGrappling = true;

    bool strightLine = true;

    // Depending if we are drawing the line for the local or the non local player we use different 
    // reference values for the endpoint. We cannot always use the networked one since this value 
    // has a little lag until it was updated by the server so the local client would first draw the 
    // line to Vec3.Zero since its not updated yet and then the value would update and the correct one 
    // would be shown. To not encounter this artifact we decide on enabling what to use.
    private Vector2 grapplePointToUse;

    private void OnEnable() {
        moveTime = 0;
        m_lineRenderer.positionCount = percision;
        waveSize = StartWaveSize;
        strightLine = false;

        LinePointsToFirePoint();

        m_lineRenderer.enabled = true;
    }

    private void OnDisable() {
        m_lineRenderer.enabled = false;
        isGrappling = false;
    }

    private void LinePointsToFirePoint() {
        for (int i = 0; i < percision; i++) {
            m_lineRenderer.SetPosition(i, grapplingGun.gunHolder.position);
        }
    }

    private void Update() {
        moveTime += Time.deltaTime;

        // For more details see the big comment above.
        if (IsLocalPlayer()) {
            // Use the local one.
            grapplePointToUse = grapplingGun.grapplePoint;
        }
        else {
            // Use the networked one.
            grapplePointToUse = grapplingGun.grapplePointNetworked.Value;
        }

        DrawRope();
    }

    void DrawRope() {
        if (!strightLine) {
            if (m_lineRenderer.GetPosition(percision - 1).x == grapplePointToUse.x) {
                strightLine = true;
            }
            else {
                DrawRopeWaves();
            }
        }
        else {
            if (!isGrappling) {
                grapplingGun.Grapple();
                isGrappling = true;
            }
            if (waveSize > 0) {
                waveSize -= Time.deltaTime * straightenLineSpeed;
                DrawRopeWaves();
            }
            else {
                waveSize = 0;

                if (m_lineRenderer.positionCount != 2) { m_lineRenderer.positionCount = 2; }

                DrawRopeNoWaves();
            }
        }
    }

    void DrawRopeWaves() {
        for (int i = 0; i < percision; i++) {
            float delta = (float)i / ((float)percision - 1f);
            Vector2 offset = Vector2.Perpendicular(grapplingGun.grappleDistanceVector).normalized * ropeAnimationCurve.Evaluate(delta) * waveSize;
            Vector2 targetPosition = Vector2.Lerp(grapplingGun.gunHolder.position, grapplePointToUse, delta) + offset;
            Vector2 currentPosition = Vector2.Lerp(grapplingGun.gunHolder.position, targetPosition, ropeProgressionCurve.Evaluate(moveTime) * ropeProgressionSpeed);

            m_lineRenderer.SetPosition(i, currentPosition);
        }
    }

    void DrawRopeNoWaves() {
        m_lineRenderer.SetPosition(0, grapplingGun.gunHolder.position);
        m_lineRenderer.SetPosition(1, grapplePointToUse);
    }

    private bool IsLocalPlayer()
    {
        return GetComponentInParent<NetworkObject>().IsLocalPlayer;
    }
}

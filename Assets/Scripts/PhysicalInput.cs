using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicalInput : MonoBehaviour
{
    CharacterNerveCenter characterNerveCenter;
    [SerializeField]
    float groundDistance = 0.3f;
    [SerializeField]
    float radius = 0.5f;
    [SerializeField]
    int step = 10;

    readonly int layerMask = (1 << 0);// ~(1<<3 + 1<<6 + 1<<7 + 1<<8);// ignores characters when checking for ground

    private GroundInfo groundInfo;
    public ref readonly GroundInfo GroundData => ref groundInfo;

    private Vector3 velocity = new();
    public ref readonly Vector3 Velocity => ref velocity;
    public Vector3 internalVelocity = Vector3.zero;
    public Vector3 lastAttemptedDirection = Vector3.forward;
    public Quaternion targetRotation;
    public Vector3 moveInput;
    private void Awake()
    {
        targetRotation = transform.rotation;
        characterNerveCenter = GetComponent<CharacterNerveCenter>();
    }

    private void Update()
    {
        GatherGroundInfo();
        GatherVelocity();
    }

    private Vector3 lastPos;
    private void GatherVelocity()
    {
        if (internalVelocity != Vector3.zero) lastAttemptedDirection = internalVelocity;
        Vector3 currentPos = transform.position;
        velocity = (currentPos - lastPos) / Time.deltaTime;
        lastPos = currentPos;
    }

    private void GatherGroundInfo()
    {
        Vector3 origin = transform.position + (Vector3.up * 1f);
        float minYPoint = transform.position.y - groundDistance;
        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, groundDistance + 1f, layerMask,QueryTriggerInteraction.Ignore))
        {

            Debug.DrawRay(origin, Vector3.down * (hit.distance), Color.red, 0);
            groundInfo.detectGround = true;
            groundInfo.normal = hit.normal;
            groundInfo.point = hit.point;

            Rigidbody target = hit.rigidbody;
            groundInfo.lastHitPointVelocity = target!=null ?target.GetPointVelocity(hit.point):Vector3.zero;
       
        }
        else
        {
            groundInfo.detectGround = false;
            groundInfo.normal = Vector3.up;
            
            float stepRadius= radius/ step;

            for (int i = 1; i < step; i++)//start at 1 because we already did one check with the raycast above
            {
                float currentRadius = stepRadius * i;

                //Visualize
                Debug.DrawRay(origin + Vector3.back * currentRadius, Vector3.down * (groundDistance + 1f), Color.red, 0);
                Debug.DrawRay(origin + Vector3.forward * currentRadius, Vector3.down * (groundDistance + 1f), Color.red, 0);
                Debug.DrawRay(origin+Vector3.right*currentRadius, Vector3.down * (groundDistance + 1f), Color.red, 0);
                Debug.DrawRay(origin + Vector3.left * currentRadius, Vector3.down * (groundDistance + 1f), Color.red, 0);
                //--------------

                if (Physics.SphereCast(origin,currentRadius,Vector3.down,out hit, groundDistance + 1f, layerMask, QueryTriggerInteraction.Ignore))
                {
                    if (hit.point.y >= minYPoint)
                    {
                        if (Physics.Raycast(hit.point + (Vector3.up * 0.1f), Vector3.down, out hit, 0.2f, layerMask, QueryTriggerInteraction.Ignore))//check the ground with a raycast to account for corner normals
                        {
                            //We are grounded! lets get outa here!
                            Debug.DrawRay(groundInfo.point + (Vector3.up * 0.1f), Vector3.down * 0.1f, Color.green, 0f);
                            groundInfo.detectGround = true;
                            groundInfo.normal = hit.normal;
                            groundInfo.point = hit.point;

                            Rigidbody target = hit.rigidbody;
                            groundInfo.lastHitPointVelocity = target != null ? target.GetPointVelocity(hit.point) : Vector3.zero;

                            break;

                        }
                    }
                }
            }
        }
        groundInfo.angle = Vector3.Angle(groundInfo.normal, Vector3.up);
        groundInfo.slopeDir = Vector3.ProjectOnPlane(Vector3.down, groundInfo.normal).normalized;

        SendMessage("IsGrounded", groundInfo);
    }


    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        characterNerveCenter.CollisionAngle(hit);
    }


    private void OnDrawGizmosSelected()
    {
        Vector3 origin = transform.position + (Vector3.up *  1f);
        Vector3 finalPos = transform.position - (Vector3.up * groundDistance);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(origin, radius);
        Gizmos.DrawWireSphere(finalPos, radius);
            
    }
}
public struct GroundInfo
{
    public bool detectGround;
    public Vector3 normal;
    public float angle;
    public Vector3 slopeDir;
    public Vector3 point;
    public Vector3 lastHitPointVelocity;
}
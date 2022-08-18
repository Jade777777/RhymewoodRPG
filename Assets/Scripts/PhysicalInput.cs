using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicalInput : MonoBehaviour
{
    CharacterNerveCenter characterNerveCenter;
    [SerializeField]
    public float groundDistance = 0.3f;
    [SerializeField]
    float radius = 0.5f;
    [SerializeField]
    int step = 10;
    [SerializeField]
    public float maxSlope=40;//This value must be the same value used in the slope checks of the animator
    [SerializeField]
    public float originY =1f;

    public readonly int groundLayerMask = 1 << 0;//  ~(1<<3 + 1<<6 + 1<<7 + 1<<8);// ignores characters when checking for ground

    private GroundInfo groundInfo;
    public ref readonly GroundInfo GroundData => ref groundInfo;

    private Vector3 velocity = new();
    public ref readonly Vector3 Velocity => ref velocity;
    [NonSerialized]
    public Vector3 internalVelocity = Vector3.zero;
    [NonSerialized]
    public Vector3 lastAttemptedDirection = Vector3.forward;
    [NonSerialized]
    public Quaternion targetRotation;
    [NonSerialized]
    public Vector3 moveInput;


    private void Awake()
    {
        for (int i = 0; i < 5; i++)
        {
            velRoll.Enqueue(Vector3.zero);
        }
        targetRotation = transform.rotation;
        characterNerveCenter = GetComponent<CharacterNerveCenter>();
    }

    private void Update()
    {
        GatherGroundInfo();
        GatherVelocity();
    }

    private Vector3 lastPos;
    Queue<Vector3> velRoll = new();
    private void GatherVelocity()
    {
        if (internalVelocity != Vector3.zero) lastAttemptedDirection = internalVelocity;
        //calculate rolling velocity
        Vector3 currentPos = transform.position;
        velRoll.Dequeue();
        velRoll.Enqueue(Vector3.ClampMagnitude((currentPos - lastPos) / Time.deltaTime, 50));
        velocity = Vector3.zero;
        foreach(Vector3 vel in velRoll)
        {
            velocity += vel;
        }
        velocity /= velRoll.Count;

        lastPos = currentPos;
    }

    private void GatherGroundInfo()
    {
        float groundDistance = this.groundDistance+ Mathf.Clamp(velocity.y*Time.deltaTime, float.NegativeInfinity, 0);//keeps landing from feeling squishy due to a partial move.
        
        Vector3 origin = transform.position + (Vector3.up * originY);
        float minYPoint = transform.position.y - groundDistance;

        groundInfo.detectGround = false;
        groundInfo.normal = Vector3.up;
        float stepRadius = radius / step;

        bool grounded = Physics.Raycast(origin, Vector3.down, out RaycastHit hit, groundDistance + originY, groundLayerMask, QueryTriggerInteraction.Ignore);
        bool isSlideSlope = false;
        if (grounded)
        {
                Debug.DrawRay(origin, Vector3.down * (hit.distance), Color.red, 0);
                groundInfo.detectGround = true;
                groundInfo.normal = hit.normal;
                groundInfo.point = hit.point;

                Rigidbody target = hit.rigidbody;
                groundInfo.lastHitPointVelocity = target != null ? target.GetPointVelocity(hit.point) : Vector3.zero;

            if (Vector3.Angle(hit.normal, Vector3.up) >= maxSlope) isSlideSlope = true;


        }
        if (grounded == false||isSlideSlope)
        {

            for (int i = 1; i < step; i++)//start at 1 because we already did one check with the raycast above
            {
                float currentRadius = stepRadius * i;



                if (Physics.SphereCast(origin,currentRadius,Vector3.down,out hit, groundDistance + originY, groundLayerMask, QueryTriggerInteraction.Ignore))
                {
                    if (hit.point.y >= minYPoint)
                    {
                        Vector3 hitspot = hit.point;
                        
                        hitspot.y = 0;
                        Vector3 hitorigin = origin;
                        hitorigin.y = 0;
                        Vector3 offset = hitspot - hitorigin;
                        offset = offset.normalized * 0.01f*Vector3.Dot(hit.normal,Vector3.up) + (Vector3.up * 0.1f);// make sure raycast doesn't miss the edge.
                        if (Physics.Raycast(hit.point+offset, Vector3.down, out hit, 0.2f, groundLayerMask, QueryTriggerInteraction.Ignore))//check the ground with a raycast to account for corner normals
                        {
                            //Visualize
                            Debug.DrawRay(origin + Vector3.back * currentRadius, Vector3.down * (groundDistance + originY), Color.red, 0);
                            Debug.DrawRay(origin + Vector3.forward * currentRadius, Vector3.down * (groundDistance + originY), Color.red, 0);
                            Debug.DrawRay(origin + Vector3.right * currentRadius, Vector3.down * (groundDistance + originY), Color.red, 0);
                            Debug.DrawRay(origin + Vector3.left * currentRadius, Vector3.down * (groundDistance + originY), Color.red, 0);
                            //--------------

                            //We are grounded! lets get outa here!
                            Debug.DrawRay(groundInfo.point + (Vector3.up * 0.1f), Vector3.down * 0.1f, Color.green, 0f);
                            groundInfo.detectGround = true;
                            groundInfo.normal = hit.normal;
                            groundInfo.point = hit.point;

                            Rigidbody target = hit.rigidbody;
                            groundInfo.lastHitPointVelocity = target != null ? target.GetPointVelocity(hit.point) : Vector3.zero;

                            if (Vector3.Angle(hit.normal, Vector3.up) >= maxSlope) isSlideSlope = true;
                            if (!isSlideSlope)
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
        Vector3 origin = transform.position + (Vector3.up *  originY);
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
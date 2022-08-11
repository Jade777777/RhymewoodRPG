using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BaseState : MonoBehaviour
{
    #region Inspector
    //---------------------------------------------// save these values on awake and reset to default OnEnable.
    [SerializeField]
    public MovementType movementType = MovementType.Ground;

    [SerializeField]
    [Range(0f, 1f)]
    public float moveInputWeight = 1f;

    [SerializeField]
    [Range(0f, 1f)]
    public float cameraInputWeight = 1f;

    [SerializeField]
    [Range(0f, 1f)]
    public float cameraAnimationWeight = 0.2f;

    [SerializeField]
    public float turnSpeed = 360;

    [SerializeField]
    public float smoothMoveInput = 4f;


    //----------------------------------------------
    #endregion

    protected CharacterStats        characterStats;
    protected CharacterController   characterController;
    protected PhysicalInput         physicalInput;
    protected Animator              animator;
    protected HitStop               hitStop;
    protected CameraController                cameraController;
    protected Transform                       head;
    protected Transform                       cameraTarget;
    protected CharacterNerveCenter            cnc;
    protected AnimatorScriptControl           animatorScriptControl;
    
    public enum MovementType { Ground, Horizontal, Slide, Airborn, Animated };

    private Movement movement;
    protected virtual void Awake()
    {
        animator            = GetComponent<Animator>();
        physicalInput       = GetComponent<PhysicalInput>();
        characterController = GetComponent<CharacterController>();
        cnc                 = GetComponent<CharacterNerveCenter>();
        animatorScriptControl=GetComponent<AnimatorScriptControl>();
        head                = new List<GameObject>(GameObject.FindGameObjectsWithTag("PlayerHead")).Find(g => g.transform.IsChildOf(this.transform)).transform;
        cameraTarget        = new List<GameObject>(GameObject.FindGameObjectsWithTag("CameraTarget")).Find(g => g.transform.IsChildOf(this.transform)).transform;//GameObject.FindGameObjectsWithTag("CameraTarget")[0].transform;
        cameraController          = Camera.main.GetComponent<CameraController>();
        characterStats      = GetComponent<StatWarden>().characterStats;
        hitStop             = GetComponent<HitStop>();

    }
    private void Start()
    {
        
        if (cnc.IsPlayer)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
    protected virtual void OnEnable()
    {
    }
    protected virtual void OnDisable()
    {
    }

    #region Movement Input

    Vector3 inputDirection = Vector3.zero;
    public virtual void MoveInput(Vector2 input)
    {

        Vector2 weightedMoveInput = input;// * animatorScriptControl.moveInputWeight;

        inputDirection = new Vector3(weightedMoveInput.x, 0, weightedMoveInput.y);
        
    }
    #endregion

    #region Movement types
    Vector3 moveDistance;
    protected void MoveAllongGround()
    {
        Vector3 dir = (physicalInput.moveInput);
        dir.y = 0f;
        Vector3 inputDirOnGround = Vector3.ProjectOnPlane(dir, physicalInput.GroundData.normal).normalized * physicalInput.moveInput.magnitude;
        inputDirOnGround.y = 0f;
        inputDirOnGround *= characterStats.PrimalStats()["Move Speed"];

        float targetY;

        float distToGround = physicalInput.GroundData.point.y - transform.position.y;
        float fallDist = (physicalInput.internalVelocity.y - (9.8f * Time.deltaTime)) * Time.deltaTime;

        if(physicalInput.GroundData.detectGround) fallDist = Mathf.Clamp(fallDist, distToGround, Mathf.Infinity);
        targetY = physicalInput.GroundData.detectGround&&hitStop.knockBackVelocity==Vector3.zero ? distToGround : fallDist;


        Vector3 moveSpeed = inputDirOnGround + physicalInput.GroundData.lastHitPointVelocity;
        moveDistance = (moveSpeed) * Time.deltaTime+(Vector3.up*targetY);
        
        physicalInput.internalVelocity = moveSpeed;
    }

    protected void MoveHorizontaly()
    {
        Vector3 dir = physicalInput.moveInput;
        dir.y = 0f;
        dir = dir.normalized * physicalInput.moveInput.magnitude;
        dir *= characterStats.PrimalStats()["Move Speed"];
        Vector3 moveSpeed = dir + physicalInput.GroundData.lastHitPointVelocity;
        moveDistance =(moveSpeed) * Time.deltaTime;
        physicalInput.internalVelocity = moveSpeed;

    }


    protected void Slide()// sliding also handles free falling
    {
        Vector3 dir = physicalInput.moveInput;
        dir.y = 0f;
        Vector3 inputDirOnGround = Vector3.ProjectOnPlane(dir, physicalInput.GroundData.normal).normalized * physicalInput.moveInput.magnitude;

        float slideControl = 0.15f;
        float acceleration = 9.8f;
        float maxSpeed = 100;

        Vector3 vel = physicalInput.internalVelocity;
        if (physicalInput.GroundData.slopeDir != Vector3.zero)
        {
            vel = Vector3.ProjectOnPlane(vel, physicalInput.GroundData.normal);
        }
        vel.y = Mathf.Clamp(vel.y, float.NegativeInfinity, 0);
        vel = Vector3.ClampMagnitude(vel, maxSpeed); 
        
        Vector3 targetSlope =physicalInput.GroundData.slopeDir != Vector3.zero ? physicalInput.GroundData.slopeDir : Vector3.down;
        targetSlope = (targetSlope*(1-slideControl)) + (inputDirOnGround*slideControl);//adjsuts
        targetSlope.Normalize();
        Vector3 moveSpeed = Vector3.MoveTowards(vel, targetSlope * maxSpeed, acceleration * Time.deltaTime);

        

        moveDistance =(moveSpeed*Time.deltaTime);
        if (physicalInput.GroundData.detectGround )
        {
            float distToGround = physicalInput.GroundData.point.y - transform.position.y;
            moveDistance.y = distToGround;
        }

        physicalInput.internalVelocity = moveSpeed;
    }
    private void Airborn()
    {
        Vector3 vel = physicalInput.internalVelocity;

        vel.y = 0; //only horizontal velocity
        float acceleration = characterStats.PrimalStats()["Move Speed"];//it takes 2 seconds to fully change directions. It takes 1 second to stop
        float speed= characterStats.PrimalStats()["Move Speed"];

        Vector3 dir = physicalInput.moveInput;
        dir.y = 0f;
        dir = dir.normalized * physicalInput.moveInput.magnitude; //only horizontal input relative to camera
        

        Vector3 moveSpeed = Vector3.MoveTowards(vel, dir * speed, acceleration*Time.deltaTime);
        moveDistance=(moveSpeed+ Vector3.up*physicalInput.GroundData.lastHitPointVelocity.y) * Time.deltaTime;
        physicalInput.internalVelocity = moveSpeed+ Vector3.up*physicalInput.Velocity.y;
    }
    float animatedOverride;
    private void Animated()
    {
        animatedOverride = 0;
        moveDistance = Vector3.zero;
        physicalInput.internalVelocity = physicalInput.Velocity;
    }
    #endregion

    #region Orientation Input
    private bool rotUpdated = false; 
    public void PlayerLookInput(Vector2 input)
    {
        if (this.enabled&&cnc.IsPlayer==true)
        {
            rotUpdated = true;
            Vector2 weightedLookInput = input * animatorScriptControl.cameraInputWeight;
            Vector3 facing = new Vector3(0, weightedLookInput.x, 0) * 0.05f;
            physicalInput.targetRotation = Quaternion.Euler(transform.rotation.eulerAngles + facing);
           
            Vector3 lookY = new Vector3(-weightedLookInput.y, 0, 0) * 0.05f;
            lookY = cameraTarget.localRotation.eulerAngles + lookY;
            lookY.x = JadeMath.ClampAngle(lookY.x, -89, 89);
            cameraTarget.localRotation = Quaternion.Euler(lookY);

        }
    }
    
    public void NPCLookInput(Vector3 direction)//add in rotation speed
    {
       
        if (this.enabled&&cnc.IsPlayer == false)
        {
            rotUpdated = true;
            direction.y = 0f;
            Quaternion rot = Quaternion.LookRotation(direction, Vector3.up);
            physicalInput.targetRotation = rot;
        }
    }

    #endregion


    protected Vector3 cOffset = new(0, 0.3f, -0.3f);

    protected virtual void Update()
    {
       
        physicalInput.moveInput = ( Vector3.MoveTowards((physicalInput.moveInput),transform.TransformDirection(inputDirection * animatorScriptControl.moveInputWeight), animatorScriptControl.smoothMoveInput * Time.deltaTime));
      
        if(rotUpdated == false)
        {
            physicalInput.targetRotation = transform.rotation;
        }
        else
        {
            rotUpdated = false;
        }
        transform.rotation = Quaternion.RotateTowards(transform.rotation, physicalInput.targetRotation, Time.deltaTime * animatorScriptControl .turnSpeed* animatorScriptControl.cameraInputWeight);
       
    }
    protected void LateUpdate()
    {
        switch (animatorScriptControl.movementType)
        {
            case MovementType.Ground:
                movement = () => MoveAllongGround();
                break;
            case MovementType.Horizontal:
                movement = () => MoveHorizontaly();
                break;
            case MovementType.Slide:
                movement = () => Slide();
                break;
            case MovementType.Airborn:
                movement = () => Airborn();
                break;
            case MovementType.Animated:
                movement = () => Animated();
                break;
            default:
                Debug.LogError("Movement Type has not been called");
                break;
        }

        movement();
        characterController.Move(animatedOverride * animator.speed * (moveDistance + (hitStop.knockBackVelocity*Time.deltaTime)));
        animatedOverride = 1;

        if (cnc.IsPlayer)//if the character is being controlled by the player as of Awake
        {
            Vector3 camPos = head.position + cameraTarget.TransformDirection(cOffset);
            camPos = head.InverseTransformPoint(camPos); // clamp the local values
            camPos.y = Mathf.Clamp(camPos.y, -cOffset.z, float.PositiveInfinity) - 0.2f;
            camPos.z = Mathf.Clamp(camPos.z, -0.15f, float.PositiveInfinity);
            camPos = head.TransformPoint(camPos); // change them back to global

            Quaternion weightedHeadRotation = head.rotation * Quaternion.Inverse(transform.rotation);
            weightedHeadRotation = Quaternion.Lerp(Quaternion.identity, weightedHeadRotation, animatorScriptControl.cameraAnimationWeight);
            Quaternion camRot = weightedHeadRotation * cameraTarget.rotation;

            cameraController.SetPositionAndRotation(camPos, camRot);
        }

    }

    private delegate void Movement();


}

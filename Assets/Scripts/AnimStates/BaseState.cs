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
    [Range(0f, 1f)]
    public float stabalizeCameraY = 0f;

    [SerializeField]
    [Range(0f, 1f)]
    public float stabalizeCameraX = 0f;

    [SerializeField]
    [Range(0f, 1f)]
    public float stabalizeCameraZ = 0f;

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
    protected CameraController      cameraController;
    protected Transform             head;
    protected Transform             cameraTarget;
    protected CharacterNerveCenter  cnc;
    protected AnimatorScriptControl animatorScriptControl;


    public enum MovementType { Ground, GroundSnap, Horizontal, Slide, Airborn, Animated };

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
    private void MoveCC()
    {
        characterController.Move(animator.speed
                                * (moveDistance + (hitStop.knockBackVelocity * Time.deltaTime))
                                );
    }

   
    protected void MoveAllongGround()
    {
        Vector3 dir = physicalInput.moveInput;
        dir.y = 0f;
        dir = dir.normalized * physicalInput.moveInput.magnitude;
        Vector3 inputDirOnGround=dir;
        if (physicalInput.GroundData.angle < physicalInput.maxSlope)
        {
            inputDirOnGround = Vector3.ProjectOnPlane(dir, physicalInput.GroundData.normal).normalized * physicalInput.moveInput.magnitude;
        }

        inputDirOnGround.y = 0f;
        inputDirOnGround *= characterStats.PrimalStats()["Move Speed"];

        float targetY;

        float distToGround = physicalInput.GroundData.point.y - transform.position.y;
        float fallDist = 0;

        if (physicalInput.GroundData.angle < physicalInput.maxSlope && hitStop.knockBackVelocity == Vector3.zero)
        {
            targetY = Mathf.Sign(distToGround) * Mathf.Pow(distToGround, 2f) * 50 * Time.deltaTime;
        }
        else
        {
            fallDist = (physicalInput.internalVelocity.y - (9.8f * Time.deltaTime));
            targetY = fallDist*Time.deltaTime;
            if (physicalInput.GroundData.detectGround)
            {
                targetY = Mathf.Clamp(targetY, distToGround, Mathf.Infinity);
            }
        }


        Vector3 moveSpeed = inputDirOnGround + physicalInput.GroundData.lastHitPointVelocity;
        moveDistance = (moveSpeed) * Time.deltaTime+(Vector3.up*targetY);
        
        physicalInput.internalVelocity = dir*characterStats.PrimalStats()["Move Speed"]
                                         + physicalInput.GroundData.lastHitPointVelocity
                                         + Vector3.up*fallDist;

        MoveCC();
    }
    
    protected void SnapToGround()
    {

        float snapSpeed=Mathf.Max(Mathf.Abs(physicalInput.internalVelocity.y)*1.5f,0)  * Time.deltaTime;//snap to ground 1.5 times as fast as the fall speed
        Vector3 dir = physicalInput.moveInput;
        dir.y = 0f;
        dir = dir.normalized * physicalInput.moveInput.magnitude;
        Vector3 inputDirOnGround = dir;
        if (physicalInput.GroundData.angle < physicalInput.maxSlope)
        {
            inputDirOnGround = Vector3.ProjectOnPlane(dir, physicalInput.GroundData.normal).normalized * physicalInput.moveInput.magnitude;
            
        }

        inputDirOnGround.y = 0f;
        inputDirOnGround *= characterStats.PrimalStats()["Move Speed"];

        float targetY;

        float distToGround = physicalInput.GroundData.point.y - transform.position.y;
        


        if (physicalInput.GroundData.angle < physicalInput.maxSlope && hitStop.knockBackVelocity == Vector3.zero)
        {
            targetY = distToGround;
        }
        else
        {
            float fallDist = physicalInput.internalVelocity.y; //- (9.8f * Time.deltaTime);
            targetY = fallDist * Time.deltaTime;

            if (physicalInput.GroundData.detectGround)
            {
                targetY = Mathf.Clamp(targetY, distToGround, Mathf.Infinity);
            }
        }


        if (Mathf.Abs(targetY) > snapSpeed*2)
        {
            targetY = snapSpeed*Mathf.Sign(targetY);
        }
 
        Vector3 moveSpeed = inputDirOnGround + physicalInput.GroundData.lastHitPointVelocity;
        
        moveDistance = (moveSpeed) * Time.deltaTime + (Vector3.up * targetY);

        physicalInput.internalVelocity = dir * characterStats.PrimalStats()["Move Speed"]
                                         + physicalInput.GroundData.lastHitPointVelocity
                                         + Vector3.up*(physicalInput.internalVelocity.y-9.8f*2*Time.deltaTime);

        MoveCC();
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


        MoveCC();
    }


    protected void Slide()// sliding also handles free falling
    {

        float acceleration = 9.8f;
        float maxSpeed = 100;
        float minSpeed = 4;

        Vector3 vel = physicalInput.internalVelocity;
        float inclineMultiplier = 1;

        if (physicalInput.GroundData.detectGround&&physicalInput.GroundData.angle> physicalInput.maxSlope)
        {
            vel = Vector3.ProjectOnPlane(vel, physicalInput.GroundData.normal);
            inclineMultiplier = physicalInput.GroundData.angle / 90;// slide less the shallower the angle.
            inclineMultiplier *= inclineMultiplier;
        }
        vel.y = Mathf.Clamp(vel.y, float.NegativeInfinity, 0);
        vel = Vector3.ClampMagnitude(vel, maxSpeed); 
        
        Vector3 targetSlope =physicalInput.GroundData.slopeDir != Vector3.zero 
                && physicalInput.GroundData.angle > physicalInput.maxSlope ? physicalInput.GroundData.slopeDir : Vector3.down;

        targetSlope.Normalize();
        Vector3 moveSpeed = Vector3.MoveTowards(vel, targetSlope * maxSpeed, (acceleration * Time.deltaTime) * inclineMultiplier);
        //Remove down vector from moveSpeed;
        Vector3 down = Vector3.Project(moveSpeed, targetSlope);
        moveSpeed -= down;



        Vector3 dir = physicalInput.moveInput;
        dir.y = 0f;
        Vector3 inputDirOnGround = Vector3.ProjectOnPlane(dir, physicalInput.GroundData.normal).normalized;
        float inputDirDown = Vector3.Dot(inputDirOnGround, targetSlope);
        inputDirOnGround -= targetSlope*inputDirDown;

        float slideControl = physicalInput.moveInput.magnitude;
        float AirControl = characterStats.PrimalStats()["Air Control"];
        float speed = characterStats.PrimalStats()["Move Speed"];

        if (inputDirDown >= 0)
        {
            float inputAcceleration = 0.1f;
            down += (targetSlope *inputAcceleration* AirControl * inputDirDown * Time.deltaTime);
            
        }
        else
        {
            down = Vector3.MoveTowards(down, targetSlope*minSpeed, -inputDirDown * AirControl * slideControl * Time.deltaTime);
        }


        moveSpeed = Vector3.MoveTowards(moveSpeed, inputDirOnGround* speed, AirControl * slideControl * Time.deltaTime);
        moveSpeed += down;

        moveDistance =(moveSpeed*Time.deltaTime);
        if (physicalInput.GroundData.detectGround 
            && physicalInput.GroundData.angle<70
            &&physicalInput.GroundData.angle>physicalInput.maxSlope)
        {
            float targetY =physicalInput.GroundData.point.y - transform.position.y;
            targetY = Mathf.Sign(targetY) * Mathf.Pow(targetY, 2f) * 50 * Time.deltaTime;
            moveDistance.y = targetY;
        }

        physicalInput.internalVelocity = moveSpeed;



        MoveCC();
    }
    private void Airborn()
    {
        Vector3 vel = physicalInput.internalVelocity;

        vel.y = 0; //only horizontal velocity
        float AirControl = characterStats.PrimalStats()["Air Control"];//it takes 2 seconds to fully change directions. It takes 1 second to stop
        float speed= characterStats.PrimalStats()["Move Speed"];

        Vector3 dir = physicalInput.moveInput;
        dir.y = 0f;
        dir = dir.normalized * physicalInput.moveInput.magnitude; //only horizontal input relative to camera
        

        Vector3 moveSpeed = Vector3.MoveTowards(vel, dir * speed, AirControl*Time.deltaTime);
        moveDistance=(moveSpeed+ Vector3.up*physicalInput.GroundData.lastHitPointVelocity.y) * Time.deltaTime;


        float offset= (characterController.skinWidth + 0.1f);
        Vector3 origin = characterController.center + transform.position - moveDistance.normalized * offset;
        Vector3 bottom = origin + Vector3.down*characterController.height*0.5f;
        Vector3 top = origin + Vector3.up * characterController.height*0.5f;
        
        if(Physics.CapsuleCast(bottom,top,characterController.radius,moveDistance, out RaycastHit hit, moveDistance.magnitude+offset, physicalInput.groundLayerMask))
        {
            Debug.Log("Running into wall");
            moveDistance = Vector3.Project(moveDistance, Vector3.Cross(hit.normal, Vector3.up));
            
        }
        physicalInput.internalVelocity = moveSpeed+ Vector3.up*physicalInput.Velocity.y;



        MoveCC();
    }

    private void Animated()
    {
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
            lookY.x = JadeMath.ClampAngle(lookY.x, -85, 85);
            cameraTarget.localRotation = Quaternion.Euler(lookY);

        }
    }
    public void ResetLookInput()
    {
        cameraTarget.localRotation = Quaternion.identity;
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
    protected virtual void LateUpdate()
    {
        switch (animatorScriptControl.movementType)
        {
            case MovementType.Ground:
                movement = () => MoveAllongGround();
                break;
            case MovementType.GroundSnap:
                movement = () => SnapToGround();
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



        CameraPosAndRot(out Vector3 headOffset, out float excess);

        Vector3 controllerOffsetDelta = Vector3.zero;
        if (excess > 0f)//if its less than zero we do nothing, if its more we adjust
        {

            Vector3 controllerOffset = headOffset.normalized * excess;

            //characterController.center =new(controllerOffset.x,characterController.center.y,controllerOffset.z);
        }




    }

    private void CameraPosAndRot(out Vector3 headOffset, out float excess)
    {
        if (cnc.IsPlayer)//if the character is being controlled by the player as of Awake
        {
            Vector3 camPos = head.position + cameraTarget.TransformDirection(cOffset);
            camPos = head.InverseTransformPoint(camPos); // clamp the local values
            camPos.y = Mathf.Clamp(camPos.y, -cOffset.z, float.PositiveInfinity) - 0.2f;
            camPos.z = Mathf.Clamp(camPos.z, -0.15f, float.PositiveInfinity);
            camPos = head.TransformPoint(camPos); // change them back to global

            Quaternion relativeHeadRotation = head.rotation * Quaternion.Inverse(transform.rotation);
            Quaternion weightedHeadRotation = Quaternion.Lerp(cameraTarget.rotation, relativeHeadRotation * transform.rotation, animatorScriptControl.cameraAnimationWeight);
            Quaternion camRot = weightedHeadRotation;


            cameraController.SetPositionAndRotation(camPos, camRot, physicalInput.Velocity);
            head.localScale = Vector3.zero;
            headOffset = transform.InverseTransformPoint(camPos);
        }
        else
        {
            headOffset = transform.InverseTransformPoint(head.position);
            head.localScale = Vector3.one;
        }

        //The controlleroffset can be used to adjust the modelas well as the camera to ensure it does not collide with geometry
        headOffset.y = 0;
        excess = headOffset.magnitude - characterController.radius * 0.7f;
    }

    private delegate void Movement();


}

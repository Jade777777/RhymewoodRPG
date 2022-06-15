using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CharacterFate : MonoBehaviour
{

    [SerializeField]
    GameObject characterInstance;

    [SerializeField]
    List<Behavior> rawBehaviors;

    NavMeshAgent agent;
    CharacterNerveCenter cnc;
    KnowledgeBase knowledgeBase;


    Dictionary<string, List<PatrolPoint>> behaviors = new();
    Dictionary<string, PatrolPoint> currentPatrolPoints = new();


    private void Awake()
    {
        cnc = characterInstance.GetComponent<CharacterNerveCenter>();
        knowledgeBase = characterInstance.GetComponent<KnowledgeBase>();
        agent = GetComponent<NavMeshAgent>();
        agent.radius = characterInstance.GetComponent<CharacterController>().radius *2f;
        agent.avoidancePriority = 3;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.GoodQualityObstacleAvoidance;
        foreach (var value in rawBehaviors)
        {
            currentPatrolPoints.Add(value.BehaviorName,value.patrolPoints[0]);
            behaviors.Add(value.BehaviorName, value.patrolPoints);
            StartCoroutine(PatrolPointUpdate(value.BehaviorName));
        }
    }
    private void Start()
    {
        agent.updatePosition = false;
        agent.updateRotation = false;
        agent.speed = cnc.PrimalStats()["Move Speed"];
    }


    IEnumerator PatrolPointUpdate(string behaviorName)
    {
        while (behaviors.Count>0)
        {
            foreach (PatrolPoint patrolPoint in behaviors[behaviorName])
            {
                currentPatrolPoints[behaviorName] = patrolPoint;
                yield return new WaitForSeconds(patrolPoint.WaitTime);
            }
        }
    }



    private void LateUpdate()
    {
        
        agent.SetDestination(CalcPosition());// add flanking functionality and awareness of the combat situation. make sure target positions around the enemies are all unique.
        RotateCharacter();
        MoveCharacter();
        
    }

    private Vector3 CalcPosition()
    {
        
        agent.nextPosition = characterInstance.transform.position;
        PatrolPoint currentPatrolPoint = currentPatrolPoints[cnc.GetBehavior()];
        //******
        Transform target = ProccessTarget(currentPatrolPoint.TargetPosition);//calc apropriate target;
        //******


        
        if (target == null)
        {
            return characterInstance.transform.position;
        }
        
        Vector3 currentOffset = agent.nextPosition - target.position;

        currentOffset.y = 0f;
        float currentDistance = currentOffset.magnitude;
        Vector3 targetOffset= Vector3.zero;

        switch (currentPatrolPoint.Type)
        {
            case PositioningType.Unlocked://retreating/ranged enemies may use this
                targetOffset = currentOffset.normalized;
                break;
            case PositioningType.Velocity:// Enemies may try to impede the movement of the player
                targetOffset = target.GetComponent<PhysicalInput>().lastAttemptedDirection.normalized;
                break;
            case PositioningType.Front: // get in the players face, make it a little easier.
                targetOffset = target.forward;
                break;
            case PositioningType.Behind:// This is good for positional dodging as well sneaking
                targetOffset = -target.forward;
                break;
        }
        
        Debug.Assert(targetOffset != Vector3.zero);
        if(Vector3.Angle(targetOffset, currentOffset)<=currentPatrolPoint.Angle/2)
        {
            targetOffset = currentOffset.normalized;
        }
        targetOffset = Vector3.RotateTowards(currentOffset.normalized, targetOffset,0.78f, 0f);//Characters move around a target in a circular fasion. Makes it look cleaner and avoids some collisions.
        Vector3 navPos = target.position + targetOffset*Mathf.Clamp(currentDistance,
                                                                    currentPatrolPoint.MinDistance+characterInstance.GetComponent<CharacterController>().radius,
                                                                    currentPatrolPoint.MaxDistance+agent.radius);
        return navPos;
    }
    private void MoveCharacter()
    {

        if (agent.hasPath && agent.pathStatus != NavMeshPathStatus.PathInvalid)
        {
            Vector3 localDesiredInput = characterInstance.transform.InverseTransformDirection(agent.desiredVelocity);
            Vector2 velocity = new Vector2(localDesiredInput.x, localDesiredInput.z) / agent.speed;
            Debug.Assert(velocity.magnitude<=1.1f);//for some reason it seems as if NAN is returned sometimes, 
            if (velocity != Vector2.zero) cnc.Move(velocity);
        }
        else
        {
            cnc.Move(Vector2.zero);
        }
    }
    private void RotateCharacter()
    {
        PatrolPoint currentPatrolPoint = currentPatrolPoints[cnc.GetBehavior()];
        //******
        Transform target = ProccessTarget(currentPatrolPoint.TargetLook);//calc apropriate target;
        //******
        Vector3 lookDirection;
        if (target != null)
        {
            lookDirection = (target.position - characterInstance.transform.position).normalized;
        }
        else
        {
            lookDirection = (characterInstance.GetComponent<PhysicalInput>().internalVelocity).normalized;
        }
        if (lookDirection != Vector3.zero)
        {
            cnc.NPCLook(lookDirection);
        }
    }

    private enum PositioningType { Front, Behind, Velocity, Unlocked};
    [System.Serializable]
    struct PatrolPoint
    {
        public Target TargetPosition;
        public Target TargetLook;
        public PositioningType Type;
        public float Angle;
        public float MaxDistance;//
        public float MinDistance;// this will generaly not be set by mele characters unless they need to exhibit cowardly like behaviors.
        public float WaitTime;
    }

    [System.Serializable]
    struct Behavior
    {
        public string BehaviorName;
        public List<PatrolPoint> patrolPoints;
    }
    [System.Serializable]
    struct Target 
    {
        public TargetType targetType; 
        public Transform defaultPoint; // This is the default/fallback value. Used for designer to detrmine patrol point.

    }

    private Transform ProccessTarget(Target target)
    {
        GameObject t;

        switch (target.targetType)
        {
            case TargetType.PatrolPoint:
                t = null;
                break;
            case TargetType.ClosestHostile:
                t = knowledgeBase.ClosestSightedHostile;
                break;
            case TargetType.ClosestNuetral:
                t = knowledgeBase.ClosestSightedNuetral;
                break;
            case TargetType.ClosestFriendly:
                t = knowledgeBase.ClosestSightedFriendly;
                break;
            case TargetType.ClosestCharacter:
                t = knowledgeBase.ClosestSightedCharacter;
                break;
            case TargetType.HighestAgro:
                t = knowledgeBase.HighestAgroSighted;
                break;
            case TargetType.CurrentEnemy:
                t = knowledgeBase.CurrentEnemy;
                break;
            case TargetType.FriendlysHighestAgro:
                t = knowledgeBase.FriendlyHighestAgro();
                break;
            default:
                t = null;
                Debug.LogError(target.targetType + "Has not been properly assigned");
                break;
        }
        
        if (t == null)
        {
            return target.defaultPoint;
        }
        else 
        {
           
            return t.transform;
        } 

    }
    private enum TargetType { PatrolPoint, ClosestHostile, ClosestNuetral, ClosestFriendly, ClosestCharacter, HighestAgro, CurrentEnemy, FriendlysHighestAgro };

}

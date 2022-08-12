using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class CharacterFate : BaseFate
{
    [SerializeField]
    List<Behavior> rawBehaviors;


    KnowledgeBase knowledgeBase;


    Dictionary<string, List<PatrolPoint>> behaviors = new();
    Dictionary<string, PatrolPoint> currentPatrolPoints = new();
    
    private static Dictionary<Transform, SortedList<int, GameObject>> targetGroups = new();
    protected override void Awake()
    {
        base.Awake();
        knowledgeBase = characterInstance.GetComponent<KnowledgeBase>();
        
        agent.avoidancePriority = 3;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.GoodQualityObstacleAvoidance;

    }
    private void Start()
    {
        foreach (var value in rawBehaviors)
        {
            currentPatrolPoints.Add(value.BehaviorName, value.patrolPoints[0]);
            behaviors.Add(value.BehaviorName, value.patrolPoints);
            StartCoroutine(PatrolPointUpdate(value.BehaviorName));
        }
        agent.updatePosition = false;
        agent.updateRotation = false;
        agent.speed = cnc.PrimalStats()["Move Speed"];
        agent.stoppingDistance = .04f;
    }


    IEnumerator PatrolPointUpdate(string behaviorName)
    {
        while (behaviors.Count>0)
        { 

            foreach (PatrolPoint patrolPoint in behaviors[behaviorName])
            {
                currentPatrolPoints[behaviorName] = patrolPoint;

                if (cnc.GetBehavior() == behaviorName)
                {
                    PressButton(patrolPoint);
                }
               
                yield return new WaitForSeconds(patrolPoint.BaseWaitTime+Random.Range(0,patrolPoint.WaitTimeRandomize));
                while (characterInstance.activeSelf == false)
                {
                    yield return new WaitForSeconds(1);
                }
            }
        }
    }



    private void LateUpdate()
    {
        if (characterInstance.activeInHierarchy == true)
        {
            Vector3 destination = CalcPosition();
            if (isValidMove) {
                agent.SetDestination(destination);// add flanking functionality and awareness of the combat situation. make sure target positions around the enemies are all unique.
                RotateCharacter();
                MoveCharacter();
            } 
        }
        else if (oldTarget != null && targetGroups.ContainsKey(oldTarget))
        {
            targetGroups[oldTarget].Remove(characterInstance.GetInstanceID());
        }
        
    }
 
    private void PressButton(PatrolPoint patrolPoint)
    {
        

        switch (patrolPoint.buttonPress)
        {
            case ButtonPress.Attack:
                cnc.Attack();
                break;
            case ButtonPress.Kick:
                cnc.Kick();
                break;
            case ButtonPress.Jump:
                Debug.Log(cnc.GetBehavior());
                cnc.Jump(true);
                break;
            case ButtonPress.Interact:
                cnc.Interact();
                break;
            default:
                break;
        }
    }




    Transform oldTarget = null;
    bool isValidMove;
    private Vector3 CalcPosition()
    {
        if (Vector3.Distance(characterInstance.transform.position, agent.nextPosition) >= 1f)
        {
            if (agent.Warp(characterInstance.transform.position))
            {
                if (NavMesh.SamplePosition(characterInstance.transform.position + Vector3.up, out NavMeshHit hit, 2, 1 << NavMesh.GetAreaFromName("Walkable")))
                {
                    agent.nextPosition = hit.position;
                    isValidMove = true;
                }
                else
                {
                    //TODO: If it is not a valid move tranisition to aerial path finding
                    isValidMove = false;
                }
            }
        }
        else
        {
            if(NavMesh.SamplePosition(characterInstance.transform.position + Vector3.up, out NavMeshHit hit, 2, 1 << NavMesh.GetAreaFromName("Walkable")))
            {
                agent.nextPosition = hit.position;
                isValidMove = true;
            }
            else
            {
                isValidMove = false;
            }
        }
        PatrolPoint currentPatrolPoint = currentPatrolPoints[cnc.GetBehavior()];
        //******
        Transform target = ProccessTarget(currentPatrolPoint.TargetPosition);//calc apropriate target;
                                                                             //******

        


        //Check if there are any other characters with the saem target and make room for eachother;
        
        if (oldTarget != target)
        {
            if (oldTarget != null && targetGroups.ContainsKey(oldTarget))
            {
                targetGroups[oldTarget].Remove(characterInstance.GetInstanceID());
            }
            if (target != null)
            {

                if (!targetGroups.ContainsKey(target))
                {
                    targetGroups[target] = new SortedList<int, GameObject>();
                }
                targetGroups[target].Add(characterInstance.GetInstanceID(), characterInstance);
            }
            oldTarget = target;
        }

        if (target == null)
        {
            return characterInstance.transform.position;
        }

        int index = targetGroups[target].IndexOfKey(characterInstance.GetInstanceID());
        int count = targetGroups[target].Count;

        Vector3 currentOffset = agent.nextPosition - target.position;

        currentOffset.y = 0f;
        float currentDistance = currentOffset.magnitude;
        Vector3 targetOffset = Vector3.zero;



        if (count == 1)
        {

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
        }
        else if (count >= 2)
        {
            
            Transform leftChar = targetGroups[target].ElementAt((index+count - 1) % (count)).Value.transform;
            Transform rightChar = targetGroups[target].ElementAt((index+count + 1) % (count)).Value.transform;
            Vector3 dirLeft = (leftChar.position - target.position).normalized;
            Vector3 dirRight = (rightChar.position - target.position).normalized;
            Quaternion left = Quaternion.LookRotation(dirLeft);
            Quaternion right = Quaternion.LookRotation(dirRight);
            Quaternion middle = Quaternion.Euler(0, left.eulerAngles.y+((right.eulerAngles.y-left.eulerAngles.y+359.9f)%360)/2, 0);
            Vector3 dirCenter = middle * Vector3.forward;
            targetOffset = dirCenter;
            
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
        public ButtonPress buttonPress;
        public float Angle;
        public float MaxDistance;//
        public float MinDistance;// this will generaly not be set by mele characters unless they need to exhibit cowardly like behaviors.
      
        public float BaseWaitTime;
        public float WaitTimeRandomize;
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
        
        if (t == null||!t.gameObject.activeInHierarchy)
        {
            return target.defaultPoint;
        }
        else 
        {
            return t.transform;
        } 

    }

}

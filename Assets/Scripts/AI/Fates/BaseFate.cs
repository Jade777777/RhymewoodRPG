using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class BaseFate : MonoBehaviour
{
    [SerializeField]
    protected GameObject characterInstance;
    protected CharacterNerveCenter cnc;
    protected NavMeshAgent agent;

    protected virtual void Awake()
    {
        cnc = characterInstance.GetComponent<CharacterNerveCenter>();
        agent = GetComponent<NavMeshAgent>();
        agent.radius = characterInstance.GetComponent<CharacterController>().radius  +0.1f;
    }


    protected enum TargetType { PatrolPoint, ClosestHostile, ClosestNuetral, ClosestFriendly, ClosestCharacter, HighestAgro, CurrentEnemy, FriendlysHighestAgro }
    //Add ClosestHidingSpot, RestSpot (safe campfires and things like that) These can also be added  through patrol points as well to test basic functionality
    protected enum ButtonPress { None, Attack, Kick, Jump, Interact}
}

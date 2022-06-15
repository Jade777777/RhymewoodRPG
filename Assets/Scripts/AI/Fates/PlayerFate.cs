using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class PlayerFate : MonoBehaviour
{
    [SerializeField]
    GameObject characterInstance;
    Vector3 spawnPoint;
    NavMeshAgent agent;


    string behavior = "Default";


    CharacterNerveCenter cnc;
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        spawnPoint = characterInstance.transform.position;
        agent.radius = characterInstance.GetComponent<CharacterController>().radius;
    }

    private void UpdateIntention()
    {
        behavior = cnc.GetBehavior();
    }



    private void Start()
    {
        cnc = characterInstance.GetComponent<CharacterNerveCenter>();
        cnc.IsPlayer = true;
        agent.updatePosition = false;
        agent.speed = cnc.PrimalStats()["Move Speed"];
    }


    private void LateUpdate()
    {

        agent.nextPosition = characterInstance.transform.position;
    }

    public void UpdateBehavior()
    {
        switch (behavior)
        {
            case "Default":
                inControl = true;
                break;
            case "Other":
                inControl = false;
                break;


        }
    }



    #region Player Input
    bool inControl = true;

    public void OnMove(InputValue input)
    {
        if (inControl)
        {
            cnc.Move(input.Get<Vector2>());
        }
    }
    public void OnLook(InputValue input)
    {
        if (inControl)
        { 
            cnc.PlayerLook(input.Get<Vector2>());
        }
    }

    public void OnJump(InputValue input)
    {
        if (inControl)
        {
            cnc.Jump(input.isPressed);
        }
    }
    public void OnAttack(InputValue input)
    {
        if (inControl)
        {
            cnc.Attack();
        }
    }
    public void OnKick(InputValue input)
    {
        if (inControl)
        {
            cnc.Kick();
        }
    }
    public void OnInteract(InputValue input)
    {
        if (inControl)
        {
            cnc.Interact();
        }
    }
    #endregion
}

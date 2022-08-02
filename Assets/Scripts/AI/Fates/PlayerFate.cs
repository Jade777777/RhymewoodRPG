using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class PlayerFate : BaseFate
{
    string behavior = "Default";


    protected override void Awake()
    {
        base.Awake();
        cnc.IsPlayer = true;
    }

    private void UpdateIntention()
    {
        behavior = cnc.GetBehavior();
    }



    private void Start()
    {

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

    public void OnSwapToWeapon1(InputValue input)
    {
        cnc.SwapToWeapon1();
    }
    public void OnSwapToWeapon2(InputValue input)
    {
        cnc.SwapToWeapon2();
    }
    public void OnSwapToWeapon3(InputValue input)
    {
        cnc.SwapToWeapon3();
    }
    #endregion
}

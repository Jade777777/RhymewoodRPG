using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorScriptControl : MonoBehaviour
{
    [SerializeField]
    string[] state;
    Animator animator;
    string currentState;
    MonoBehaviour currentScript;




    // Make this the devault source for the value of base state
    public BaseState.MovementType movementType = BaseState.MovementType.Ground;
    [Range(0f, 1f)]
    public float moveInputWeight = 1f;
    [Range(0f, 1f)]
    public float cameraInputWeight = 1f;
    [Range(0f, 1f)]
    public float cameraAnimationWeight = 0.2f;
    public float turnSpeed = 360;
    public float smoothMoveInput = 4f;










    private void Start()
    {
        MonoBehaviour[] s = GetComponents<BaseState>();
        state = new string[s.Length];
        for (int i = 0; i < s.Length; i++)
        {
            state[i] = s[i].GetType().ToString();
        }

        animator = GetComponent<Animator>();
        currentState = CheckState();
        currentScript = GetComponent(currentState) as MonoBehaviour;
        if (currentScript != null) currentScript.enabled = true;
    }
    private void LateUpdate()
    {
        string newState = CheckState();
        if (currentState != newState)
        {
            currentState = newState;
            currentScript.enabled = false;
            currentScript = GetComponent(currentState) as MonoBehaviour;
            if (currentScript != null)
            {
                currentScript.enabled = true;
                ResetCurrentStateVar();
            }
        }
    }
    private void ResetCurrentStateVar()
    {
        BaseState currentState = currentScript as BaseState;
        movementType = currentState.movementType;
        moveInputWeight = currentState.moveInputWeight;
        cameraInputWeight = currentState.cameraInputWeight;
        cameraAnimationWeight = currentState.cameraAnimationWeight;
        turnSpeed = currentState.turnSpeed;
        smoothMoveInput = currentState.smoothMoveInput;
        // Debug.Log("reset values!" +moveType);
    }


    private string CheckState()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName(currentState))
            return currentState;

        foreach (var st in state)
            if (animator.GetCurrentAnimatorStateInfo(0).IsName(st)) return st;

        Debug.LogError("Animator not in a recognized state!");
        return "";
    }

    //These values cannot be set during the first frame of the animation or they will be overriden by the default values.
    public void ASC_SetMovementType(BaseState.MovementType value)
    {
        movementType = value;
    }
    public void ASC_SetMoveInputWeight01(float value)
    {
        moveInputWeight = value;
    }
    public void ASC_SetCameraInputWeight01(float value)
    {
        cameraInputWeight = value;
    }
    public void ASC_SetCameraAnimationWeight01(float value)
    {
        cameraAnimationWeight = value;
    }
    public void ASC_SetTurnSpeed(float value)
    {
        turnSpeed = value;
    }
    public void ASC_SetSmoothMoveInput(float value)
    {
        smoothMoveInput = value;
    }

}


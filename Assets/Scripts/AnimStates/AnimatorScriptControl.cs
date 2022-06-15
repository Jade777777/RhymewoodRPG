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
    void Start()
    {
        MonoBehaviour[] s = GetComponents<BaseState>();
        state = new string[s.Length];
        for (int i = 0; i< s.Length; i++)
        {
            state[i] = s[i].GetType().ToString();
        }
        
        animator = GetComponent<Animator>();
        currentState = CheckState();
        currentScript = GetComponent(currentState) as MonoBehaviour;
        if (currentScript != null) currentScript.enabled = true;
    }
    void LateUpdate()
    {
        string newState = CheckState();
        if (currentState != newState)
        {
            currentState = newState;
            currentScript.enabled = false;
            currentScript = GetComponent(currentState) as MonoBehaviour;
            if(currentScript!=null) currentScript.enabled = true;
        }
    }
    string CheckState()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName(currentState)) 
            return currentState;

        foreach (var st in state)
            if (animator.GetCurrentAnimatorStateInfo(0).IsName(st)) return st;

        Debug.LogError("Animator not in a recognized state!");
        return "";
    }
}

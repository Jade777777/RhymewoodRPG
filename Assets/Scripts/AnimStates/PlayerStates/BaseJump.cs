using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseJump : BaseState
{
    
    protected override void Awake()
    {
        base.Awake();
        animator = GetComponent<Animator>();
    }
    protected override void OnEnable()
    {
        base.OnEnable();
        animator.SetFloat("CollisionAngle", 0f);
        base.characterController.slopeLimit = 180f;
    }
    protected override void OnDisable()
    {
        base.OnDisable();
        base.characterController.slopeLimit = 0f;
    }
}

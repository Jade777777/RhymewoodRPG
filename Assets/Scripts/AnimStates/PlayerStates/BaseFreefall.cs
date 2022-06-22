using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseFreefall : BaseState
{

    protected override void Awake()
    {
        base.Awake();
    }
    protected override void OnEnable()
    {
        base.OnEnable();
        base.characterController.slopeLimit = 180f;
    }
    protected override void OnDisable()
    {
        base.OnDisable();
        base.characterController.slopeLimit = 0f;
    }
}

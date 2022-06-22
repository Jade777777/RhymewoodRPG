using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseFalling : BaseState
{
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
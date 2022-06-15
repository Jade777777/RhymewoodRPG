using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDrop : BaseState
{
    protected override void OnEnable()
    {
        base.OnEnable();
        base.characterController.slopeLimit = 40f;
    }
    protected override void OnDisable()
    {
        base.OnDisable();
        base.characterController.slopeLimit = 0f;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoInteract : BaseState
{

    protected override void OnDisable()
    {
        cameraTarget.localRotation = Quaternion.identity;
        physicalInput.moveInput = physicalInput.Velocity;
    }
}

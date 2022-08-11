using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseInteract : BaseState
{
    protected virtual void Update()
    {
    }
    private void LateUpdate()
    {
        if (cnc.IsPlayer)//if the character is being controlled by the player as of Awake
        {
            cameraTarget.localRotation = Quaternion.identity;
            Vector3 camPos = head.position + cameraTarget.TransformDirection(cOffset);
            camPos = head.InverseTransformPoint(camPos); // clamp the local values
            camPos.y = Mathf.Clamp(camPos.y, -cOffset.z, float.PositiveInfinity) - 0.2f;
            camPos.z = Mathf.Clamp(camPos.z, -0.15f, float.PositiveInfinity);
            camPos = head.TransformPoint(camPos); // change them back to global

            Quaternion weightedHeadRotation = head.rotation * Quaternion.Inverse(transform.rotation);
            weightedHeadRotation = Quaternion.Lerp(Quaternion.identity, weightedHeadRotation, animatorScriptControl.cameraAnimationWeight);
            Quaternion camRot = weightedHeadRotation * cameraTarget.rotation;

            cameraController.SetPositionAndRotation(camPos, camRot);
        }

    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Quaternion rotationTarget;
    private Vector3 positionTarget;
    private Vector3 totalOffsetPosition = Vector3.zero;
    private Quaternion totalOffsetRotation = Quaternion.identity;
    public void OffsetCamera(Vector3 offsetPosition, Vector3 offsetRotation)
    {
        offsetPosition = transform.rotation*offsetPosition;
        totalOffsetPosition += offsetPosition;
        totalOffsetRotation *= Quaternion.Euler(offsetRotation);
    }

    public void SetPositionAndRotation(Vector3 position, Quaternion rotation) // this should be called once per late update
    {
        rotationTarget = rotation * totalOffsetRotation;
        positionTarget = position + totalOffsetPosition;
        //transform.SetPositionAndRotation(position + totalOffsetPosition, rotation*totalOffsetRotation);

        totalOffsetPosition = Vector3.zero;
        totalOffsetRotation = Quaternion.identity;
    }
    private void LateUpdate()
    {
        transform.position = positionTarget;
        transform.rotation = JadeMath.EaseQuaternion(transform.rotation, rotationTarget, 0.999999f, Time.deltaTime);// you can tell this has an effect because we can look at the players body.
    }
}

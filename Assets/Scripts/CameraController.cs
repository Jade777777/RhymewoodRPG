using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private const float ease = 0.999999f;
    private const float tiltMultiplier = 1;
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

    public void SetPositionAndRotation(Vector3 position, Quaternion rotation,Vector3 velocity) // this should be called once per late update
    {
        Vector3 rollDirection = Vector3.Project(velocity, transform.right);
        float rollMagnitude = rollDirection.magnitude* -Mathf.Sign(Vector3.Dot(rollDirection, transform.right));

        Quaternion roll = Quaternion.AngleAxis(rollMagnitude*tiltMultiplier,Vector3.forward); 

        rotationTarget = rotation * totalOffsetRotation * roll;
        positionTarget = position + totalOffsetPosition;

        totalOffsetPosition = Vector3.zero;
        totalOffsetRotation = Quaternion.identity;
    }
    private void LateUpdate()
    {
        transform.position = positionTarget;
        transform.rotation = JadeMath.EaseQuaternion(transform.rotation, rotationTarget, ease, Time.deltaTime);// you can tell this has an effect because we can look at the players body.
    }
}

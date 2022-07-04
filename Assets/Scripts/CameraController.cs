using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{

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
        transform.SetPositionAndRotation(position + totalOffsetPosition, rotation*totalOffsetRotation);
        totalOffsetPosition = Vector3.zero;
        totalOffsetRotation = Quaternion.identity;
    }
}

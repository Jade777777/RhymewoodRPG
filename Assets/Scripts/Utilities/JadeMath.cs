using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JadeMath
{
    public static float ClampAngle(float angle, float from, float to)
    {
        if (angle < 0f) angle = 360 + angle;
        if (angle > 180f) return Mathf.Max(angle, 360 + from);
        return Mathf.Min(angle, to);
    }

    public static Quaternion EaseQuaternion(Quaternion currentRotation, Quaternion targetRotation, float beta, float deltaTime)
    {
        float t = 1 - Mathf.Pow(1 - beta, deltaTime);
        return Quaternion.Slerp(currentRotation, targetRotation, t);
    }
    public static Vector3 EaseVector3(Vector3 currentVector, Vector3 targetVector, float beta, float deltaTime)
    {
        float t = 1 - Mathf.Pow(1 - beta, deltaTime);
        return Vector3.Lerp(currentVector, targetVector, t);
    }

}

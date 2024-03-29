using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ScreenShake
{

    static CameraController cameraController;

    static float max = 10f;
    static float apex = 0.2f;
    public static void Shake(float time, float maxAngle,float frequency)
    {
       
        if(cameraController == null)
        {
            cameraController = Camera.main.GetComponent<CameraController>();
        }
        StaticCoroutine.Start(ShakeProcess(time,maxAngle,frequency));

    }
    
    static IEnumerator ShakeProcess(float time, float maxAngle, float frequency)
    {
        maxAngle = Mathf.Clamp(maxAngle, 0, max);
        float position;
        
        float startTime = Time.time;
        float apexTime = Time.time + (time * apex);
        float endTime =  Time.time + time;


        yield return null;

        while (endTime>Time.time) {

            float fade;
            if (apexTime >= Time.time)// increas linear
            {
                fade =  Mathf.InverseLerp(startTime, apexTime, Time.time);
            }
            else
            {
                fade = 1-Mathf.InverseLerp(apexTime, endTime, Time.time);
                fade *= fade;//exponential fall off
            }
            position = frequency * Time.time;
     

            Vector3 offsetRotation = new Vector3(Mathf.PerlinNoise(position, 10f) - 0.5f,
                                                  Mathf.PerlinNoise(position, 20f) - 0.5f,
                                                  Mathf.PerlinNoise(position, 30f) - 0.5f) *maxAngle;
            

            cameraController.OffsetCamera(Vector3.zero, offsetRotation*fade);
            yield return null;
        }
        Debug.Log("It's working!");

    }
}

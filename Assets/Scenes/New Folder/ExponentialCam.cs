using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ExponentialCam : MonoBehaviour
{
    Camera baseCamera;
    public GameObject additionalCamera;
    Camera[] exponential;
    public int steps;
    public float minFOV;
    public float maxFOV;
    float farClipPlain;
    float nearClipPlain;
    void Start()
    {
        baseCamera = GetComponent<Camera>();
        exponential = new Camera[steps];

        farClipPlain = baseCamera.farClipPlane;
        nearClipPlain = baseCamera.nearClipPlane;

        baseCamera.farClipPlane= (farClipPlain - nearClipPlain) / steps  + nearClipPlain;

        var cameraData = baseCamera.GetUniversalAdditionalCameraData();
        
 
        //for (int i =0; i <steps; i++)
        {
            int i = 0;
            float prevFov = (maxFOV - minFOV) / steps * (i) + minFOV;
            float thisFov = (maxFOV - minFOV) / steps * (i) + minFOV;
            float nearClip = (farClipPlain - nearClipPlain) / steps * (i) + nearClipPlain;
            
            float width = nearClip * Mathf.Tan(prevFov / 2) * 2;// get the width of the right triangle mutliplied by 2;

            float camPos = width / 2 * Mathf.Tan(thisFov / 2);

            //float farClip = (farClipPlain - nearClipPlain) / steps * (i + 1) + nearClipPlain;
            
            

            
            exponential[i] = Instantiate(additionalCamera,gameObject.transform).GetComponent<Camera>();
            exponential[i].transform.localPosition = new Vector3(0, 0, nearClip- camPos);
            exponential[i].transform.localRotation = Quaternion.identity;
            exponential[i].nearClipPlane = camPos;
            cameraData.cameraStack.Add(exponential[i]);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

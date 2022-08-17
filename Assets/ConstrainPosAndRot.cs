using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstrainPosAndRot : MonoBehaviour
{
    public Transform target;
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.position = target.position;
        transform.rotation = target.rotation;
    }
}

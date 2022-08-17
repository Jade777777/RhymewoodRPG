using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
public class OrientArmsToCamera : MonoBehaviour
{
    CharacterNerveCenter cnc;
    public Rig runtimePlayerRig;
    public OverrideTransform overrideTransform;
   
    public Transform cameraTarget;
    [Range(0,1)]
    public float OrientPlayerArms = 0;

    private void Awake()
    {
        cnc = GetComponent<CharacterNerveCenter>();
        cameraTarget = new List<GameObject>(GameObject.FindGameObjectsWithTag("CameraTarget")).Find(g => g.transform.IsChildOf(this.transform)).transform;
    }
    private void Start()
    {
        if (cnc.IsPlayer == false)
        {
            runtimePlayerRig.weight = 0;
            enabled = false;
        }
        else
        {
            runtimePlayerRig.weight = 1;
        }
    }
    
    private void Update()
    {
        if (cnc.IsPlayer == true)
        {
            runtimePlayerRig.weight = OrientPlayerArms;
            Vector3 overrideRotation = cameraTarget.localRotation.eulerAngles;
            overrideRotation.y = 0;
            overrideRotation.z = 0;
           
            overrideTransform.data.rotation = overrideRotation;
            
        }
    }
}

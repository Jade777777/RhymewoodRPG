using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
public class OrientArmsToCamera : MonoBehaviour
{
    CharacterNerveCenter cnc;
    Animator animator;
    public Rig runtimePlayerRig;
    public OverrideTransform overrideTransform;
    public Transform cameraTarget;

    private void Awake()
    {
        cnc = GetComponent<CharacterNerveCenter>();
        animator = GetComponent<Animator>();
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
        overrideTransform.data.rotation = cameraTarget.localRotation.eulerAngles;
    }

}

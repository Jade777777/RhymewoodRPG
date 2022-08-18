using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterHurtBox : HurtBox
{
    private CharacterNerveCenter cnc;
    private float characterRadius;


    protected override void Awake()
    {
        base.Awake();
        cnc = JadeUtility.GetComponentInParents<CharacterNerveCenter>(transform);
        sourceObject = cnc.gameObject;
        characterRadius = JadeUtility.GetComponentInParents<CharacterController>(transform).radius;
    }
    private void OnEnable()
    {
        GetComponent<Collider>().isTrigger = true;
        GetComponent<Rigidbody>().isKinematic = true;
    }

    protected override void ProcessDamage(HitBox hitBox)
    {
        cnc.SruckByHitBox(hitBox, this);
    }

    public override float CenterToBounds(Transform position)
    {
        return characterRadius;
    }


}

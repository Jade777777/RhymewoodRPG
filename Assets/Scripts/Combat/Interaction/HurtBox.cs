using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class HurtBox : MonoBehaviour
{
    public CharacterNerveCenter cnc { get; private set; }
    public float characterRadius { get; private set; }
    private void Awake()
    {
        Debug.Assert(GetComponent<Collider>().isTrigger == true);
        Debug.Assert(GetComponent<Rigidbody>().isKinematic == true);

        cnc = JadeUtility.GetComponentInParents<CharacterNerveCenter>(transform);
        characterRadius = JadeUtility.GetComponentInParents<CharacterController>(transform).radius;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out HitBox hitBox) && hitBox.cnc != cnc) 
        {
            cnc.SruckByHitBox(hitBox, this);
        }
    }

}

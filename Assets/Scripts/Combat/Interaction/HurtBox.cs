using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// TODO: Make HurtBox an abstract class. 
/// CNC and awake should be determined by
/// the derived classes. The HurtBox will 
/// need to be adjusted to deal with CNC
/// not always being present.
/// </summary>

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

    public void TakeDamage(HitBox hitBox)
    {
        cnc.SruckByHitBox(hitBox, this);
    }

}

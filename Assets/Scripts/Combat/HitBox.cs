using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitBox : MonoBehaviour
{
    [SerializeField]
    private WeaponWarden weaponWarden;
    [SerializeField]
    bool allQuirks= true;
    [SerializeField]
    List<string> damageQuirks;
    [SerializeField]
    private float multiplier = 1f;

    private PhysicalInput physicalInput;


    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<HurtBox>(out HurtBox hit) && other.transform.parent != transform.parent)
        {
            print("deal damage");
            gameObject.SendMessageUpwards("DealDamage",other.transform.parent);

        }
    }
}

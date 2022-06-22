using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class HitBox : MonoBehaviour
{


    [SerializeField]
    bool allQuirks= true;
    [SerializeField]
    List<string> damageQuirks;
    [SerializeField]
    private float multiplier = 1f;
    public KnockbackType knockbackType;

    private WeaponWarden weaponWarden;
    public CharacterNerveCenter cnc { get; private set; }
    private void Awake()
    {
        weaponWarden = JadeUtility.GetComponentInParents<WeaponWarden>(transform);
        cnc = JadeUtility.GetComponentInParents<CharacterNerveCenter>(transform);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<HurtBox>(out HurtBox hurtBox) && hurtBox.cnc != cnc)
        {
            cnc.StrikeHurtBox(hurtBox);
        }
    }

    public ReadOnlyDictionary<string, float> GetDamage()
    {
        return weaponWarden.WeaponTotalDamage();
    }
}
public enum KnockbackType { Light, Medium, Heavy, Ragdoll}

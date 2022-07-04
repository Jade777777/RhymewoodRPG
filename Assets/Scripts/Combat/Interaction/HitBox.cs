using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class HitBox : MonoBehaviour
{


    [SerializeField]
    bool allQuirks= true;
    [SerializeField]
    List<string> damageQuirks;
    [SerializeField]
    private float multiplier = 1f;
    [SerializeField]
    private float knockbackDistance=0f;//the distance the enemy is thrown back after allignment calculations
    [SerializeField]
    private float takeGround = 0.2f;//the distance both characters move forward on a strike

    public KnockbackType knockbackType;



    private EquipedWeapon equipedWeapon;
    public CharacterNerveCenter cnc { get; private set; }
    public float characterRadius { get; private set; }
    private float engagementDistance;
    private void Awake()
    {
        Debug.Assert(GetComponent<Collider>().isTrigger == true);
        Debug.Assert(GetComponent<Rigidbody>().isKinematic == true);

        equipedWeapon = JadeUtility.GetComponentInParents<EquipedWeapon>(transform);
        cnc = JadeUtility.GetComponentInParents<CharacterNerveCenter>(transform);
        characterRadius = JadeUtility.GetComponentInParents<CharacterController>(transform).radius;
        engagementDistance = equipedWeapon.equipedWeapon.weapon.engagementDistance; 
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<HurtBox>(out HurtBox hurtBox) && hurtBox.cnc != cnc)
        {
            
            cnc.StrikeHurtBox(this ,hurtBox);
        }
    }

    public void GetKnockbackDistance(HurtBox hurtBox, out Vector3 enemyKnockback, out Vector3 selfKnockback)
    {
        Vector3 currentOffset = hurtBox.cnc.transform.position - cnc.transform.position;
        Vector3 direction = currentOffset.normalized;
        
        float currentDistance = currentOffset.magnitude-(hurtBox.characterRadius+characterRadius);

        Vector3 facing =  cnc.transform.forward;

        float desiredChange = engagementDistance - currentDistance;


        if (currentDistance < engagementDistance)
        {
            selfKnockback = (-desiredChange * 0.5f)* direction + (takeGround)* facing;
            enemyKnockback = (desiredChange * 0.5f)* direction + (knockbackDistance + takeGround) * facing;
            
        }
        //else // I'm not sure if i want this, it might make it too easy to chase down enemies?
        //{
        //    selfKnockback = (-desiredChange + takeGround) * direction;
        //    enemyKnockback = ( 0f +knockbackDistance + takeGround) * direction;
        //}
        else
        {
            selfKnockback = (0f) * direction + (takeGround) * facing;
            enemyKnockback = (0f) * direction + (knockbackDistance + takeGround) * facing;
        }
    }

    public ReadOnlyDictionary<string, float> GetDamage()
    {
        return equipedWeapon.WeaponTotalDamage();
    }
}
public enum KnockbackType { Light, Medium, Heavy, Ragdoll}

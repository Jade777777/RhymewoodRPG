using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;


public class WeaponHitBox : HitBox
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
    [SerializeField]
    private int poiseDamage = 5;
    [SerializeField]
    private KnockdownType knockdownType = KnockdownType.Stagger;



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
        
        
    }
    private void Start()
    {
        characterRadius = JadeUtility.GetComponentInParents<CharacterController>(transform).radius;
        engagementDistance = equipedWeapon.weaponInstance.weapon.engagementDistance;
    }

    protected override void OnStrikeHurtBox(HurtBox hurtBox)
    {
        if (hurtBox.cnc != cnc)
        {
            hurtBox.TakeDamage(this);
            cnc.StrikeHurtBox(this, hurtBox);
        }
    }


    public override void GetKnockbackDistance(HurtBox hurtBox, out Vector3 enemyKnockback, out Vector3 selfKnockback)
    {
        Vector3 currentOffset = hurtBox.cnc.transform.position - cnc.transform.position;
        Vector3 direction = currentOffset.normalized;
        Vector3 facing = cnc.transform.forward;
        float currentDistance = currentOffset.magnitude-(hurtBox.characterRadius+characterRadius);
        float desiredChange = engagementDistance - currentDistance;

        if (currentDistance < engagementDistance)
        {
            selfKnockback = (-desiredChange * 0.5f)* direction + (takeGround)* facing;
            enemyKnockback = (desiredChange * 0.5f)* direction + (knockbackDistance + takeGround) * facing;
            
        }
        else
        {
            selfKnockback = (0f) * direction + (takeGround) * facing;
            enemyKnockback = (0f) * direction + (knockbackDistance + takeGround) * facing;
        }
    }

    public override ReadOnlyDictionary<string, float> GetDamage()
    {
        Dictionary<string, float> damage = new();
        ReadOnlyDictionary<string, float> totalDamage = equipedWeapon.WeaponTotalDamage();
        if (allQuirks == true)
        {
            foreach(KeyValuePair<string, float> kvp in totalDamage)
            {
                 damage.Add(kvp.Key, kvp.Value * multiplier);
            }
        }
        else
        {
            foreach(string quirkName in damageQuirks)
            {
                if (totalDamage.ContainsKey(quirkName))
                {
                    damage.Add(quirkName, totalDamage[quirkName]);
                }
            }

        }
        return new ReadOnlyDictionary<string, float>(damage);
    }
    public override int GetPoiseDamage()
    {
        return poiseDamage;
    }
    public override KnockdownType GetKnockdownType()
    {
        return knockdownType;
    }
}


using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEditor;
using UnityEngine;

public class EquipedWeapon : MonoBehaviour
{
    AnimatorOverrideController animatorOverrideController;
    [SerializeField]
    Weapon weapon;
    [SerializeField]
    int level;
    [SerializeField]
    WeaponInfusion infusion;
    public WeaponInstance weaponInstance { get; private set; }

    [SerializeField]
    private Transform primaryWeaponJoint;
    [SerializeField]
    private Transform secondaryWeaponJoint;

    private GameObject primaryModelInstance;
    private GameObject secondaryModelInstance;

    private List<GameObject> attackModelInstance = new();
    private CharacterStats characterStats;

    private List<KeyValuePair<AnimationClip, AnimationClip>> defaultAnimationOverrides;
    private List< KeyValuePair<AnimationClip, AnimationClip> > animationOverrides = new();

    private static int countID=0;
    private int ID;
    private void Awake()
    {
        ID = countID;
        countID++;
        characterStats = GetComponent<StatWarden>().characterStats;
        weaponInstance = new(weapon, level, infusion);
    }
    void Start()
    {
        Animator animator = GetComponent<Animator>();
        animatorOverrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
        animator.runtimeAnimatorController = animatorOverrideController;

        defaultAnimationOverrides = new List<KeyValuePair<AnimationClip, AnimationClip>>(animatorOverrideController.overridesCount);
        animatorOverrideController.GetOverrides(defaultAnimationOverrides);

        EquipWeapon(weaponInstance);

    }


    
    public void EquipWeapon(WeaponInstance weaponInstance)
    {
        Debug.Assert(weaponInstance != null);

        //cleanup previous weapon models
        CleanUpWeapon();
        //Equip the weapon
        SetWeapon(weaponInstance);
    }

    private void SetWeapon(WeaponInstance weaponInstance)
    {
        //TODO:
        //save weapon infusion.?

        this.weaponInstance = weaponInstance;//save the value of the new weapon
        //instantiate the new weapon model
        primaryModelInstance = Instantiate(this.weaponInstance.weapon.primaryWeaponModel, primaryWeaponJoint);
        primaryModelInstance.name = this.weaponInstance.name + " " + ID;
        secondaryModelInstance = Instantiate(this.weaponInstance.weapon.secondaryWeaponModel, secondaryWeaponJoint);
        secondaryModelInstance.name = this.weaponInstance.name + " " + ID;
        //instantiate the new attack models used by the new animations;

        foreach (Attack attack in this.weaponInstance.weapon.attacks)
        {
            bool SkipSpawn= false;
            foreach(GameObject instance in attackModelInstance)
            {
                SkipSpawn = SkipSpawn || (instance.name == attack.attackAnimation.overrideClip.name);
            }
            if (!SkipSpawn)
            {
                GameObject attackModel = Instantiate(attack.attackModel, transform);
                attackModel.name = attack.attackAnimation.overrideClip.name; //attack.attackModel.name;
                attackModelInstance.Add(attackModel);
            }

            AnimationClip attackOverrideClip = attack.attackAnimation.overrideClip;
            KeyValuePair<AnimationClip, AnimationClip> attackAnimationPair = new(attack.attackAnimation.originalClip, attackOverrideClip);
            animationOverrides.Add(attackAnimationPair);
        }

        animatorOverrideController.ApplyOverrides(animationOverrides);
        //Get the weapons hitboxes
        RetrievePrimaryHitboxes();
        RetrieveSecondaryHitboxes();
        //set the stats for the current weapon
        UpdateWeaponStats();
    }
    private void CleanUpWeapon()
    {
        if (primaryModelInstance != null)
        {
            Destroy(primaryModelInstance);
        }
        if (secondaryModelInstance != null)
        {
            Destroy(secondaryModelInstance);
        }
        foreach (GameObject attackModel in attackModelInstance)//it's destroyed at the end of the frame so we don't need to loop backwards
        {
            Destroy(attackModel);
        }
        //clear animation overrides;
        animationOverrides = new();
        attackModelInstance = new();
    }




    #region Weapon Hitbox
    private Component[] PrimaryHitBoxes;
    private Component[] SecondaryHitBoxes;
    private void RetrievePrimaryHitboxes()
    {

        PrimaryHitBoxes = primaryModelInstance.GetComponentsInChildren<WeaponHitBox>(true);
        foreach(WeaponHitBox hitBox in PrimaryHitBoxes)
        {
            hitBox.gameObject.SetActive(false);
        }
        Debug.Log("Got some hitboxes here");
    }
    private void RetrieveSecondaryHitboxes()
    {

        SecondaryHitBoxes = secondaryModelInstance.GetComponentsInChildren<WeaponHitBox>(true);
        foreach (WeaponHitBox hitBox in SecondaryHitBoxes)
        {
            hitBox.gameObject.SetActive(false);
        }
        Debug.Log("Got some hitboxes here");
    }
    public void EW_ActivateWeaponHitBox(int i)
    {
        if (i == 0)
        {
            foreach (WeaponHitBox hitBox in PrimaryHitBoxes)
            {
                hitBox.gameObject.SetActive(true);
            }
        }
        else if (i == 1)
        {
            foreach (WeaponHitBox hitBox in SecondaryHitBoxes)
            {
                hitBox.gameObject.SetActive(true);
            }
        }
    }
    public void EW_DisableWeaponHitBox(int i)
    {
        if (i == 0) 
        {
            foreach (WeaponHitBox hitBox in PrimaryHitBoxes)
            {
                hitBox.gameObject.SetActive(false);
            } 
        }
        else if(i == 1)
        {
            foreach (WeaponHitBox hitBox in SecondaryHitBoxes)
            {
                hitBox.gameObject.SetActive(false);
            }
        }
    }
    #endregion






    public ReadOnlyDictionary<string, float> WeaponTotalDamage()
    {
        return new ReadOnlyDictionary<string, float>(weaponTotalDamage.Stats());
    }
    public ReadOnlyDictionary<string, float> WeaponBaseDamage()
    {
        return new ReadOnlyDictionary<string, float>(weaponBaseDamage.Stats());
    }
    public ReadOnlyDictionary<string, float> WeaponBonusDamage()
    {
        return new ReadOnlyDictionary<string, float>(weaponBonusDamage.Stats());
    }






    public void UpdateWeaponStats()//use delegate to call this method  when the player stats updats. The weaponWarden needs to subscribe to a delegate.
    {
        baseFlatDamage = new();
        weaponBaseDamage = new();// this is scaled with the level
        weaponPrimalAdjustedValue = new();
        weaponBonusDamage = new();
        weaponTotalDamage = new();
        //----------------------------------------Base Damage---------------------------------------
        weaponBaseDamage.AddReferenceOnlyStat(baseFlatDamage);
        foreach (BaseQuirk bq in weaponInstance.weapon.baseQuirk)
        {
            baseFlatDamage.Add(new FlatStatLeaf(bq.quirkType, bq.baseDamage));
        }
        foreach (KeyValuePair<string, float> bqa in weaponInstance.weaponInfusion.BaseQuirkAdjust())
        {
            baseFlatDamage.Add(new FlatStatLeaf(bqa.Key, bqa.Value));
        }
        foreach (KeyValuePair<string, AnimationCurve> wbls in weaponInstance.weaponInfusion.BaseQuirkLevelScaling())
        {
            weaponBaseDamage.Add(new MultStatLeaf(wbls.Key, wbls.Key, wbls.Value.Evaluate(weaponInstance.level)));
        }
        foreach (KeyValuePair<string, float> kvp in weaponBaseDamage.Stats())
        {
            Debug.Log("The base " + kvp.Key + " Quirk of this weapon is:   " + kvp.Value);
        }
        //--------------------------------------------------------------------------------------------


        //---------------------------------Calculate the primal bonus---------------------------------
        foreach (KeyValuePair<string, float> stat in characterStats.PrimalStats())
        {
            weaponPrimalAdjustedValue.AddReferenceOnlyStat(new FlatStatLeaf(stat.Key, stat.Value));
        }
        foreach (KeyValuePair<string, AnimationCurve> psb in weaponInstance.weaponInfusion.PrimalScalingBonus())
        {
            weaponPrimalAdjustedValue.Add(new MultStatLeaf(psb.Key, psb.Key, psb.Value.Evaluate(weaponInstance.level)));
        }

        foreach (KeyValuePair<string, float> kvp in weaponPrimalAdjustedValue.Stats())
        {
            Debug.Log("the adjusted primal value for" + kvp.Key + " is   " + kvp.Value);
        }

        weaponBonusDamage.AddReferenceOnlyStat(new BatchStatLeaf(weaponPrimalAdjustedValue));

        foreach (BaseQuirk bq in weaponInstance.weapon.baseQuirk)
        {
            foreach (BasePrimalScaling bps in bq.primalScaling)
            {
                weaponBonusDamage.Add(new MultStatLeaf(bq.quirkType, bps.primalStat, bps.baseScaling));
                Debug.Log(bq.quirkType + bps.primalStat + bps.baseScaling);
            }
        }
        foreach (KeyValuePair<string, float> kvp in weaponBonusDamage.Stats())
        {
            Debug.Log("The primal bonus for " + kvp.Key + " Quirk of this weapon is:   " + kvp.Value);
        }


        //-----------------------------------------------------------------------------------------------

        weaponTotalDamage.Add(new BatchStatLeaf(weaponBaseDamage));
        weaponTotalDamage.Add(new BatchStatLeaf(weaponBonusDamage));
        foreach (KeyValuePair<string, float> kvp in weaponTotalDamage.Stats())
        {
            Debug.Log("The total damage for " + kvp.Key + " Quirk of this weapon is:   " + kvp.Value);
        }
        //-------------------------------------Final Damage----------------------------------------------
    }


    //------used for calcualtions-----------------------------
    private StatComposite baseFlatDamage;
    private ReferenceStatComposite weaponPrimalAdjustedValue;
    //--------------------------------------------------------

    //-------Final Values-------------------------------------
    private ReferenceStatComposite weaponBaseDamage;
    private ReferenceStatComposite weaponBonusDamage;
    private StatComposite weaponTotalDamage;
    //--------------------------------------------------------
    // Base damage is equal to WeaponBaseDamage,  Bonus Damage is equal to weaponBaseDamage*primalQuirkBonus
    //total damage is just WeaponBaseDamage+weaponTotalDamage;

    [System.Serializable]
    public struct BaseQuirk// The base damage of the weapon
    {
        public string quirkType;// The name of the present damage type (ex. Physical, Fire, Magic)
        public float baseDamage;
        public List<BasePrimalScaling> primalScaling;
    }
    [System.Serializable]
    public struct BasePrimalScaling// the base scaling of the weapon, scales directly with primal stats;
    {
        public string primalStat;// The stat name that it scales with (ex. strength, dexterety)
        public float baseScaling;// Flat% valuethis value is multiplied with the WeaponInfusion.Scaling[level] to determine the total scaling
        //If a base Scaling percent isn't present, that stat doesn't scale with that type of quirk on the weapon; the infusion must account for all damage types;
    }



}

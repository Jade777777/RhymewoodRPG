using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public class WeaponWarden : MonoBehaviour
{
    //The level of the weapon, each instance of a weapon will have it's own level
    [SerializeField]
    private float level;

    //The base values are unique to every weapon prefab.They never change throughout the game
    [SerializeField]
    private List<BaseQuirk> baseQuirk;

    //These values are shared between various weapons and dependant on the enchantment the weapons are imbued with.
    [SerializeField]
    private WeaponInfusion weaponInfusion;//adjust base damage, adjust base scaling



    //
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


    private void Awake()
    {
        characterStats = JadeUtility.GetComponentInParents<StatWarden>(transform).characterStats;
        UpdateWeaponStats();
    }

    public void Start()
    {
        UpdateWeaponStats();
    }
    public void InfuseWeapon(WeaponInfusion weaponInfusion)
    {
        this.weaponInfusion = weaponInfusion;
        UpdateWeaponStats();
    }
    public void SetCharacterStats(CharacterStats characterStats)
    {
        this.characterStats = characterStats;
        UpdateWeaponStats();//use delegate to set weapon stats whenever the player gains new stat points.
    }
    public void LevelUp()
    {
        level++;
        UpdateWeaponStats();
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
        foreach(BaseQuirk bq in baseQuirk)
        {
            baseFlatDamage.Add(new FlatStatLeaf(bq.quirkType, bq.baseDamage));
        }
        foreach (KeyValuePair<string, float> bqa in weaponInfusion.BaseQuirkAdjust())
        {
            baseFlatDamage.Add(new FlatStatLeaf(bqa.Key, bqa.Value));
        }
        foreach(KeyValuePair<string, AnimationCurve> wbls in weaponInfusion.BaseQuirkLevelScaling())
        {
            weaponBaseDamage.Add(new MultStatLeaf(wbls.Key, wbls.Key, wbls.Value.Evaluate(level)));
        }
        foreach (KeyValuePair<string, float> kvp in weaponBaseDamage.Stats())
        {
            Debug.Log("The base " +kvp.Key + " Quirk of this weapon is:   " + kvp.Value);
        }
        //--------------------------------------------------------------------------------------------


        //---------------------------------Calculate the primal bonus---------------------------------
        foreach (KeyValuePair<string, float> stat in characterStats.PrimalStats()) 
        {
            weaponPrimalAdjustedValue.AddReferenceOnlyStat(new FlatStatLeaf(stat.Key, stat.Value));
        }
        foreach (KeyValuePair<string, AnimationCurve> psb in weaponInfusion.PrimalScalingBonus())
        {
            weaponPrimalAdjustedValue.Add(new MultStatLeaf(psb.Key, psb.Key, psb.Value.Evaluate(level)));
        }

        foreach (KeyValuePair<string, float> kvp in weaponPrimalAdjustedValue.Stats())
        {
            Debug.Log("the adjusted primal value for" + kvp.Key + " is   " + kvp.Value);
        }

        weaponBonusDamage.AddReferenceOnlyStat(new BatchStatLeaf(weaponPrimalAdjustedValue));

        foreach (BaseQuirk bq in baseQuirk)
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

    private CharacterStats characterStats;
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
    private struct BaseQuirk// The base damage of the weapon
    {
        public string quirkType;// The name of the present damage type (ex. Physical, Fire, Magic)
        public float baseDamage;
        public List<BasePrimalScaling> primalScaling;
    }
    [System.Serializable]
    private struct BasePrimalScaling// the base scaling of the weapon, scales directly with primal stats;
    {
        public string primalStat;// The stat name that it scales with (ex. strength, dexterety)
        public float baseScaling;// Flat% valuethis value is multiplied with the WeaponInfusion.Scaling[level] to determine the total scaling
        //If a base Scaling percent isn't present, that stat doesn't scale with that type of quirk on the weapon; the infusion must account for all damage types;
    }



}

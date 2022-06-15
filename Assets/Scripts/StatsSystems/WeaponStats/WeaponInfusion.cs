using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;


//the weapon infusion controls how a weapon scales as it's level increases.
[CreateAssetMenu(fileName = "WeaponInfusion", menuName = "ScriptableObjects/WeaponInfusion", order = 1)]
public class WeaponInfusion : ScriptableObject, ISerializationCallbackReceiver
{
    [SerializeField]
    private List<QuirkBonus> inQuirkBonus;
    [SerializeField]
    private List<PrimalScaling> inPrimalScalingBonus;


    public ReadOnlyDictionary<string, AnimationCurve> PrimalScalingBonus()
    {
        return new ReadOnlyDictionary<string, AnimationCurve>(primalScalingBonus);
    }
    public ReadOnlyDictionary<string, float> BaseQuirkAdjust()
    {
        return new ReadOnlyDictionary<string, float>(baseQuirkAdjust);
    }
    public ReadOnlyDictionary<string, AnimationCurve> BaseQuirkLevelScaling()
    {
        return new ReadOnlyDictionary<string, AnimationCurve>(baseQuirkLevelScaling);
    }
    private Dictionary<string, AnimationCurve> primalScalingBonus;
    private Dictionary<string, float> baseQuirkAdjust;
    private Dictionary<string, AnimationCurve> baseQuirkLevelScaling;



    public void OnAfterDeserialize()//sets the readonly values for the weapon infusion every time they are upodated. This happens in editor.
    {
        primalScalingBonus = GetPrimalScaling();
        baseQuirkAdjust = GetBaseQuirkAdjust();
        baseQuirkLevelScaling = GetBaseQuirkLevelScaling();
    }
    private Dictionary<string,AnimationCurve> GetPrimalScaling()
    {
        Dictionary<string, AnimationCurve> primalScalingValues = new();
        foreach(PrimalScaling value in inPrimalScalingBonus)
        {
            primalScalingValues.Add(value.primalStat, value.levelScaling);
        }
        return primalScalingValues;
    }
    private Dictionary<string, float> GetBaseQuirkAdjust()
    {
        Dictionary<string, float> baseDamageAdjust = new();
        foreach (QuirkBonus value in inQuirkBonus)
        {
            baseDamageAdjust.Add(value.quirkType, value.baseDamageAdjustment);
        }
        return baseDamageAdjust;
    }
    private Dictionary<string, AnimationCurve> GetBaseQuirkLevelScaling()
    {
        Dictionary<string, AnimationCurve> baseDamageLevelScaling = new();
        foreach (QuirkBonus value in inQuirkBonus)
        {
            baseDamageLevelScaling.Add(value.quirkType, value.levelScaling);
        }
        return baseDamageLevelScaling;
    }









    [System.Serializable]
    private struct QuirkBonus
    {
        public string quirkType;
        public float baseDamageAdjustment;// directly efects base damage per level. Stat composite, added directly to weapon stats.
        public AnimationCurve levelScaling;//multiplier per level
    }
    [System.Serializable]
    private struct PrimalScaling
    {
        public string primalStat;
        public AnimationCurve levelScaling; // multiply this value by The relevant primal stat as well as the TOTAL base damage after adjustments, 
        //ex. 1.1 1.2 1.4. It will rarely be any higher.
    }





    public void OnBeforeSerialize()
    {
        //throw new System.NotImplementedException();
    }


}

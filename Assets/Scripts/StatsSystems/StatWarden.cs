using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;


public delegate void LivingStatEvent();
public class StatWarden : MonoBehaviour
{
    public CharacterStats characterStats;

    private Dictionary<string, float> livingStats = new(); // 0 represents full health, negative numbers show how much health is missing

    public event LivingStatEvent updateLivingStatEvent;

    private void Start()
    {
        foreach(var val in characterStats.EmergentStats())
        {
            Debug.Log(ImpactStat(val.Key, -1f, new List<string>{ "Wysical" }));
        }
        foreach(var val in livingStats)
        {
            GetLivingStat(val.Key, out float currentValue, out float maxValue);
            Debug.Log(val.Key + "   ~ ~ ~   " + currentValue +"  out of  " +maxValue);
        }
    }


    public float ImpactStat(string stat, float impact, List<string> quirks)
    {

        float statFilter = 1f;
        float adjustedImpact = impact;
        foreach (string quirk in quirks)
        {
            statFilter *= characterStats.QuirkAugurs().TryGetValue(quirk, out float value) ? value : 1f;
        }
        adjustedImpact *= statFilter;
        if (characterStats.EmergentStats().ContainsKey(stat))
        {
            if (!livingStats.ContainsKey(stat))
            {
                livingStats.Add(stat, 0f);
            }
            float maxStat = characterStats.EmergentStats().TryGetValue(stat, out float value) ? value : 0f;
            livingStats[stat] = Mathf.Clamp(livingStats[stat] + adjustedImpact, -maxStat, 0f);

            updateLivingStatEvent?.Invoke();//********
            return maxStat + livingStats[stat];
        }
        else
        {
            Debug.LogError("Stat " + stat + " does not exist in " + GetType().Name);
            return -1f;//-1 is returned if the stat does not exist;
        }
    }

    public void GetLivingStat(string stat, out float currentValue, out float maxValue)
    {
        if (characterStats.EmergentStats().ContainsKey(stat))
        {
            if (!livingStats.ContainsKey(stat))
            {
                livingStats.Add(stat, 0f);
            }
            maxValue = characterStats.EmergentStats()[stat];
            currentValue = maxValue + livingStats[stat];
        }
        else
        {
            Debug.LogError("Stat " + stat + " does not exist in " + GetType().Name);
            maxValue = 0f;
            currentValue = 0f;
        }
    }
}

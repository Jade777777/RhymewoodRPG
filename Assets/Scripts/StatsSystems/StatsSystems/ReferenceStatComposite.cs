using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReferenceStatComposite : StatComposite
{
    protected List<StatComponent> referenceOnlyComponents = new();
    public void AddReferenceOnlyStat(StatComponent stat)
    {
        if (!stat.Equals(this))
        {
            referenceOnlyComponents.Add(stat);
            ProcessStats();
        }
        else
        {
            Debug.LogError("Can't Add() a stat composite to itself!");
        }
    }

    public void RemoveReferenceOnlyStat(int id)
    {
        List<StatComponent> removeList = new() { };
        foreach (StatComponent component in referenceOnlyComponents)
        {
            if (component.ID == id)
            {
                removeList.Add(component);
            }
        }
        foreach (StatComponent trash in removeList)
        {
            referenceOnlyComponents.Remove(trash);
        }
        ProcessStats();
    }
    internal Dictionary<string, List<(string, AnimationCurve)>> GetReferenceDependentStats()
    {
        Dictionary<string, List<(string, AnimationCurve)>> dependentStats = new();
        foreach (StatComponent child in referenceOnlyComponents)
        {
            foreach (KeyValuePair<string, List<(string, AnimationCurve)>> dependency in child.GetDependentStats())
            {
                if (dependentStats.ContainsKey(dependency.Key))
                {
                    dependentStats[dependency.Key].AddRange(dependency.Value);
                }
                else
                {
                    dependentStats.Add(dependency.Key, dependency.Value);
                }
            }
        }
        return dependentStats;
    }

    internal Dictionary<string, float> GetReferenceRawStats()
    {
        Dictionary<string, float> rawStats = new();
        foreach (StatComponent child in referenceOnlyComponents)
        {

            foreach (KeyValuePair<string, float> raw in child.GetRawStats())
            {
                if (rawStats.ContainsKey(raw.Key))
                {
                    rawStats[raw.Key] += raw.Value;
                }
                else
                {
                    rawStats.Add(raw.Key, raw.Value);
                }
            }
        }
        return rawStats;
    }









    protected override void ProcessStats()
    {
        Dictionary<string, float> referenceRawStats = GetReferenceRawStats();
        Dictionary<string, List<(string, AnimationCurve)>> referenceDependentStats = GetReferenceDependentStats();
        Dictionary<string, float> referenceFinalStats = new(referenceRawStats); // Final stats start out as raw stats, We calculate the total bonus, and add it to the base values
        foreach (KeyValuePair<string, List<(string, AnimationCurve)>> referenceDependentStat in referenceDependentStats)
        {
            float total = 0;
            foreach ((string, AnimationCurve) dependency in referenceDependentStat.Value)
            {
                if (referenceRawStats.TryGetValue(dependency.Item1, out float referenceRawStatValue))
                {
                    total += dependency.Item2.Evaluate(referenceRawStatValue);
                }
            }

            if (referenceFinalStats.ContainsKey(referenceDependentStat.Key))
            {
                referenceFinalStats[referenceDependentStat.Key] += total;// add the total bonus for the particular key
            }
            else
            {
                referenceFinalStats.Add(referenceDependentStat.Key, total);// if there is no key create a new one using the total bonus.
            }
        }




        Dictionary<string, float> rawStats = GetRawStats();
        Dictionary<string, List<(string, AnimationCurve)>> dependentStats = GetDependentStats();

        Dictionary<string, float> finalStats = new(rawStats); // Final stats start out as raw stats, We calculate the total bonus, and add it to the base values
        foreach (KeyValuePair<string, List<(string, AnimationCurve)>> dependentStat in dependentStats)
        {
            float total = 0;
            foreach ((string, AnimationCurve) dependency in dependentStat.Value)
            {

                if (referenceFinalStats.TryGetValue(dependency.Item1, out float referenceStatValue))
                {
                    total += dependency.Item2.Evaluate(referenceStatValue);
                }
                if (rawStats.TryGetValue(dependency.Item1, out float rawStatValue))
                {
                    total += dependency.Item2.Evaluate(rawStatValue);
                }
            }

            if (finalStats.ContainsKey(dependentStat.Key))
            {
                finalStats[dependentStat.Key] += total;// add the total bonus for the particular key
            }
            else
            {
                finalStats.Add(dependentStat.Key, total);// if there is no key create a new one using the total bonus.
            }
        }
        stats = finalStats;

    }

}

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;


public class StatComposite : StatComponent
{
    protected List<StatComponent> components = new();
    
    public StatComposite() : base() 
    {
        ProcessStats();
    }

    public void Add(StatComponent stat) 
    {        
        if (!stat.Equals(this))
        {
            components.Add(stat);
            ProcessStats();
        }
        else
        {
            Debug.LogError("Can't Add() a stat composite to itself!");
        }
    }

    public void Remove(int id)
    {
        List<StatComponent> removeList = new() { };
        foreach(StatComponent component in components)
        {
            if (component.ID == id)
            {
                removeList.Add(component);
            }
        }
        foreach(StatComponent trash in removeList)
        {
            components.Remove(trash);
        }
        ProcessStats();
    }
    public void Refresh()
    {
        ProcessStats();
    }

    internal override Dictionary<string, List<(string, AnimationCurve)>> GetDependentStats()
    {
        Dictionary<string, List<(string, AnimationCurve)>> dependentStats = new();
        foreach (StatComponent child in components)
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

    internal override Dictionary<string, float> GetRawStats()
    {
        Dictionary<string, float> rawStats = new();
        foreach (StatComponent child in components)
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
}

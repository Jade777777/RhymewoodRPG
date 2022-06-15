using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MultStatLeaf : StatComponent
{
    protected string statName;

    protected ReferenceInfo referenceInfo;

    float maxValue=1000;
    public MultStatLeaf(string name, string referenceName, float linearCurve) : base()
    {
        statName = name;
        referenceInfo.referenceStatName = referenceName;
        referenceInfo.curve =  AnimationCurve.Linear(0, 0, maxValue, maxValue * linearCurve);
        
   
        ProcessStats();
    }
    public MultStatLeaf(string name, string referenceName, AnimationCurve curve) : base()
    {
        statName = name;
        referenceInfo.referenceStatName = referenceName;
        referenceInfo.curve = curve;
        ProcessStats();
    }

    internal override Dictionary<string, float> GetRawStats()
    {
        Dictionary<string, float> empty = new() { };
        return empty;
    }
    internal override Dictionary<string, List<(string, AnimationCurve)>> GetDependentStats()
    {
        List<(string, AnimationCurve)> dependentStatWithValue = new();
        dependentStatWithValue.Add((referenceInfo.referenceStatName, referenceInfo.curve));
        Dictionary<string, List<(string, AnimationCurve)>> multStats = new()
        {
            { statName, dependentStatWithValue }
        };
        return multStats;
    }


}


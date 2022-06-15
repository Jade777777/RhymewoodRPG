using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlatStatLeaf : StatComponent
{
    protected string statName;

    protected float flatValue;


    public FlatStatLeaf(string sn, float fv) : base()
    {
        statName = sn;
        flatValue = fv;
        ProcessStats();
    }

    internal override Dictionary<string, float> GetRawStats()
    {
        Dictionary<string, float> flatStats = new()
        {
            { statName, flatValue }
        };
        return flatStats;
    }
    internal override Dictionary<string, List<(string, AnimationCurve)>> GetDependentStats()
    {
        Dictionary<string, List<(string, AnimationCurve)>> empty = new() { };
        return empty;
    }


}

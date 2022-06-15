using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BatchStatLeaf : StatComponent
{
    
    protected StatComponent component;
    public BatchStatLeaf(StatComponent c) : base()
    {
        component = c;
        ProcessStats();
    }

    internal override Dictionary<string, float> GetRawStats()
    {
        return new Dictionary<string,float>(component.Stats());
    }
    internal override Dictionary<string, List<(string, AnimationCurve)>> GetDependentStats()
    {
        Dictionary<string, List<(string, AnimationCurve)>> empty = new() { };
        return empty;
    }
}

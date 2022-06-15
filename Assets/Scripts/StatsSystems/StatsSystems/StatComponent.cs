using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;



public abstract class StatComponent
{
	public ReadOnlyDictionary<string, float> Stats() 
	{
		return new ReadOnlyDictionary<string, float>(stats);
	} 
    protected Dictionary<string, float> stats = new() { };
	
	private static int iteration=0;
	public readonly int ID;

	public StatComponent()
	{
		ID = iteration;
		iteration++;	
	}

	//<Stat Name, Stat Value>
	internal abstract Dictionary<string, float> GetRawStats();

	//<Stat Name, Stat Reference, Stat Multiplier>
	internal abstract Dictionary<string, List <(string, AnimationCurve)>> GetDependentStats();

    protected virtual void ProcessStats()
    {
        Dictionary<string, float> rawStats = GetRawStats();
		Dictionary<string, List<(string, AnimationCurve)>> dependentStats = GetDependentStats();

        Dictionary<string, float> finalStats = new(rawStats); // Final stats start out as raw stats, We calculate the total bonus, and add it to the base values
        foreach(KeyValuePair<string, List<(string,AnimationCurve)>> dependentStat in dependentStats)
        {
			float total = 0;
			foreach((string, AnimationCurve) dependency in dependentStat.Value)
            {
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

	[System.Serializable]
    protected struct ReferenceInfo
    {
		[SerializeField]
		public string referenceStatName;
		[SerializeField]
		public AnimationCurve curve;
    }

}

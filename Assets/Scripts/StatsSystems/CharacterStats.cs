using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;


[CreateAssetMenu(fileName = "CharacterStats", menuName = "ScriptableObjects/CharacterStats", order = 1)]
public class CharacterStats : ScriptableObject, ISerializationCallbackReceiver
{
    [SerializeField]
    private GameEvent characterBaseStatUpdated;


    private StatComposite primalStatComposite = new();
    private ReferenceStatComposite emergentStatComposite = new();
    private ReferenceStatComposite quirkAugurStatComposite = new();


    //these are the core of the stat system. Leveling up and items add to and adjust these stats.
    [SerializeField]
    private List<PrimalStat> primalStats = new() { };

    //these act as the Max Stats for for things like health and stamina. These stats are always present only modified by primal stats
    [SerializeField]
    private List<EmergentStat> emergentStats = new() { };

    //these values represent the % at which incoming healing/damage is adjusted. These stats are always present only modified by primal stats
    [SerializeField]
    private List<QuirkAugur> quirkAugur = new() { };

    //-----------------------------------------------------------






    public ReadOnlyDictionary<string, float> PrimalStats()
    {
        return primalStatComposite.Stats();
    }
    public ReadOnlyDictionary<string, float> EmergentStats()
    {
        return emergentStatComposite.Stats();
    }
    public ReadOnlyDictionary<string, float> QuirkAugurs()
    {
        return quirkAugurStatComposite.Stats();
    }
    public void AddStat(StatComponent stat)
    {
        primalStatComposite.Add(stat);
        emergentStatComposite.Refresh();
        quirkAugurStatComposite.Refresh();
        characterBaseStatUpdated.TriggerEvent();
    }
    public void RemoveStat(int id)
    {
        primalStatComposite.Remove(id);
        emergentStatComposite.Refresh();
        quirkAugurStatComposite.Refresh();
        characterBaseStatUpdated.TriggerEvent();
    }
    //-------------------------------------------------------------

    public void OnAfterDeserialize()
    {
        ResetToDefaultStats();
    }
    public void OnBeforeSerialize() { }


    private void ResetToDefaultStats()
    {
        

        primalStatComposite = new();
        emergentStatComposite = new();
        quirkAugurStatComposite = new();
        

        foreach (PrimalStat stat in primalStats)
        {
            primalStatComposite.Add(new FlatStatLeaf(stat.name, stat.flatModifier));

            foreach (Multiply multMod in stat.multiplyModifiers)
            {
                primalStatComposite.Add(new MultStatLeaf(stat.name, multMod.referenceStat, multMod.multiplier));
            }
            foreach (Curve curveMod in stat.curveModifiers)
            {
                primalStatComposite.Add(new MultStatLeaf(stat.name, curveMod.referenceStat, curveMod.curve));
            }
        }
 ;
        emergentStatComposite.AddReferenceOnlyStat(primalStatComposite);

        foreach (EmergentStat stat in emergentStats)
        {
            emergentStatComposite.Add(new FlatStatLeaf(stat.name, stat.flatModifier));

            foreach (Multiply multMod in stat.multiplyModifiers)
            {
                emergentStatComposite.Add(new MultStatLeaf(stat.name, multMod.referenceStat, multMod.multiplier));
            }
            foreach (Curve curveMod in stat.curveModifiers)
            {
                emergentStatComposite.Add(new MultStatLeaf(stat.name, curveMod.referenceStat, curveMod.curve));
            }
        }
        quirkAugurStatComposite.AddReferenceOnlyStat(primalStatComposite);

        foreach (QuirkAugur stat in quirkAugur)
        {
            quirkAugurStatComposite.Add(new FlatStatLeaf(stat.name, stat.flatModifier));
            
            foreach (Multiply multMod in stat.multiplyModifiers)
            {
                quirkAugurStatComposite.Add(new MultStatLeaf(stat.name, multMod.referenceStat, multMod.multiplier));
            }
            foreach (Curve curveMod in stat.curveModifiers)
            {
                quirkAugurStatComposite.Add(new MultStatLeaf(stat.name, curveMod.referenceStat, curveMod.curve));
            }
        }


        Debug.Log(GetType().Name + " Has been reset to default values!");
        foreach (KeyValuePair<string, float> kvp in primalStatComposite.Stats())
        {
            Debug.Log(kvp.Key + ":   " + kvp.Value);
        }
        Debug.Log("The following are Emergent Stats!");
        foreach (KeyValuePair<string, float> kvp in emergentStatComposite.Stats())
        {
            Debug.Log(kvp.Key + ":   " + kvp.Value);
        }
        Debug.Log("The following are the Augur Inputs!");
        foreach (KeyValuePair<string, float> kvp in quirkAugurStatComposite.Stats())
        {
            Debug.Log(kvp.Key + ":   " + kvp.Value);
        }
 

    }




    [System.Serializable]
    private struct PrimalStat
    {
        public string name;

        public float flatModifier;
       
        public  List<Multiply> multiplyModifiers;
        
        public  List<Curve> curveModifiers;
    }

    [System.Serializable]
    private struct EmergentStat
    {
        public string name;

        public float flatModifier;

        public List<Multiply> multiplyModifiers;

        public List<Curve> curveModifiers;
    }

    [System.Serializable]
    private struct QuirkAugur
    {
        public string name;

        public float flatModifier;

        public List<Multiply> multiplyModifiers;

        public List<Curve> curveModifiers;
    }




    [System.Serializable]
    public struct Multiply
    {
        public Multiply( string r, float m)
        {
            referenceStat = r;
            multiplier = m;
        }
        public string referenceStat;
        public float multiplier;
    }

    [System.Serializable]
    public struct Curve
    {
        public Curve(string r, AnimationCurve c)
        {
            referenceStat = r;
            curve = c;
        }
        public string referenceStat;
        public AnimationCurve curve;
    }

}

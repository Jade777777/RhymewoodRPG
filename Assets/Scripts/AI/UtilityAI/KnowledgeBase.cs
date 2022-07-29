using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class KnowledgeBase : MonoBehaviour
{
    [HideInInspector]
    public GameObject ClosestSightedHostile { get; private set; }
    [HideInInspector]
    public GameObject ClosestFriendlysEnemy{ get; private set; }
    [HideInInspector]
    public GameObject ClosestSightedNuetral { get; private set; }
    [HideInInspector]
    public GameObject ClosestSightedFriendly { get; private set; }
    [HideInInspector]
    public GameObject ClosestSightedCharacter { get; private set; }
    [HideInInspector]
    public GameObject HighestAgroSighted { get; private set; }
    [HideInInspector]
    public GameObject CurrentEnemy { get; private set; }
    public GameObject FriendlyHighestAgro()
    {
        return ClosestSightedFriendly.GetComponent<KnowledgeBase>().HighestAgroSighted;
    }


    private CharacterNerveCenter cnv;
    private EquipedWeapon equipedWeapon;
    private void Awake()
    {
        cnv = GetComponent<CharacterNerveCenter>();
        equipedWeapon = GetComponent<EquipedWeapon>();
    }

    // Start is called before the first frame update
    public float GetAxisValue(UtilityAxis utilityAxis)
    {
        float axisValue = 0f;
        switch (utilityAxis)
        {
            case UtilityAxis.Health:
                axisValue = AxisHealth();
                break;
            case UtilityAxis.Threat://
                axisValue = AxisThreat();
                break;
            case UtilityAxis.Agro://0 means no enemies have agro, 1 means an enemy has an agro of 20 or more
                axisValue = AxisAgro();
                break;
            case UtilityAxis.AgroEnemyDistance:// 0 means they are 20 meters or more away, 1 means they are within the weapons engagement distance
                axisValue = AxisAgroEnemyDistance();
                break;
            case UtilityAxis.ClosestFriendly://0 means they are 20 meters away, 1 means they are in the same position
                axisValue = AxisFriendlyDistance();
                break;
            case UtilityAxis.ClosestCharacter://same^
                axisValue = AxisCharacterDistance();
                break;
            default:
                Debug.LogError(utilityAxis + " has not been implemented!");
                break;
        }
        Debug.Assert(axisValue <= 1f && axisValue >= 0f);
        return axisValue;
    }

    [SerializeField] 
    private Kin kin;
    public Kin KinType { get { return kin; } }
    private Dictionary<GameObject,float> charactersSighted= new();
    private float forgetTime=8f;
    private HashSet<GameObject> hostilesSighted = new();
    private HashSet<GameObject> nuetralsSighted = new();
    public HashSet<GameObject> friendlysSighted = new();
    private HashSet<GameObject> friendlysCurrentEnemies = new();
    private Dictionary<GameObject,float> characterAgro = new();//When damage is taken the threat is increased
    
    public void SightCharacter(GameObject character)
    {
            KnowledgeBase ckb = character.GetComponent<KnowledgeBase>();
            
            if (KinType.Hostile.Contains(ckb.KinType))
            {
                charactersSighted[character] = Time.time + forgetTime;
                hostilesSighted.Add(character);
            }
            else if (KinType.Friendly.Contains(ckb.KinType))
            {
                charactersSighted[character] = Time.time + forgetTime*4;//increase the forget time for friendly creatures because it aids in team work
                friendlysSighted.Add(character);
                 
            
                if (ckb.HighestAgroSighted != null 
                    && !KinType.Friendly.Contains(ckb.HighestAgroSighted.GetComponent<KnowledgeBase>().KinType) 
                    && !ckb.HighestAgroSighted.Equals(gameObject))
                {
                Debug.Log("I See a friendly with an enemy");
                friendlysCurrentEnemies.Add(ckb.HighestAgroSighted);
                charactersSighted[ckb.HighestAgroSighted] = Time.time + forgetTime;
                }
            }
            else
            {
                charactersSighted[character] = Time.time + forgetTime;
                nuetralsSighted.Add(character);
            }
    }


    public void UpdateSighting()
    {
        foreach(KeyValuePair<GameObject,float> c in new Dictionary<GameObject,float>(charactersSighted))
        {
            if(Time.time>c.Value)
            {
                charactersSighted.Remove(c.Key);
                hostilesSighted.Remove(c.Key);
                nuetralsSighted.Remove(c.Key);
                friendlysSighted.Remove(c.Key);
                friendlysCurrentEnemies.Remove(c.Key);
            }
        }
        foreach(GameObject c in friendlysSighted)
        {

        }
        UpdateAgro();
        FindClosestCharacter();
        FindClosestFriendly();
        FindClosestNuetral();
        FindClosestSightedHostile();
        FindClosestFriendlysEnemy();
        FindCurrentEnemy();
        FindHighestAgro();
    }
    private void UpdateAgro()
    {
        foreach(KeyValuePair<GameObject, float> c in new Dictionary<GameObject, float>(characterAgro))
        {
            if (c.Value <= Time.time)
            {
                Debug.Log("REMOVING :" + c.Key.name + "FROM AGRO LIST");
                characterAgro.Remove(c.Key);
            }
        }
    }
    public void HostileImpact(GameObject character, float impact)//impact is a percent of the health that was removed
    {
        Debug.Log(character.name+",,,,,"+transform.name);

        float RecoverTime = impact*80+1;// if a character llooses a quarter of there helath they develop agro for 20 seconds.
        if (kin.Hostile.Contains(character.GetComponent<KnowledgeBase>().kin))
        {
            RecoverTime *= 1.36f;
        }
        else if (kin.Friendly.Contains(character.GetComponent<KnowledgeBase>().kin))
        {
            RecoverTime = 5.87f;
        }

        if (characterAgro.ContainsKey(character))
        {
            characterAgro[character] = Mathf.Max(characterAgro[character]+RecoverTime, Time.time+RecoverTime);
        }
        else
        {
            characterAgro.Add(character, Time.time+RecoverTime);
        }
        SightCharacter(character);
    }




    private float AxisHealth()
    {
        return 0.3f;
    }
    private float AxisThreat()
    {
 
        float dangers = nuetralsSighted.Count + hostilesSighted.Count;
        float support = friendlysSighted.Count;
        float threat = Mathf.Clamp01((dangers - support)/8);// 8 is the max amount of danger to take into acount, clamp the final value between 0 and 1 as it should be.
        //threat is determined by how many enemies&nueterals vs allies are present
        return threat;
    }
    
    private float AxisAgroEnemyDistance()
    {

        if(CurrentEnemy != null)
        {
            float weaponRange = equipedWeapon.weaponInstance.weapon.engagementDistance;
            return 1-Mathf.Clamp01(((CurrentEnemy.transform.position - transform.position).magnitude-weaponRange) / 20);
        }
        else
        {
            return 0;
        }
    }
 
    
    private float AxisFriendlyDistance()
    {
        if (ClosestSightedFriendly != null)
        {
            return 1-Mathf.Clamp01((ClosestSightedFriendly.transform.position - transform.position).magnitude / 20);
        }
        else
        {
            return 0;
        }
    }
    private float AxisCharacterDistance()
    {
        if (ClosestSightedCharacter != null)
        {
            return 1-Mathf.Clamp01((ClosestSightedCharacter.transform.position - transform.position).magnitude / 20);
        }
        else
        {
            return 0;
        }
    }
    private float AxisAgro()
    {
        GameObject target = HighestAgroSighted;
        if (target != null)
        {
            return Mathf.Clamp01(characterAgro[target]-Time.time/20f);
        }
        else
        {
            return 0;
        }
    }










    private void FindClosestSightedHostile()
    {
        ClosestSightedHostile = hostilesSighted.OrderBy(character => (character.transform.position - transform.position).sqrMagnitude).FirstOrDefault<GameObject>();
    }
    private void FindClosestFriendlysEnemy()
    {
        ClosestFriendlysEnemy = friendlysCurrentEnemies.OrderBy(character => (character.transform.position - transform.position).sqrMagnitude).FirstOrDefault<GameObject>();
    }
    private void FindClosestNuetral()
    {
        ClosestSightedNuetral=nuetralsSighted.OrderBy(character => (character.transform.position - transform.position).sqrMagnitude).FirstOrDefault<GameObject>();
    }
    private void FindClosestFriendly()
    {
        ClosestSightedFriendly = friendlysSighted.OrderBy(character => (character.transform.position - transform.position).sqrMagnitude).FirstOrDefault<GameObject>();
    }
    private void FindClosestCharacter()
    {
        ClosestSightedCharacter = charactersSighted.OrderBy(character => (character.Key.transform.position - transform.position).sqrMagnitude).FirstOrDefault().Key;
    }
    private void FindHighestAgro()
    {
        HighestAgroSighted = characterAgro.OrderBy(character => (character.Value)).LastOrDefault().Key;
        if (HighestAgroSighted!= null&& !charactersSighted.ContainsKey(HighestAgroSighted))
        {
            HighestAgroSighted = null;
        }

    }
    private void FindCurrentEnemy()
    {
        if (HighestAgroSighted != null)
        {
            CurrentEnemy = HighestAgroSighted;
               
        }
        else if (ClosestFriendlysEnemy != null)
        {
            CurrentEnemy = ClosestFriendlysEnemy;
        }
        else if (ClosestSightedHostile != null)
        {
            CurrentEnemy = ClosestSightedHostile;
        }
        else
        {
            CurrentEnemy = null;
        }

    }




    public enum UtilityAxis { Health, Threat, Agro, AgroEnemyDistance, ClosestFriendly, ClosestCharacter}
}

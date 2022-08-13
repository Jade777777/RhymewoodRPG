using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public class SimpleHitBox : HitBox
{
    [SerializeField]
    protected float knockbackDistance;
    [SerializeField]
    private KnockdownType knockdownType;
    [SerializeField]
    private int poiseDamage;
    [SerializeField]
    private List<QuirkDamage> quirkDamage;
    private ReadOnlyDictionary<string, float> quirkDamageD;
    [SerializeField]
    private float tick=0.5f;
    private void Awake()
    {
        Dictionary<string, float> damage= new();
        foreach(QuirkDamage qd in quirkDamage)
        {
            damage.Add(qd.quirkType, qd.damage);
        }
        quirkDamageD = new ReadOnlyDictionary<string, float>(damage);
    }
    HashSet<HurtBox> activeTick = new();
    Coroutine cref = null;
    protected override void OnTriggerEnter(Collider other)
    {        
        if (other.TryGetComponent(out HurtBox hurtBox))
        {
            activeTick.Add(hurtBox);
            OnStrikeHurtBox(hurtBox);
        }
        if (cref == null)
        {
            cref = StartCoroutine(DamageTick());
        }    
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out HurtBox hurtBox))
        {
            generateNewID();
            activeTick.Remove(hurtBox); 
        }
        if (activeTick.Count == 0)
        {
            StopCoroutine(DamageTick());
            cref = null;
        }
    }

    private IEnumerator DamageTick()
    {
        Debug.Log("Damage start!");
        while (activeTick.Count!=0)
        {
            yield return new WaitForSeconds(tick);
            Debug.Log("Tick");
            generateNewID();
            foreach (HurtBox hurtBox in activeTick)
            {
                OnStrikeHurtBox(hurtBox);
            }
            
        }
    }



    public override ReadOnlyDictionary<string, float> GetDamage()
    {
        return quirkDamageD;
    }

    public override void GetKnockbackDistance(HurtBox hurtBox, out Vector3 enemyKnockback, out Vector3 selfKnockback)
    {
        selfKnockback = Vector3.zero;
        enemyKnockback = (hurtBox.transform.position  - transform.position).normalized * knockbackDistance;
    }

    public override KnockdownType GetKnockdownType()
    {
        return knockdownType;
    }

    public override int GetPoiseDamage()
    {
        return poiseDamage;
    }

    protected override void OnStrikeHurtBox(HurtBox hurtBox)
    {
        Debug.Log(this.name + " Has struck something!");
        hurtBox.TakeDamage(this);
        
    }




}

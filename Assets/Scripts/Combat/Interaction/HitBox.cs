using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public abstract class HitBox : MonoBehaviour
{
    
    public float hitStop= 0.13f;

    public int hitID { get; private set; }
    private static int generateID;

    private void OnEnable()
    {
        generateNewID();
    }
    protected void generateNewID()
    {
        generateID++;
        hitID = generateID;
    }
    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out HurtBox hurtBox))
        {
            OnStrikeHurtBox(hurtBox);
        }
    }
    protected abstract void OnStrikeHurtBox(HurtBox hurtBox);

    public abstract void GetKnockbackDistance(HurtBox hurtBox, out Vector3 enemyKnockback, out Vector3 selfKnockback);
    public abstract ReadOnlyDictionary<string, float> GetDamage();
    public abstract int GetPoiseDamage();
    public abstract KnockdownType GetKnockdownType();
}
public enum KnockdownType { Stagger, Crush, Launch }
[System.Serializable]
public struct QuirkDamage// The base damage of the weapon
{
    public string quirkType;// The name of the present damage type (ex. Physical, Fire, Magic)
    public float damage;
}
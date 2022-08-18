using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public abstract class HitBox : MonoBehaviour
{
    
    public float hitStop= 0.13f;

    public int hitID { get; private set; }
    private static int generateID;

    protected virtual void Awake()
    {
        int HitBoxLayer = LayerMask.NameToLayer("HitBox");
        gameObject.layer = gameObject.layer | HitBoxLayer;
    }
    private void OnEnable()
    {
        GenerateNewID();
    }
    protected void GenerateNewID()
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
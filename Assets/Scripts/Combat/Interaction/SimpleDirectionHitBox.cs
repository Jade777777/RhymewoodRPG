using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleDirectionHitBox : SimpleHitBox
{
    [SerializeField]
    Vector3 knockbackDirection = Vector3.forward;
    public override void GetKnockbackDistance(HurtBox hurtBox, out Vector3 enemyKnockback, out Vector3 selfKnockback)
    {
        selfKnockback = Vector3.zero;
        enemyKnockback = transform.TransformDirection(knockbackDirection).normalized * knockbackDistance;
    }
}

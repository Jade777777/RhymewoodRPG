using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// The hurtbox provides the TakeDamage method for use with hitboxes
/// Nothing else will interact with a hurtbox.
/// Characterhurtboxes pass on the responsibility of taking damage
/// to the character nerve center, while rigidbody hurtboxes calculate 
/// how the rigidbody will respond and it is the only component required to 
/// set up rigidbodies for this game. generic rigidbodies should never be used.
/// </summary>

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public abstract class HurtBox : MonoBehaviour
{
    [HideInInspector]
    public GameObject sourceObject;
    

    protected virtual void Awake()
    {

        gameObject.layer = LayerMask.NameToLayer("HurtBox");
        sourceObject = gameObject;
    }
    protected Queue<int> hitIDs = new();
    protected int maxSize = 10;
    public virtual bool TakeDamage(HitBox hitBox)
    {
        
        if (!hitIDs.Contains(hitBox.hitID))
        {
            while (hitIDs.Count >= maxSize)
            {
                hitIDs.Dequeue();
            }

            hitIDs.Enqueue(hitBox.hitID);
            StartCoroutine(ProcessDamageAtEndOfFrame(hitBox));
        }
        else
        {
            return false;//don't do anything if the hitbox has already hit this target
        }
        return true;
    }

    IEnumerator ProcessDamageAtEndOfFrame(HitBox hitBox)
    {
        yield return new WaitForEndOfFrame();
        ProcessDamage(hitBox);
    }
    protected abstract void ProcessDamage(HitBox hitBox);

    public abstract float CenterToBounds(Transform position);//we use the position because the distance from the edges verry depending on the location.
}

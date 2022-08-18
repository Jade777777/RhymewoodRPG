using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigidBodyHurtBox : HurtBox
{
    protected Rigidbody rb;

    protected override void Awake()
    {
        base.Awake();
        gameObject.layer = LayerMask.NameToLayer("Default");
        rb = GetComponent<Rigidbody>();
    }
    private void OnEnable()
    {
        GetComponent<Collider>().isTrigger = false;
        rb.isKinematic = false;
    }
    public override float CenterToBounds(Transform t)
    {
        return (transform.position-rb.ClosestPointOnBounds(t.position)).magnitude;
    }

    protected override void ProcessDamage(HitBox hitBox)
    {
        float hitStop = hitBox.hitStop;
        hitBox.GetKnockbackDistance(this, out Vector3 enemyKnockback, out _);

        if (activeKnockback != null)
        {
            StopCoroutine(activeKnockback);
        }
        activeKnockback = StartCoroutine(RigidKnockBack(enemyKnockback, hitStop));
    }

    Coroutine activeKnockback;
    IEnumerator RigidKnockBack(Vector3 knockBack, float hitStop)
    {
        Vector3 startVelocity = rb.velocity;

        rb.isKinematic = true;
        
        yield return new WaitForSeconds(HitStop.hitPauseTime);


        //cleave through the target, put force and motion into the hit
        float cleaveTimer = 0;
        float scaledCleaveTime = HitStop.cleaveTime * hitStop;

        Vector3 CleaveDirection = (knockBack.normalized + Vector3.up).normalized;
        float speed = 20 / rb.mass;
        while (cleaveTimer < scaledCleaveTime)
        {
            transform.position += CleaveDirection*(1- (cleaveTimer/scaledCleaveTime))*speed*Time.deltaTime;
            cleaveTimer += Time.deltaTime;
            yield return null;
        }


        //hold and build up power, stretch like a rubberband

        yield return new WaitForSeconds(hitStop * HitStop.impactPauseTime);

        //Launch The target
        rb.isKinematic = false;
        rb.AddForce((knockBack * 5) , ForceMode.Impulse);//+ (startVelocity * rigidBody.mass)

    }


}

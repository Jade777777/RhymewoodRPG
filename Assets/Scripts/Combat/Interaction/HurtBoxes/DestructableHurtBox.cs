using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructableHurtBox : RigidBodyHurtBox
{
    [SerializeField]
    GameObject SpawnObject;
    
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
        return (transform.position - rb.ClosestPointOnBounds(t.position)).magnitude;
    }

    public void InheritHitIDs(Queue<int> hitIDs)
    {
        base.hitIDs = hitIDs;
    }

    protected override void ProcessDamage(HitBox hitBox)
    {
        GameObject spawn = Instantiate(SpawnObject, transform.position, transform.rotation);
        if (spawn.TryGetComponent(out DestructableHurtBox spawnHurtBox))
        {
            spawnHurtBox.InheritHitIDs(hitIDs);
           
        }
        gameObject.SetActive(false);
    }

}

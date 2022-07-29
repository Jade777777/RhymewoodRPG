using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterPerception : MonoBehaviour
{
    KnowledgeBase knowledgeBase;
    CharacterController cc;
    [SerializeField]
    private float viewDistance = 10;
    [SerializeField]
    private float TimeStep = 0.4f;
    [SerializeField]
    public bool UpdateSight = true;

    private int layerMask = 1 << 3;
    private void Awake()
    {
        TimeStep = ((Random.value+2) * TimeStep)/3;
        knowledgeBase = GetComponent<KnowledgeBase>();
        cc = GetComponent<CharacterController>();
    }

    private void Start()
    {
        StartCoroutine(SightTick());
    }

    IEnumerator SightTick()
    {
        yield return new WaitForSeconds(0.25f+TimeStep*Random.value);
        while (true)
        {
            if (UpdateSight)
            {
                float viewRadius = viewDistance / 2;
                Vector3 viewCenter = transform.position + (transform.forward * (viewRadius))+(cc.center);//+cc.radius add this to view radius if we dont want the colliders to overlp
                Collider[] sighted = Physics.OverlapSphere(viewCenter, viewRadius, layerMask, QueryTriggerInteraction.Ignore);


                foreach (Collider thing in sighted)
                {

                        float offset = cc.radius;
                        Vector3 origin = transform.position + (transform.forward * offset) + cc.center;
                        Vector3 direction = thing.transform.position - transform.position;

                        if (Physics.Raycast(origin, direction, out RaycastHit hitInfo, direction.magnitude, layerMask) && hitInfo.collider == thing)
                        {
                            Debug.DrawRay(origin, direction, Color.red, 0.5f);
                            knowledgeBase.SightCharacter(thing.gameObject);
                        }
                    
                }
            }
            yield return new WaitForSeconds(TimeStep);
        }
    }
    private void OnDrawGizmosSelected()
    {
        float radius = viewDistance / 2;
        Vector3 origin = transform.position + (transform.forward * (radius))+Vector3.up;
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(origin, radius);


    }


}

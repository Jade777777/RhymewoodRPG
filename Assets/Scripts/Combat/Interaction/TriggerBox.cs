using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerBox : MonoBehaviour
{
    KnowledgeBase knowledgeBase;
    private void Awake()
    {
        knowledgeBase = GetComponentInParent<KnowledgeBase>();
    }
    [SerializeField]
    string methodName="Attack";
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform != transform.parent)
        {
            print("Trigger Box Entered!" +other.transform.name);
            gameObject.SendMessageUpwards(methodName,other.transform);

        }
    }
}

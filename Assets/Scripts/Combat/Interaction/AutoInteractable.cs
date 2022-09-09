using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Interactable))]
public class AutoInteractable : MonoBehaviour
{
    Interactable interactable;
    private void Awake()
    {
        interactable = GetComponent<Interactable>();
    }
    // Update is called once per frame

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == LayerMask.NameToLayer("Character"))
        {
            interactable.Activate(other.GetComponent<Animator>());
        }
    }
}

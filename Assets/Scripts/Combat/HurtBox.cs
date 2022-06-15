using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HurtBox : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out HitBox hit) && other.transform.parent != transform.parent) 
        {
            print("take damage");
            gameObject.SendMessageUpwards("TakeDamage");
        }
    }
}

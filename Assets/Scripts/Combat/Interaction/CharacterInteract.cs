using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterInteract : MonoBehaviour
{
    private Camera mainCamera;
    public float interactionDistance;
    public float interactionAngle;
    CharacterNerveCenter cnc;
    Animator animator;
    
    private void Awake()
    {
        cnc = GetComponent<CharacterNerveCenter>();
        mainCamera = Camera.main;
        animator = GetComponent<Animator>();
    }

    public void CI_Interact()
    {
        Collider[] inRange = Physics.OverlapSphere(mainCamera.transform.position, interactionDistance,1<<9);
        GameObject target = null;
        float smallestAngle = 180;
        if (cnc.IsPlayer == false)// if the player is interacting we determine the object by the direction the player is looking.
        {                        // we don't want the playerto be trying to interact with a door and an object at there feet is preventing them from doing so.
            foreach (Collider col in inRange)
            {

                Vector3 colPos = col.ClosestPoint(mainCamera.transform.position);
                Vector3 distanceVector = colPos - mainCamera.transform.position;
                float angle = Vector3.Angle(mainCamera.transform.forward, distanceVector);
                if (angle <= interactionAngle && angle < smallestAngle)
                {
                    smallestAngle = angle;
                    target = col.gameObject;
                }
            }
        }
        else
        {
            foreach (Collider col in inRange)
            {

                Vector3 colPos = col.ClosestPoint(transform.position);
                Vector3 distanceVector = colPos - mainCamera.transform.position;
                distanceVector.y = 0;
                float angle = Vector3.Angle(transform.forward, distanceVector);
                if (angle <= interactionAngle && angle < smallestAngle)
                {
                    smallestAngle = angle;
                    target = col.gameObject;
                }

            }
        }
        if(target!= null)
        {
            target.GetComponent<Interactable>().Activate(animator);
        }

            

    }
}

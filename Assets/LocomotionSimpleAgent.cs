// LocomotionSimpleAgent.cs
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class LocomotionSimpleAgent : MonoBehaviour
{
    CharacterNerveCenter cnv;
    NavMeshAgent agent;


    void Start()
    {
       
        cnv = GetComponent<CharacterNerveCenter>();
        agent = GetComponent<NavMeshAgent>();
        // Don’t update position automatically
        agent.updatePosition = false;
        agent.speed = cnv.PrimalStats()["Move Speed"];
    }

    void Update()
    {
        MoveCharacter();

    }

    private void MoveCharacter()
    {
        if (agent.hasPath)
        {
            Vector3 localDesiredVelocity = transform.InverseTransformDirection(agent.desiredVelocity);
            Vector2 velocity = new Vector2(localDesiredVelocity.x, localDesiredVelocity.z);


            Vector2 moveInput = Vector2.ClampMagnitude(velocity / agent.speed, 1f);
            if (moveInput != Vector2.zero) cnv.Move(moveInput);



            agent.nextPosition = transform.position;
        }
        else
        {
            cnv.Move(Vector2.zero);
        }
    }
}
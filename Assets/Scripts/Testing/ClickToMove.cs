// ClickToMove.cs
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

[RequireComponent(typeof(NavMeshAgent))]
public class ClickToMove : MonoBehaviour
{
    RaycastHit hitInfo = new();
    NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }
    void Update()
    {

    }
    public void OnFire()
    {
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Camera.main.pixelWidth/2,Camera.main.pixelHeight/2,0));
        if (Physics.Raycast(ray.origin, ray.direction, out hitInfo))
            agent.destination = hitInfo.point;
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectMover : MonoBehaviour
{
    public Vector3 move;
    public float frequency;
    Rigidbody body;
    Vector3 origin;
    private void Start()
    {
        origin = transform.position;
        body = GetComponent<Rigidbody>();
    }

    void Update()
    {
        Vector3 positionOffset= Vector3.Lerp(move, -move, Mathf.PingPong(Time.time*frequency,1));
       // positionOffset += Vector3.Lerp(new Vector3(move.z,move.y,move.x), -new Vector3(move.z, move.y, move.x), Mathf.PingPong(Time.time * frequency*2, 1));
        body.MovePosition(origin + positionOffset);

    }
}

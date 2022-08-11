using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableOnPlay : MonoBehaviour
{
    private void Awake()
    {
        Destroy(gameObject);
    }
}

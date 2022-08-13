using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRender : MonoBehaviour
{
    CharacterNerveCenter cnc;

    [SerializeField]
    GameObject playerModelCamCopy;
    [SerializeField]
    GameObject modelOriginal;

    private void Awake()
    {
        cnc = GetComponent<CharacterNerveCenter>();
    }
    void Start()
    {
        if (cnc.IsPlayer)
        {
            modelOriginal.SetActive(false);
            playerModelCamCopy.SetActive(true);
        }
        if (cnc.IsPlayer == false)
        {
            modelOriginal.SetActive(true);
            playerModelCamCopy.SetActive(false);
        }
    }


}

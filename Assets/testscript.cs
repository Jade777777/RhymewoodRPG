using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testscript : MonoBehaviour
{
    public GameObject EquipedWeapon;
    private GameObject weaponInstance;

    private void Start()
    {
        StartCoroutine(CreateAndDestroy());
    }
    IEnumerator CreateAndDestroy()
    {

        while (true)
        {
            weaponInstance = Instantiate(EquipedWeapon, transform);
            weaponInstance.name = "TestThing";
            GetComponent<Animator>().Rebind();// this lets the animator know that changes have been made and it needs to be updated. It works smoothly
            yield return new WaitForSeconds(6);
            Destroy(weaponInstance);
            yield return new WaitForSeconds(2);
        }
    }
}

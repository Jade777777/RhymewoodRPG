using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Weapon", menuName = "ScriptableObjects/Weapon", order = 1)]
public class Weapon : ScriptableObject
{
    public GameObject model;
    public WeaponClass weaponClass;

}
public enum WeaponClass { Sword, Spear, Hammer };

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Weapon", menuName = "Weapons/Weapon", order = 1)]
public class Weapon : ScriptableObject
{
    public GameObject weaponModel;
    public List<Attack> attacks;

    //move weapon warden logic into equiped weapon
    public List<WeaponWarden.BaseQuirk> baseQuirk;


}

[System.Serializable]
public class WeaponInstance // this class is wahat lets us get rid of the WeaponWarden.
{
    public Weapon weapon;
    public int level;
    public WeaponInfusion weaponInfusion;
}

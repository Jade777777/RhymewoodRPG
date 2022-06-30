using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Weapon", menuName = "Weapons/Weapon", order = 1)]
public class Weapon : ScriptableObject
{
    public GameObject weaponModel;
    public float engagementDistance = 2;
    public List<Attack> attacks;

    //move weapon warden logic into equiped weapon
    public List<EquipedWeapon.BaseQuirk> baseQuirk;

}

[System.Serializable]
public class WeaponInstance // this class is wahat lets us get rid of the WeaponWarden.
{

    public WeaponInstance(Weapon weapon, int level, WeaponInfusion weaponInfusion)
    {
        this.weapon = weapon;
        this.level = level;
        this.weaponInfusion = weaponInfusion;
        Debug.Log("Constructing a weapon instance!");
    }
    public string name 
    {
        get 
        { 
            return weapon.name; 
        } 
        private set 
        { 
            name = value; 
        } 
    }
    public Weapon weapon;
    public int level;
    public WeaponInfusion weaponInfusion;
}

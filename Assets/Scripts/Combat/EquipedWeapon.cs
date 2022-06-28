using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EquipedWeapon : MonoBehaviour
{
    AnimatorOverrideController animatorOverrideController;

    [SerializeField]
    private Weapon equipedWeapon;//replace with weaponInstance
    [SerializeField]
    private WeaponInstance weapon;//this will replace equipedWeapon

    [SerializeField]
    private Transform weaponJoint;

    private GameObject modelInstance;
    private List<GameObject> attackModelInstance = new();

    private List<KeyValuePair<AnimationClip, AnimationClip>> defaultAnimationOverrides;
    private List< KeyValuePair<AnimationClip, AnimationClip> > animationOverrides = new();

    private static int countID=0;
    private int ID;
    private void Awake()
    {
        ID = countID;
        countID++;
    }
    void Start()
    {
        Animator animator = GetComponent<Animator>();
        animatorOverrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
        animator.runtimeAnimatorController = animatorOverrideController;

        defaultAnimationOverrides = new List<KeyValuePair<AnimationClip, AnimationClip>>(animatorOverrideController.overridesCount);
        animatorOverrideController.GetOverrides(defaultAnimationOverrides);

        if(GetComponent<CharacterNerveCenter>().IsPlayer) SetWeapon(equipedWeapon);

    }

    
    public void EquipWeapon(Weapon weapon)
    {
        Debug.Assert(weapon != null);

        //cleanup previous weapon models
        CleanUpWeapon();
        //Equip the weapon
        SetWeapon(weapon);

    }

    private void SetWeapon(Weapon weapon)
    {
        //TODO:
        //save weapon infusion.

        equipedWeapon = weapon;//save the value of the new weapon
        //instantiate the new weapon model
        modelInstance = Instantiate(equipedWeapon.weaponModel, weaponJoint);
        modelInstance.name = equipedWeapon.name+" "+ID;
        //instantiate the new attack models used by the new animations;

        foreach (Attack attack in equipedWeapon.attacks)
        {
            bool SkipSpawn= false;
            foreach(GameObject instance in attackModelInstance)
            {
                SkipSpawn = SkipSpawn || (instance.name == attack.attackAnimation.overrideClip.name);
            }
            if (!SkipSpawn)
            {
                GameObject attackModel = Instantiate(attack.attackModel, transform);
                attackModel.name = attack.attackAnimation.overrideClip.name; //attack.attackModel.name;
                attackModelInstance.Add(attackModel);
            }

            AnimationClip attackOverrideClip = attack.attackAnimation.overrideClip;
            KeyValuePair<AnimationClip, AnimationClip> attackAnimationPair = new(attack.attackAnimation.originalClip, attackOverrideClip);
            animationOverrides.Add(attackAnimationPair);
        }

        animatorOverrideController.ApplyOverrides(animationOverrides);
        //Get the weapons hitboxes
        RetrieveHitboxes();

    }

 
    private void CleanUpWeapon()
    {
        Destroy(modelInstance);
        foreach (GameObject attackModel in attackModelInstance)//it's destroyed at the end of the frame so we don't need to loop backwards
        {
            Destroy(attackModel);
        }
        //clear animation overrides;
        animationOverrides = new();
        attackModelInstance = new();
    }

    private Component[] HitBoxes;
    private void RetrieveHitboxes()
    {

        HitBoxes = modelInstance.GetComponentsInChildren<HitBox>(true);
        Debug.Log("Got some hitboxes here");
    }
    public void EW_ActivateWeaponHitBox()
    {
        foreach(HitBox hitBox in HitBoxes)
        {
            hitBox.gameObject.SetActive(true);
        }
    }
    public void EW_DisableWeaponHitBox()
    {
        foreach (HitBox hitBox in HitBoxes)
        {
            hitBox.gameObject.SetActive(false);
        }
    }
    
}

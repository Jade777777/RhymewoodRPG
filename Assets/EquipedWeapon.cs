using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipedWeapon : MonoBehaviour
{
    AnimatorOverrideController animatorOverrideController;

    [SerializeField]
    private Weapon equipedWeapon;



    [SerializeField]
    private Transform weaponJoint;

    private GameObject modelInstance;
    private List<GameObject> attackModelInstance = new();

    private List<KeyValuePair<AnimationClip, AnimationClip>> defaultAnimationOverrides;
    private List< KeyValuePair<AnimationClip, AnimationClip> > animationOverrides = new();
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
        equipedWeapon = weapon;//save the value of the new weapon
        //instantiate the new weapon model
        modelInstance = Instantiate(equipedWeapon.weaponModel, weaponJoint);
        //instantiate the new attack models used by the new animations;
        foreach (Attack attack in equipedWeapon.attacks)
        {
            attackModelInstance.Add(Instantiate(attack.attackModel, transform));
            KeyValuePair<AnimationClip, AnimationClip> attackAnimationPair = new(attack.attackAnimation.originalClip, attack.attackAnimation.overrideClip);
            animationOverrides.Add(attackAnimationPair);
        }

        animatorOverrideController.ApplyOverrides(animationOverrides);
       
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
}

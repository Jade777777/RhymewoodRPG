using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Attack", menuName = "Weapons/Attack", order = 1)]
public class Attack : ScriptableObject
{
    public AnimationOverridePair attackAnimation;
    public GameObject attackModel;

    
    [System.Serializable]
    public struct AnimationOverridePair
    {
        public AnimationClip originalClip;
        public AnimationClip overrideClip;
    }
}

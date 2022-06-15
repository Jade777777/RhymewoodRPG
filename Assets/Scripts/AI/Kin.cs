using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.ObjectModel;
[CreateAssetMenu(fileName = "Kin", menuName = "ScriptableObjects/Kin", order = 1)]
public class Kin : ScriptableObject,ISerializationCallbackReceiver
{
    [SerializeField]
    private List<Kin> friendly;

    [SerializeField]
    private List<Kin> hostile;

    public HashSet<Kin> Friendly { get; private set; }

    //Relations are Nuetral by ommision. If not found to be friend or foe, nuetrality is assumed.

    public HashSet<Kin> Hostile { get; private set; }
    public void OnAfterDeserialize()
    {
        Friendly = new HashSet<Kin>(friendly);
        Hostile = new HashSet<Kin>(hostile);
    }

    public void OnBeforeSerialize()
    {
        //throw new System.NotImplementedException();
    }
}

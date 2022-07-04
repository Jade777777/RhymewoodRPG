using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;


public static class JadeUtility 
{
    public static T CreateDeepCopy<T>(T obj) // object must be marked with [System.Syrializable]
    {
        using (var ms = new MemoryStream())
        {
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(ms, obj);
            ms.Seek(0, SeekOrigin.Begin);
            return (T)formatter.Deserialize(ms);
        }
    }



    //Used to define a flat animation curve.
    public static AnimationCurve FloatToCurve(float value)
    {
        return new AnimationCurve(new Keyframe(0, value), new Keyframe(1, value));//curves look when the input is to large, so the distance between x1 and x2 doesn't matter.
    }



    public static T GetComponentInParents<T>(Transform origin)
    {

        Transform parent = origin.parent;
        if (parent != null)
        {
            if (parent.TryGetComponent<T>(out T component))
            {
                return component;
            }
            else
            {
                Debug.Log("It has worked");
                return GetComponentInParents<T>(parent);
            }
        }
        else
        {
            Debug.Log(parent.name + " does not have a parent with a Character Nerve Center");
            return default(T);
        }
    }
}


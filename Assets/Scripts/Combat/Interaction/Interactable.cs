using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;

[RequireComponent(typeof(PlayableDirector))]
[RequireComponent(typeof(Collider))]
public class Interactable : MonoBehaviour
{
    public Object placeholder;
    public Vector3 PositionOffset;
    public Vector3 RotationOffset;
    PlayableDirector playableDirector;
    Object[] keys;
    private void Awake()
    {
        Debug.Assert(gameObject.layer == LayerMask.NameToLayer("Interactable"));
        playableDirector = GetComponent<PlayableDirector>();


        var obj = new SerializedObject(playableDirector);
        var bindings = obj.FindProperty("m_SceneBindings");
        
        keys = new Object[bindings.arraySize+1];
        if (bindings.arraySize > 0)
        {
            var outputs = playableDirector.playableAsset.outputs;
            int i = 0;
            foreach (var output in outputs)
            {
                keys[i] = output.sourceObject;
                
                Debug.Log(i + "    " + output.sourceObject.name);
                i++;
            }
        }




    }
    public static void DisplayBindings(PlayableDirector director)
    {
        var obj = new SerializedObject(director);
        var bindings = obj.FindProperty("m_SceneBindings");
        for (int i = 0; i < bindings.arraySize; i++)
        {
            var binding = bindings.GetArrayElementAtIndex(i);
            var trackProp = binding.FindPropertyRelative("key");
            var sceneObjProp = binding.FindPropertyRelative("value");
            var track = trackProp.objectReferenceValue;
            var sceneObj = sceneObjProp.objectReferenceValue;


            Debug.LogFormat("Binding {0} {1}", track != null ? track.name : "Null", sceneObj != null ? sceneObj.name : "Null");
        }
    }
    public virtual void Activate(Animator animator)
    {
        Debug.Assert(playableDirector.duration > 0);
        if (playableDirector.duration == 0 || playableDirector.state == PlayState.Playing) return;
        
        GameObject initiator = animator.gameObject;
       
        Vector3 characterPos = transform.position + transform.rotation * PositionOffset;
        initiator.transform.position = characterPos;
        Vector3 interactablePos = transform.position;
        Vector3 direction = interactablePos - characterPos;
        direction.y = 0;
        initiator.transform.rotation = Quaternion.LookRotation(direction, Vector3.up);

        playableDirector.ClearGenericBinding(keys[0]);
        playableDirector.SetGenericBinding(keys[0], initiator);

        playableDirector.Play();
        Debug.Log(gameObject.name + " HAS BEEN ACTIVATED!");


        animator.speed = 0;
        playableDirector.stopped += x => animator.speed = 1;
            
        

    }
}

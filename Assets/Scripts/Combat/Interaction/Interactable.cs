using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Playables;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(PlayableDirector))]
[RequireComponent(typeof(Collider))]
[ExecuteInEditMode]
public class Interactable : MonoBehaviour
{
    public Object placeholder;
    public Vector3 PositionOffset;
    public Vector3 RotationOffset;
    [HideInInspector]
    [SerializeField]
    protected PlayableDirector playableDirector;
    [HideInInspector]
    [SerializeField]
    protected Object[] keys;


    private void Awake()
    {
        if (Application.isPlaying)
        { 
            //Debug.Assert(gameObject.layer == LayerMask.NameToLayer("Interactable"));
        }
#if UNITY_EDITOR
        GatherPlayableDirectorData();
#endif
    }

    //Only available in editor
#if UNITY_EDITOR
    private void GatherPlayableDirectorData()
    {
        Debug.Log("Updateing interactable keys.");
        playableDirector = GetComponent<PlayableDirector>();
        var obj = new SerializedObject(playableDirector);
        var bindings = obj.FindProperty("m_SceneBindings");

        keys = new Object[bindings.arraySize + 1];
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
#endif


    public virtual void Activate(Animator animator)
    {
        if (Application.isPlaying)
        {
            Debug.Assert(playableDirector.duration > 0);
            if (playableDirector.duration == 0 || playableDirector.state == PlayState.Playing) return;

            StartCoroutine(ActivateInitiator(animator));
            if (transform.parent!=null && transform.parent.TryGetComponent<AnimatorScriptControl>(out _))//if they have an animator script control they must have an animator
            {
                StartCoroutine(ActivateSource(transform.parent.GetComponent<Animator>()));
            }
        }
    }



    float smoothTime= 0.25f;
    IEnumerator ActivateInitiator(Animator animator)
    {
        
        animator.Play("AutoInteract");
        animator.speed = 0;
        
        GameObject initiator = animator.gameObject;

        initiator.GetComponent<AnimatorScriptControl>().cameraAnimationWeightTarget = 0;
        

        Vector3 initialPosition = initiator.transform.position;
        Quaternion initialRotation = initiator.transform.rotation;
        initiator.transform.SetPositionAndRotation(initialPosition, initialRotation);
       

        Vector3 targetPosition = transform.position + transform.rotation * PositionOffset;
        Vector3 direction = transform.position - targetPosition;
        direction.y = 0;
        Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);


        Debug.Log(gameObject.name + " HAS BEEN ACTIVATED!");


        float timer=0;
        while (timer <= smoothTime)
        {
            timer += Time.deltaTime;
            Vector3 currentPos = Vector3.Slerp(initialPosition-transform.position, targetPosition-transform.position, timer / smoothTime);
            Quaternion currentRot = Quaternion.Slerp(initialRotation, targetRotation, timer / smoothTime);
            initiator.transform.SetPositionAndRotation(currentPos+transform.position, currentRot);


            yield return null;
        }
        initiator.GetComponent<CharacterController>().enabled = false;


        animator.gameObject.GetComponent<AnimatorScriptControl>().cameraAnimationWeightTarget = 1;
        playableDirector.ClearGenericBinding(keys[0]);
        playableDirector.SetGenericBinding(keys[0], initiator);
        playableDirector.Play();
        playableDirector.stopped += x => EndInteraction(animator);



    }
    IEnumerator ActivateSource(Animator animator)
    {

        animator.Play("AutoInteract");
        animator.speed = 0;
        GameObject source = animator.gameObject;
        source.GetComponent<AnimatorScriptControl>().cameraAnimationWeightTarget = 0;

        float timer = 0;
        while (timer <= smoothTime)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        source.GetComponent<CharacterController>().enabled = false;

        animator.gameObject.GetComponent<AnimatorScriptControl>().cameraAnimationWeightTarget = 1;
        playableDirector.ClearGenericBinding(keys[1]);
        playableDirector.SetGenericBinding(keys[1], source);
        playableDirector.Play();
        playableDirector.stopped += x => EndInteraction(animator);
    }
    private void EndInteraction(Animator animator)
    {
       
        if (gameObject != null&& gameObject.activeInHierarchy)
        {
            animator.gameObject.GetComponent<CharacterController>().enabled = true;
            animator.Rebind();
            animator.Update(0f);
            animator.speed = 1;
        }
    }
}

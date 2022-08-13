using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Behavior : MonoBehaviour
{
 
    public float TimeStep = 1f;
    public List<Action> actions;
    private string currentBehavior;
    public ref readonly string CurrentBehavior => ref currentBehavior;
    private KnowledgeBase knowledgeBase;
    
    private void Awake()
    {
        knowledgeBase = GetComponent<KnowledgeBase>();
    }
    void Start()
    {
        StartCoroutine(UpdateBehavior());
    }

    IEnumerator UpdateBehavior()
    {
        while (true)
        {
            currentBehavior = DecideAction();
            if (GetComponent<CharacterNerveCenter>().IsPlayer == false)
            {
                Debug.Log(currentBehavior);
            }
            yield return new WaitForSeconds(TimeStep);
        }
    }

    private string DecideAction()
    {
        knowledgeBase.UpdateSighting();
        (string, float) choice;
        choice.Item1 = "Error";
        choice.Item2 = 0f;
        foreach (Action action in actions)
        {
            float actionValue = 1f;
            foreach (Axis axis in action.axis)
            {
                float axisValue = knowledgeBase.GetAxisValue(axis.AxisType);
                axisValue = axis.AxisWeight.Evaluate(axisValue);
                actionValue *= axisValue;
                Debug.Assert(axisValue <= 1f && axisValue >= 0f);
            }
            actionValue = actionValue * action.ActionWeight;
            if (actionValue > choice.Item2)
            {
                choice.Item1 = action.ActionName;
                choice.Item2 = actionValue;
            }

        }
        if (choice.Item2 == 0f)
        {
            Debug.LogError("Warning no action selected!");
            
        }
        return choice.Item1;
    }


    [System.Serializable]
    public struct Action
    {
        public string ActionName;
        public float ActionWeight;
        public List<Axis> axis;
    }
    [System.Serializable]
    public struct Axis
    {
        public KnowledgeBase.UtilityAxis AxisType;
        public AnimationCurve AxisWeight;
    }

}

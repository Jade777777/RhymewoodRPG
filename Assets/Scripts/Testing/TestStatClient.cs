using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//[CreateAssetMenu(fileName = "Test", menuName = "Test/Test", order = 1)]
public class TestStatClient : MonoBehaviour
{

    private StatComposite whichStats;

    private StatComposite otherStats ;


    private void OnEnable()
    {
        if (whichStats == null)
        {
            whichStats = new StatComposite();
            otherStats = new StatComposite();
            FlatStatLeaf fsl = new("stat", 5f);
            FlatStatLeaf different = new FlatStatLeaf( "WOW!", 9f);
            MultStatLeaf msl = new MultStatLeaf( "stat", "WOW!", 0.5f);
            FlatStatLeaf newfsl = new FlatStatLeaf("newStat", 10);
            MultStatLeaf missing = new MultStatLeaf( "newStat", "huh", 2f);
            otherStats.Add(different);
            otherStats.Add(newfsl);
            otherStats.Add(missing);
            
            //otherStats.Remove(this);
            
            whichStats.Add(fsl);
            whichStats.Add(msl);
            whichStats.Add(different);
            whichStats.Add(otherStats);
            whichStats.Add(otherStats);
         
            foreach(KeyValuePair<string,float> kvp in whichStats.Stats())
            {
                Debug.Log(kvp.Key + ":   " + kvp.Value);

            }

            whichStats.Add(whichStats);
            StatComposite newWhich = JadeUtility.CreateDeepCopy(whichStats);

            whichStats.Remove(fsl.ID);

            Debug.Log("___________ORIGINAL____________");
            foreach (KeyValuePair<string, float> kvp in whichStats.Stats())
            {
                Debug.Log(kvp.Key + ":   " + kvp.Value);

            }
            Debug.Log("_____________COPY______________");
            foreach (KeyValuePair<string, float> kvp in newWhich.Stats())
            {
                Debug.Log(kvp.Key + ":   " + kvp.Value);

            }
            Debug.Log(whichStats.ID+"   "+ newWhich.ID);
        }
    }
}

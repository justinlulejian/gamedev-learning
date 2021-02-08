using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cube : MonoBehaviour
{
    // Start is called before the first frame update
    void OnEnable()
    {
        Main.onCompleteRoutine += DoStuffAfterMainRoutineComplete;
    }

    private void DoStuffAfterMainRoutineComplete()
    {
        Debug.Log("Hey I'm the Cube and now I can do stuff now that that Main routine is donezo!!! Callbacks woooooo");
    }

    private string CutCharactersAndReturnLength(string s)
    {
        // Debug.Log($"cutting length from string in cube: {this.GetInstanceID().ToString()}");
        string n = s.Substring(0, s.Length - 1) + this.GetInstanceID().ToString();
        Debug.Log($"cube: {this.GetInstanceID().ToString()} new string returned is: {n}");
        return n;
    } 

    private void ChangeCubePosition(Vector3 pos)
    {
        transform.position = pos;
    }

    private void OnDisable()
    {
        Main.onCompleteRoutine -= DoStuffAfterMainRoutineComplete;
    }
}

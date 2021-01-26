using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    void Start()
    {
        string difficultySelection =  MainMenu.difficultySelection;
        Debug.Log($"difficulty selection by user is: {difficultySelection}");
    }

    void Update()
    {
        
    }
}

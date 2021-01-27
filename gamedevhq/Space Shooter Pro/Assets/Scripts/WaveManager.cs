using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    void Start()
    {
        string difficultySelection = String.Empty;
        difficultySelection =  MainMenu.difficultySelection != null? MainMenu.difficultySelection : String.Empty;
        Debug.Log($"difficulty selection by user is: {difficultySelection}");

        if (string.IsNullOrEmpty(difficultySelection))
        {
            Debug.Log("Difficulty selection was not specified, defaulting to easy.");
            difficultySelection = "easy";
        }
    }

    void Update()
    {
        
    }
}

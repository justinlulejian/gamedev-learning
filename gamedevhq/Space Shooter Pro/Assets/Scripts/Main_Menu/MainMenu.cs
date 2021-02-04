using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField]
    private GameObject _difficultyChoices;

    public static string difficultySelection;

    private void Start()
    {
        if (_difficultyChoices == null)
        {
            Debug.LogError("Difficulty choice container is null from Main menu, setting" +
                           " difficulty might fail.");
        }
    }

    public void PresentDifficultyChoice()
    {
        GameObject newGameButton = GameObject.Find("New_Game_Button");
        newGameButton.SetActive(false);
        _difficultyChoices.SetActive(true);
    }

    public void LoadGameWithDifficultyChoice(string difficulty)
    {
        switch (difficulty.ToLower())
        {
            case "easy":
                difficultySelection = "easy";
;                break;
            case "normal":
                difficultySelection = "normal";
                break;
            case "insane":
                difficultySelection = "insane";
                break;
            default:
                Debug.LogError("easy difficulty chosen by default");
                break;
        }
        SceneManager.LoadSceneAsync("Scenes/Game");
    }


    public void LoadGame()
    {
        SceneManager.LoadSceneAsync("Scenes/Game");
    }
}


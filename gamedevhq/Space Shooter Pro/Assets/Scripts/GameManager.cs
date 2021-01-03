using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] 
    private bool _gameOver = false;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    public bool IsGameOver => _gameOver;

    public void GameOver()
    {
        _gameOver = true;
        StartCoroutine(WaitForRestartRoutine());
        
    }

    private IEnumerator WaitForRestartRoutine()
    {
        while (!Input.GetKeyDown(KeyCode.R))
        {
            yield return null;
        }
        SceneManager.LoadSceneAsync("Scenes/Game");
        yield return null;
    }
    
}

﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] 
    private bool _gameOver = false;

    private UIManager _uiManager;
    private SpawnManager _spawnManager;

    private void Start()
    {
        _uiManager = GameObject.Find("UI_Manager").GetComponent<UIManager>();
        _spawnManager = GameObject.Find("Spawn_Manager").GetComponent<SpawnManager>();
        
        if (_uiManager == null)
        {
            Debug.LogError("Couldn't find UI manager from Game manager.");
        }
        if (_uiManager == null)
        {
            Debug.LogError("Couldn't find Spawn Manager from Game manager.");
        }
    }

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
        _uiManager.GameOverUI();
        StartCoroutine(WaitForRestartRoutine());
        
    }
    
    public void GameWin()
    {
        _gameOver = true;
        _uiManager.GameWinUI();
        StartCoroutine(WaitForRestartRoutine());
        
    }

    private IEnumerator WaitForRestartRoutine()
    {
        while (!Input.GetKeyDown(KeyCode.R))
        {
            yield return null;
        }
        _spawnManager.DestroyEnemiesAndPowerUps();
        SceneManager.LoadSceneAsync("Scenes/Main_Menu");
        yield return null;
    }
    
}

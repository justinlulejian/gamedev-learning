using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] 
    private bool _isGameOver = false;

    public void GameOver()
    {
        _isGameOver = true;
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

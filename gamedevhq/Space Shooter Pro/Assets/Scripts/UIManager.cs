using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] 
    private Text _scoreText;
    [SerializeField] 
    private Text _gameOverText;
    [SerializeField] 
    private Text _restartText;
    [SerializeField]
    private Image _livesImage;
    [SerializeField]
    private Sprite[] _liveSprites;

    private GameManager _gameManager;
    
    // Start is called before the first frame update
    void Start()
    {
        _scoreText.text = "Score: 0";
        _gameOverText.gameObject.SetActive(false);
        _gameManager = GameObject.Find("Game_Manager").GetComponent<GameManager>();
        if (!_gameManager)
        {
            Debug.LogError("Couldn't find GameManager from UIManager");
        }
    }

    public void UpdateScore(int playerScore)
    {
        _scoreText.text = "Score: " + playerScore.ToString();
    }

    public void UpdateLives(int currentLives)
    {
        _livesImage.sprite = _liveSprites[currentLives];

        if (currentLives < 1)
        {
            _gameOverText.gameObject.SetActive(true);
            StartCoroutine(GameOverFlickerRoutine());
            _restartText.gameObject.SetActive(true);
            _gameManager.GameOver();
            
        }
    }

    private IEnumerator GameOverFlickerRoutine()
        {
            while (true)
            { 
                _gameOverText.gameObject.SetActive(false);
                yield return new WaitForSeconds(0.5f);
                _gameOverText.gameObject.SetActive(true);
                yield return new WaitForSeconds(0.5f);
            }
        }
    
   
}

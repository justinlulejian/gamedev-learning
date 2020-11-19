using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] 
    private Text _scoreText;
    [SerializeField] 
    private Text _ammoCountText;
    [SerializeField] 
    private Text _gameOverText;
    [SerializeField] 
    private Text _restartText;
    [SerializeField]
    private Image _livesImage;
    [SerializeField]
    private Sprite[] _liveSprites;

    private GameManager _gameManager;
    private Player _player;
    
    // Start is called before the first frame update
    void Start()
    {
        _scoreText.text = "Score: 0";
        _gameOverText.gameObject.SetActive(false);
        _gameManager = GameObject.Find("Game_Manager").GetComponent<GameManager>();
        _player = GameObject.Find("Player").GetComponent<Player>();
        
        if (_gameManager == null)
        {
            Debug.LogError("Couldn't find GameManager from UIManager");
        }
        if (_player != null)
        {
            UpdateAmmoCount(_player.GetAmmoCount());
        }
        else
        {
            Debug.LogError("Couldn't find Player from UIManager");
        }
    }

    public void UpdateScore(int playerScore)
    {
        _scoreText.text = "Score: " + playerScore.ToString();
    }
    
    public void UpdateAmmoCount(int ammoCount)
    { 
        _ammoCountText.text = $"Ammo: {ammoCount.ToString()}";
        if (ammoCount < 1)
        {
            NotifyPlayerNoAmmo(ammoCount);
        }
    }
    
    private void NotifyPlayerNoAmmo(int ammoCount)
    {
        StartCoroutine(AmmoCountFlickerRoutine(ammoCount));
    }
    
    // TODO(Improvement): Try slightly stretching text to be larger then smaller in
    // animation to get player's attention that they are out of ammo. And/or create a
    // sound to also indicate it's out.
    private IEnumerator AmmoCountFlickerRoutine(int ammoCount)
    {
        while (ammoCount < 1 && !_gameManager.IsGameOver)
        { 
            _ammoCountText.gameObject.SetActive(false);
            yield return new WaitForSeconds(0.5f);
            _ammoCountText.gameObject.SetActive(true);
            yield return new WaitForSeconds(0.5f);
        }
    }

    public void UpdateLives(int currentLives)
    {
        if (currentLives >= 0)
        { 
            _livesImage.sprite = _liveSprites[currentLives];
        }
        else  // Prevent damage beyond current lives causing us to go out of range of index.
        {
            _livesImage.sprite = _liveSprites[0];
        }

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

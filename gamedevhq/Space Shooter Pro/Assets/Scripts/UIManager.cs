using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] 
    private Text _scoreText;
    [SerializeField] 
    private Text _ammoCountText;
    private int maximumAmmoCount;
    [SerializeField] 
    private Text _gameOverText;
    [SerializeField] 
    private Text _gameWinText;
    [SerializeField] 
    private Text _restartText;
    [SerializeField]
    private Image _livesImage;
    [SerializeField]
    private Sprite[] _liveSprites;
    [SerializeField] 
    private GameObject _thrusterBar;
    private GameObject _thrusterBarFill;  // Object that fills the bar with a color.
    private Slider _thrusterBarSlider;  // Controls how filled the bar is with the color.
    // Whether thruster bar is restoring, controls player's ability to use thrusters.
    private bool _thrusterRestoring = false;

    private GameManager _gameManager;
    private Player _player;
    
    // Start is called before the first frame update
    void Start()
    {
        _scoreText.text = "Score: 0";
        _gameOverText.gameObject.SetActive(false);
        _gameWinText.gameObject.SetActive(false);
        _gameManager = GameObject.Find("Game_Manager").GetComponent<GameManager>();
        _player = GameObject.Find("Player").GetComponent<Player>();
        _thrusterBarSlider = _thrusterBar.GetComponent<Slider>();
        _thrusterBarFill = GameObject.FindWithTag("ThrusterBarFill");

        if (_gameOverText == null || _gameWinText == null)
        {
            Debug.LogError("Couldn't find game win and/or game over text from UIManager");
        }
        
        if (_gameManager == null)
        {
            Debug.LogError("Couldn't find GameManager from UIManager");
        }
        if (_player == null)
        {
            Debug.LogError("Couldn't find Player from UIManager");
        }
        if (_thrusterBarSlider == null)
        {
            Debug.LogError("Couldn't find thrusterBar slider from UIManager");
        }
        if (_thrusterBarFill == null)
        {
            Debug.LogError("Couldn't find thrusterBar fill from UIManager");
        }
    }

    public void UpdateScore(int playerScore)
    {
        _scoreText.text = "Score: " + playerScore.ToString();
    }
    
    public void UpdateAmmoCount(int ammoCount)
    {
        _ammoCountText.text = $"Ammo: {ammoCount.ToString()} / {_player.GetMaximumAmmoCount().ToString()}";
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
            GameOverUI();
        }
    }

    // TODO(Improvement): These two ui methods should be inverted where callers call gamemanager,
    // which then calls UI manager to display relevant UI.
    private void GameOverUI()
    {
        StartCoroutine(GameTextFlickerRoutine(_gameOverText));
        _restartText.gameObject.SetActive(true);
        _gameManager.GameOver();
    }
    
    public void GameWinUI()
    {
        StartCoroutine(GameTextFlickerRoutine(_gameWinText));
        _restartText.gameObject.SetActive(true);
        _gameManager.GameOver();
    }

    public bool IsThrusterBarRestoring()
    {
        return _thrusterRestoring;
    }
    
    public void SetThrusterBarValue(float thrusterTimeRemaining)
    {
        float percentThrusterRemaining = thrusterTimeRemaining / _player.GetThrusterTimeSeconds();
        _thrusterBarSlider.value = _thrusterBarSlider.maxValue * percentThrusterRemaining;
    }

    public void InitiateThrusterCooldown()
    {
        // TODO(Improvement): This check is necessary to prevent the cooldown coroutines from being
        // called on every Update() loop. Possible to do it in a simpler way?
        if (!_thrusterRestoring)
        {
            _thrusterRestoring = true;
            // Clamp the value to 0f max in case we undershoot into a negative float value.
            _thrusterBarSlider.value = Mathf.Clamp(
                _thrusterBarSlider.value, _thrusterBarSlider.minValue, _thrusterBarSlider.maxValue);
            ThrusterBarCooldown();
        }
    }
    
    private void ThrusterBarCooldown()
    {
        StartCoroutine(ThrusterBarFlickerRoutine());
        StartCoroutine(ThrusterBarRestore());
    }
    
    private IEnumerator ThrusterBarFlickerRoutine()
    {
        while (_thrusterRestoring)
        { 
            _thrusterBarFill.SetActive(false);
            yield return new WaitForSeconds(0.1f);
            _thrusterBarFill.SetActive(true);
            yield return new WaitForSeconds(0.1f);
        }
    }
    
    // This restores the Thruster bar in 5 seconds as a cooldown.
    private IEnumerator ThrusterBarRestore()
    {
        while (!(Mathf.Approximately(_thrusterBarSlider.value, _thrusterBarSlider.maxValue)))
        {
            yield return new WaitForSeconds(1f);
            _thrusterBarSlider.value += .2f;
        }
        // Clamp the value to 1f max in case we overshoot.
        _thrusterBarSlider.value = Mathf.Clamp(
            _thrusterBarSlider.value, _thrusterBarSlider.minValue, _thrusterBarSlider.maxValue); 
        _thrusterRestoring = false;
        _player.RestoreThrusterTime();
    }

    private IEnumerator GameTextFlickerRoutine(Text gameText)
    {
        while (true)
        { 
            gameText.gameObject.SetActive(true);
            yield return new WaitForSeconds(0.5f);
            gameText.gameObject.SetActive(false);
            yield return new WaitForSeconds(0.5f);
        }
    }
    
   
}

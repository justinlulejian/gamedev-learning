using System;
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
    [SerializeField] 
    private GameObject _thrusterBar;

    private GameObject _thrusterBarFill;

    private Slider _thrusterBarSlider;

    private GameManager _gameManager;
    private Player _player;
    
    // Start is called before the first frame update
    void Start()
    {
        _scoreText.text = "Score: 0";
        _gameOverText.gameObject.SetActive(false);
        _gameManager = GameObject.Find("Game_Manager").GetComponent<GameManager>();
        _player = GameObject.Find("Player").GetComponent<Player>();
        _thrusterBarSlider = _thrusterBar.GetComponent<Slider>();
        _thrusterBarFill = GameObject.FindWithTag("ThrusterBarFill");
        
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
    
    public void SetThrusterBar(float keyHoldDownRemaining)
    {
        float percentThrusterRemaining = keyHoldDownRemaining / 5f;
        _thrusterBarSlider.value = _thrusterBarSlider.value * percentThrusterRemaining;
    }

    public void SetThrusterBarOrCoolDown(float keyHoldDownRemaining)
    {
        if (_thrusterBarSlider.value > 0f)
        {
            SetThrusterBar(keyHoldDownRemaining);
        // TODO: Have to hold down shift for multiple seconds after bar looks empty for this to trigger?
        } else if (Mathf.Approximately(_thrusterBarSlider.value, _thrusterBarSlider.minValue))
        {
            ThrusterBarCooldown();
            // _player.RestoreThrusterBar();
        }
    }
    
    private void ThrusterBarCooldown()
    {
        StartCoroutine(ThrusterBarFlickerRoutine());
        StartCoroutine(ThrusterBarRestore());
    }
    
    private IEnumerator ThrusterBarFlickerRoutine()
    {
        while (_thrusterBarSlider.value < 1f && !(
            Mathf.Approximately(_thrusterBarSlider.value, _thrusterBarSlider.maxValue)))
        { 
            _thrusterBarFill.SetActive(false);
            yield return new WaitForSeconds(0.1f);
            _thrusterBarFill.SetActive(true);
            yield return new WaitForSeconds(0.1f);
        }
        _thrusterBarFill.SetActive(true);
    }
    
    // This waits 3 seconds, then restores the Thruster bar in 5 seconds as a cooldown.
    private IEnumerator ThrusterBarRestore()
    {
        yield return new WaitForSeconds(1f);
        while (!(Mathf.Approximately(_thrusterBarSlider.value, _thrusterBarSlider.maxValue)))
        {
            yield return new WaitForSeconds(1f);
            _thrusterBarSlider.value += .2f;
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

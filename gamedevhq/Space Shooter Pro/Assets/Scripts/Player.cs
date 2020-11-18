using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Player : MonoBehaviour

{
  [SerializeField]
  private float _speed = 5f;
  private float _speedMultipler = 2f;
  [SerializeField]
  private GameObject _laserPrefab;
  [SerializeField]
  private GameObject _tripleShotPrefab;
  [SerializeField]
  private float _fireRate = 0.15f;
  private float _canFire = -1f;
  [SerializeField]
  private int _lives = 3;
  private SpawnManager _spawnManager;

  [SerializeField]
  private bool _isTripleShotActive = false;
  [SerializeField]
  private bool _isSpeedBoostActive = false;
  [SerializeField]
  private bool _areShieldsActive = false;

  [SerializeField] 
  private GameObject _shieldsPrefab;

  [SerializeField]
  private GameObject _leftEngineDamage;
  [SerializeField]
  private GameObject _rightEngineDamage;
  private List<GameObject> _damageObjects;

  [SerializeField]
  private AudioSource _audioSource;
  [SerializeField]
  private AudioClip _laserAudioClip;
  [SerializeField]
  private AudioClip _explosionAudioClip;

  [SerializeField] 
  private int _score;
  
  [SerializeField]
  private UIManager _uiManager;

  private Renderer _renderer;
  
  void Start()
  {
    
    _spawnManager = GameObject.Find("Spawn_Manager").GetComponent<SpawnManager>();
    _uiManager = GameObject.Find("Canvas").GetComponent<UIManager>();
    _audioSource = GetComponent<AudioSource>(); 
    _damageObjects = new List<GameObject>{_leftEngineDamage, _rightEngineDamage};
    _renderer = GetComponent<Renderer>();
    _renderer.enabled = true;
    
    transform.position = new Vector3(0, 0, 0);
    
    if (_spawnManager == null)
    {
      Debug.LogError("Spawn manager is null from Player.");
    }
    if (_uiManager == null)
    {
      Debug.LogError("UI manager is null from Player.");
    }
    if (_audioSource == null)
    {
      Debug.LogError("_audioSource was null when creating player");
    }
    if (_renderer == null)
    {
      Debug.LogError("_renderer was null when creating player");
    }
  }

  void Update()
  {
    CalculateMovement();
    if (Input.GetKeyDown(KeyCode.Space) && Time.time > _canFire)
    {
      FireLaser();
    }
  }

  private void CalculateMovement()
  {
    float horizontalInput = Input.GetAxis("Horizontal");
    float verticalInput = Input.GetAxis("Vertical");

    Vector3 direction = new Vector3(horizontalInput, verticalInput, 0);
    transform.Translate(direction * (_speed * Time.deltaTime));

    // Enforce minimum player bound on y to -3.8f.
    transform.position = new Vector3(
      transform.position.x, Mathf.Clamp(transform.position.y, -3.8f, 5.92f));

    // Bound player on x axis to [11.3f, -11.3f]
    if (transform.position.x > 11.3f)
    {
      transform.position = new Vector3(-11, transform.position.y, transform.position.z);
    }
    else if (transform.position.x < -11.3f)
    {
      transform.position = new Vector3(11.3f, transform.position.y, 0);
    }
  }

  void FireLaser()
  {
    _canFire = Time.time + _fireRate;
    if (_isTripleShotActive)
    {
      Instantiate(_tripleShotPrefab, transform.position, Quaternion.identity);
    }
    else
    {
      Instantiate(
        _laserPrefab,
        transform.position + new Vector3(0, 1.05f, 0), Quaternion.identity);
    }


    // TODO: triple shot is higher volume and sounds a little off possible because it plays x3?
    // Is it possible for Tripleshot to just play the sound once but just amp the volume?
    if (_audioSource == null)
    {
      Debug.LogError("_audioSource was null in player firelaser, weird!");
    }

    _audioSource.clip = _laserAudioClip;
    _audioSource.Play();

  }

  private void OnTriggerEnter2D(Collider2D other)
  {
    // TODO: was attempting to try to only damage ship once even when the two
    // enemy lasers hit the ship, but for some reason this never triggers. Even when
    // turning off box colliders and rigidbodies on child laser and enabling both
    // on the parent object...
    // if (other.tag == "EnemyLaser")
    if (other.tag == "Laser")
    {
      Debug.Log("Player collided with: " + other.tag);
      Laser laser = other.GetComponent<Laser>();
      if (laser != null && laser.IsEnemyLaser)
      {
        Debug.Log("laser damaged player");
        Damage();
      }
      // Damage();
    } 
  }

    public void Damage()
    {
      Debug.Log("Player damaged.");
      if (_areShieldsActive)
      {
        _areShieldsActive = false;
        _shieldsPrefab.SetActive(false);
        return;
      }

      // Randomly select one of the damages to enable if lives > 1, otherwise skip
      if (_lives > 1)
      {
        System.Random random = new System.Random();
        int randomIndex = random.Next(_damageObjects.Count);
        _damageObjects[randomIndex].SetActive(true);
        // TODO: this precludes gaining health so change this if that becomes a feature.
        _damageObjects.RemoveAt(randomIndex); // Prevent it from being enabled again .
      }

      _lives--;
      _uiManager.UpdateLives(_lives);
      Debug.Log("Update lives");

      if (_lives < 1)
      {
        Debug.Log("Player death");
        // TODO: this still leaves the damage and the thruster still visible, can I recursively loop through child
        // objects to destroy or disable them like this?
        // _renderer.enabled = false;
        // This below helped with powerups sound on destroy, but doing this causes restart menu not to load...
        // AudioSource.PlayClipAtPoint(_explosionAudioClip, transform.position);
        _spawnManager.OnPlayerDeath();
        Destroy(this.gameObject, _audioSource.clip.length);
      }
    }

    public void TripleShotActive()
    {
      _isTripleShotActive = true;
      StartCoroutine(TripleShotPowerUpExpireRoutine());
    }

    private IEnumerator TripleShotPowerUpExpireRoutine()
    {
      yield return new WaitForSeconds(5f);
      _isTripleShotActive = false;
    }

    public void SpeedBoostPowerupActive()
    {
      _speed *= _speedMultipler;
      _isSpeedBoostActive = true;
      StartCoroutine(SpeedBoostPowerupExpireRoutine());
    }

    private IEnumerator SpeedBoostPowerupExpireRoutine()
    {
      yield return new WaitForSeconds(5f);
      _speed /= _speedMultipler;
      _isSpeedBoostActive = false;
    }

    public void ShieldsPowerupActive()
    {
      _areShieldsActive = true;
      _shieldsPrefab.SetActive(true);
    }

    public int GetScore()
    {
      return _score;
    }

    public void AddToScore(int points)
    {
      _score += points;
      _uiManager.UpdateScore(_score);
    }
    
}


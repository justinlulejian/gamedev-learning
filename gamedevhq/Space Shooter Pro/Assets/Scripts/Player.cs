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
  private int _shieldStrength = 0;

  [SerializeField] 
  private GameObject _shieldsPrefab;

  private SpriteRenderer _shieldsRenderer;

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
  private int _ammoCount;
  
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
    _shieldsRenderer = _shieldsPrefab.GetComponent<SpriteRenderer>();
   
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
    if (_shieldsRenderer == null) {
      Debug.LogError("Sprite renderer on player shield prefab is null.");
    }
  }

  private void Update()
  {
    CalculateMovement(Input.GetKey(KeyCode.LeftShift) ? _speedMultipler : 1);
    if (Input.GetKeyDown(KeyCode.Space) && Time.time > _canFire)
    {
      FireLaser();
    }
  }

  private void CalculateMovement(float speedMultiplier = 1)
  {
    float horizontalInput = Input.GetAxis("Horizontal");
    float verticalInput = Input.GetAxis("Vertical");

    Vector3 direction = new Vector3(horizontalInput, verticalInput, 0);
    transform.Translate(direction * (_speed * speedMultiplier * Time.deltaTime));

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
    if (_ammoCount < 1)
    {
      return;
    }
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

    ReduceAmmoCount();

    // TODO: triple shot is higher volume and sounds a little off possible because it plays x3?
    // Is it possible for Tripleshot to just play the sound once but just amp the volume?
    if (_audioSource == null)
    {
      Debug.LogError("_audioSource was null in Player FireLaser.");
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
      Laser laser = other.GetComponent<Laser>();
      if (laser != null && laser.IsEnemyLaser)
      {
        Damage();
      }
    } 
  }

  public void Damage()
  {
    if (_areShieldsActive)
    {
      DamageShields();
      return;
    }

    // Randomly select one of the damages to enable if lives > 1, otherwise skip
    if (_lives > 1)
    {
      System.Random random = new System.Random();
      int randomIndex = random.Next(_damageObjects.Count);
      _damageObjects[randomIndex].SetActive(true);
      // This precludes gaining health so change this if that becomes a feature.
      _damageObjects.RemoveAt(randomIndex); // Prevent it from being enabled again.
    }

    _lives--;
    _uiManager.UpdateLives(_lives);

    if (_lives < 1)
    {
      _spawnManager.OnPlayerDeath();
      Destroy(this.gameObject, _audioSource.clip.length);
    }
  }
    
    private void DamageShields() {
      _shieldStrength--;
      if (_shieldStrength == 0)
      {
        _areShieldsActive = false;
        _shieldsPrefab.SetActive(false);
        return;
      }
      // Shields can take three hits, then they disappear.
      Color shieldColor = _shieldsRenderer.color;
      shieldColor = new Color(
        shieldColor.r, shieldColor.g, shieldColor.b,
        shieldColor.a - (shieldColor.maxColorComponent / 3.0f));
      _shieldsRenderer.color = shieldColor;
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

    public void CollectAmmo()
    {
      _ammoCount += 1;
      _uiManager.UpdateAmmoCount(_ammoCount);
    }

    private IEnumerator SpeedBoostPowerupExpireRoutine()
    {
      yield return new WaitForSeconds(5f);
      _speed /= _speedMultipler;
      _isSpeedBoostActive = false;
    }

    public void ShieldsPowerupActive()
    {
      _shieldStrength = 3;
      _shieldsRenderer.color = new Color(_shieldsRenderer.color.r, _shieldsRenderer.color.g, _shieldsRenderer.color.b,
        _shieldsRenderer.color.maxColorComponent);
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

    public int GetAmmoCount()
    {
      return _ammoCount;
    }
    
    private void ReduceAmmoCount()
    {
      _ammoCount--;
      _uiManager.UpdateAmmoCount(_ammoCount);
    }
    
}


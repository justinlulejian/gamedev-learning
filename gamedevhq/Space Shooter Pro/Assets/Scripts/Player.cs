using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CoroutineExtensionMethods;

public class Player : MonoBehaviour

{
  [SerializeField]
  private float _speed = 5f;
  private float _defaultPlayerSpeed;
  private float _speedMultipler = 2f;
  [SerializeField]
  private GameObject _laserPrefab;
  [SerializeField]
  private GameObject _tripleShotPrefab;
  [SerializeField]
  private GameObject _missilePrefab;
  [SerializeField]
  private GameObject _shotgunPrefab;
  [SerializeField] 
  private GameObject _projectileContainer;
  [SerializeField]
  private float _fireRate = 0.15f;
  private float _canFire = -1f;
  [SerializeField]
  public float _thrusterTimeSeconds = 5f;  // Time in seconds that thruster can be used before cooldown.
  private float _thrusterTimeSecondsRemaining;
  public static float _minimumThrusterValue = .01f;  // Minimum value below which thrusters are considered disabled
  [SerializeField]
  private int _lives = 3;
  private SpawnManager _spawnManager;

  [SerializeField]
  private bool _isTripleShotActive = false;
  [SerializeField]
  private bool _isSpeedBoostActive = false;
  [SerializeField]
  private bool _isSpeedDecreaseActive = false;
  [SerializeField] 
  private bool _isMissleShotActive = false;
  [SerializeField] 
  private bool _isShotgunShotActive = false;
  [SerializeField]
  private bool _areShieldsActive = false;
  private int _shieldStrength = 0;
  private Coroutine _tripleShotExpireCoroutine;
  private Coroutine _missleExpireCoroutine;
  private Coroutine _shotgunExpireCoroutine;
  private Coroutine _speedBoostExpireCoroutine;
  
  [SerializeField]
  private float _slowedDownPlayerSpeed;
  [SerializeField] 
  private float _spedUpPlayerSpeed;

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
  private AudioClip _missileAudioClip;
  [SerializeField] 
  private AudioClip _shotgunAudioClip;
  [SerializeField] 
  private GameObject _explosionPrefab;

  [SerializeField] 
  private int _score;

  [SerializeField] 
  private bool _isDestroyed = false;

  // TODO(Improvement): Combine with Enemy's impl?
  private float _playerBossLaserCollideTimeTotal;
  private float _playerCollideTimeRedamage = 1f;

  [SerializeField]
  private int _ammoCount;

  private static int _maximumAmmoCount;
  
  [SerializeField]
  private UIManager _uiManager;

  [SerializeField]
  private MainCamera _mainCamera;


  void Start()
  {
    _spawnManager = GameObject.Find("Spawn_Manager").GetComponent<SpawnManager>();
    _uiManager = GameObject.Find("Canvas").GetComponent<UIManager>();
    _audioSource = GetComponent<AudioSource>(); 
    _damageObjects = new List<GameObject>{_leftEngineDamage, _rightEngineDamage};
    _shieldsRenderer = _shieldsPrefab.GetComponent<SpriteRenderer>();
    _mainCamera = GameObject.Find("Main Camera").GetComponent<MainCamera>();
    _maximumAmmoCount = _ammoCount;
    _defaultPlayerSpeed = _speed;
    _spedUpPlayerSpeed = _speed * _speedMultipler;
    _slowedDownPlayerSpeed = _speed / _speedMultipler;
   
    // TODO: change this back once done with boss ai.
    transform.position = new Vector3(0, 0, 0);
    
    _uiManager.UpdateAmmoCount(GetCurrentAmmoCount());

    _thrusterTimeSecondsRemaining = _thrusterTimeSeconds;
    
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
    if (_shieldsRenderer == null) {
      Debug.LogError("Sprite renderer on player shield prefab is null.");
    }
    if (_mainCamera == null) {
      Debug.LogError("Camera is null when creating player.");
    }
    if (_explosionPrefab == null) {
      Debug.LogError("explosionPrefab is null when creating player.");
    }
    if (_projectileContainer == null) {
      Debug.LogError("Projectile container is null when creating player.");
    }
  }

  private void Update()
  {
    if (Input.GetKey(KeyCode.LeftShift) && !_uiManager.IsThrusterBarRestoring())
    {
      // TODO(bug): Using thrusters appears to be slower than speed powerup, ideally they'd be
      // stackable?
      SpeedUpPlayer();
      CalculateMovement();
      // TODO(Improvement): Make thruster bigger when thrusting to give user visual feedback
      // that thruster is going. Return to normal size when in cooldown.
      if (_thrusterTimeSecondsRemaining > _minimumThrusterValue)
      {
        _uiManager.SetThrusterBarValue(_thrusterTimeSecondsRemaining);
        _thrusterTimeSecondsRemaining -= Time.deltaTime;
      }
      else
      {
        ResetPlayerSpeed();
        _uiManager.InitiateThrusterCooldown();
      }
      
    }
    else
    {
      CalculateMovement();
    }
    
    if (Input.GetKeyDown(KeyCode.Space) && Time.time > _canFire)
    {
      FireWeapon();
    }
    
    if (Input.GetKey(KeyCode.C))
    {
      foreach (var powerUp in _spawnManager.GetAllOnScreenPowerUps())
      {
        powerUp.PlayerCollecting(true, this.transform.position);
      }
    }
  }

  public void RestoreThrusterTime()
  {
    _thrusterTimeSecondsRemaining = _thrusterTimeSeconds;
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

  private void SpeedUpPlayer()
  {
    _speed = _spedUpPlayerSpeed;
  }
  
  private void SlowDownPlayer()
  {
    _speed = _slowedDownPlayerSpeed;
  }
  
  private void ResetPlayerSpeed()
  {
    _speed = _defaultPlayerSpeed;
  }

  void FireWeapon()
  {
    _canFire = Time.time + _fireRate;
    if (_isTripleShotActive)
    {
      GameObject tripleShot = InstantiatePrefabAndPlayAudioClip(_tripleShotPrefab, _laserAudioClip);
      foreach (GameObject laser in tripleShot.GetComponent<TripleShot>().GetChildLasers())
      {
        laser.GetComponent<Laser>().LaserDirection = Vector3.up;
        // Adding projectiles as children works fine for now since they're 
        laser.transform.parent = _projectileContainer.transform;
      }
      return;
    } else if (_isMissleShotActive)
    {
      GameObject missileShot = InstantiatePrefabAndPlayAudioClip(
        _missilePrefab, _missileAudioClip, positionOffset:new Vector3(0, 1f, 0));
      missileShot.transform.parent = _projectileContainer.transform;
      return;
    }
    else if (_isShotgunShotActive)
    {
      GameObject _shotgunShot = InstantiatePrefabAndPlayAudioClip(
        _shotgunPrefab, _shotgunAudioClip, positionOffset:new Vector3(0, 1f, 0));
      foreach (GameObject shotgunShot in _shotgunShot.GetComponent<ShotgunShot>().GetShotgunShots())
      {
        _shotgunShot.transform.parent = _projectileContainer.transform;
      }
      return;
    }
    
    // Normal lasers can't fire when ammo is 0, but that doesn't apply to powerups.
    if (_ammoCount > 0)
    {
      GameObject laser = InstantiatePrefabAndPlayAudioClip(
        _laserPrefab, _laserAudioClip, positionOffset: new Vector3(0, 1.05f, 0));
      laser.GetComponent<Laser>().LaserDirection = Vector3.up;
      laser.transform.parent = _projectileContainer.transform;
      ReduceAmmoCount();
    }
  }

  public List<Transform> GetProjectileTransforms()
  {
    List<Transform> projectiles = new List<Transform>();
    for (int i = 0; i < _projectileContainer.transform.childCount; i++)
    {
      Transform projectile = _projectileContainer.transform.GetChild(i);
      if (projectile)
      {
        projectiles.Add(projectile);
      }
    }
    return projectiles;
  }

  private GameObject InstantiatePrefabAndPlayAudioClip(
    GameObject prefab, AudioClip audioClip, Vector3 positionOffset = default)
  {
    GameObject instantiatedPrefab = Instantiate(
      prefab,
      transform.position + positionOffset, Quaternion.identity);
    _audioSource.clip = audioClip;
    
    // TODO(Improvement): triple shot is higher volume and sounds a little off possible because it plays x3?
    // Is it possible for Tripleshot to just play the sound once but just amp the volume?
    if (_audioSource == null)
    {
      Debug.LogError("_audioSource was null in Player FireWeapon.");
    }
    _audioSource.Play();
    return instantiatedPrefab;
  }

  private void OnTriggerEnter2D(Collider2D other)
  {
    // TODO(Improvement): was attempting to try to only damage ship once even when the two
    // enemy lasers hit the ship, but for some reason this never triggers. Even when
    // turning off box colliders and rigidbodies on child laser and enabling both
    // on the parent object...
    // if (other.tag == "EnemyLaser")
    if (other.CompareTag("Laser"))
    {
      Laser laser = other.GetComponent<Laser>();
      if (laser != null && laser.IsEnemyLaser)
      {
        Damage();
      }
    }
    if (other.CompareTag("BossLaser"))
    {
      Damage();
    }
    if (other.CompareTag("BossCircleShot"))
    {
      Damage();
    }
    
  }
  
  private void OnTriggerStay2D(Collider2D other)
  {
    if (other.CompareTag("BossLaser"))
    {
      _playerBossLaserCollideTimeTotal += Time.deltaTime;
      if (_playerBossLaserCollideTimeTotal >= _playerCollideTimeRedamage) 
      {
        Damage();
        _playerBossLaserCollideTimeTotal = 0f;
      }
    }
  }
  
  private void OnTriggerExit2D(Collider2D other)
  {
    _playerBossLaserCollideTimeTotal = 0f;
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
      ShowDamage();
    }
    
    _mainCamera.ShakeCamera(2f);
    
    _lives--;
    _uiManager.UpdateLives(_lives);

    if (_lives < 1)
    {
      _spawnManager.OnPlayerDeath();
      OnPlayerDestroyed();
    }
  }

  public bool IsDestroyed => _isDestroyed;

  private void OnPlayerDestroyed()
  {

    // TODO(Improvement): Enemy two-laser hitting can cause explosion prefab to instantiate twice, 
    // could be remedied by detected enemy laser parent object vs each laser individually.
    if (!_isDestroyed)
    {
      Instantiate(_explosionPrefab, transform.position, Quaternion.identity);
      _isDestroyed = true;
    }
    Destroy(this.gameObject, 0.10f);
  }
  
  private void DamagePrefab(bool enable)
  {
    System.Random random = new System.Random();
    bool damagePrefabActionCompleted = false;
    while (!damagePrefabActionCompleted)
    {
      int randomIndex = random.Next(_damageObjects.Count);
      bool prefabActive = _damageObjects[randomIndex].activeSelf;
      if (prefabActive != enable)
      {
        _damageObjects[randomIndex].SetActive(enable);
        damagePrefabActionCompleted = true;
      }
    }
  }
  private void ShowDamage()
  {
    DamagePrefab(true);
  }

  private void RemoveDamage()
  {
    DamagePrefab(false);
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
    this.RestartCoroutine(TripleShotPowerUpExpireRoutine(), ref _tripleShotExpireCoroutine);
  }
  
  // TODO(Improvement): Refactor the common expire routines into a configurable single one.
  private IEnumerator TripleShotPowerUpExpireRoutine()
  {
    yield return new WaitForSeconds(5f);
    _isTripleShotActive = false;
  }
  
  public void MissilePowerupActive()
  {
    _isMissleShotActive = true;
    this.RestartCoroutine(MissilePowerUpExpireRoutine(), ref _missleExpireCoroutine);
  }

  private IEnumerator MissilePowerUpExpireRoutine()
  {
    yield return new WaitForSeconds(5f);
    _isMissleShotActive = false;
  }
  
  public void ShotgunPowerupActive()
  {
    _isShotgunShotActive = true;
    this.RestartCoroutine(ShotgunPowerupExpireRoutine(), ref _shotgunExpireCoroutine);
  }

  private IEnumerator ShotgunPowerupExpireRoutine()
  {
    yield return new WaitForSeconds(5f);
    _isShotgunShotActive = false;
  }

  public void SpeedBoostPowerupActive()
  {
    SpeedUpPlayer();
    _isSpeedBoostActive = true;
    this.RestartCoroutine(SpeedBoostPowerupExpireRoutine(), ref _speedBoostExpireCoroutine);
  }
  
  private IEnumerator SpeedBoostPowerupExpireRoutine()
  {
    yield return new WaitForSeconds(5f);
    ResetPlayerSpeed();
    _isSpeedBoostActive = false;
  }
  
  // TODO(Improvement): Should thrusters be able to override this decrease entirely or only
  // partially reduce the effect?
  public void SpeedDecreasePowerupActive()
  {
    SlowDownPlayer();
    _isSpeedBoostActive = true;
    this.RestartCoroutine(SpeedBoostPowerupExpireRoutine(), ref _speedBoostExpireCoroutine);
  }
  
  private IEnumerator SpeedDecreasePowerupExpireRoutine()
  {
    yield return new WaitForSeconds(5f);
    ResetPlayerSpeed();
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
  
  public void CollectAmmo()
  {
    if (_ammoCount < _maximumAmmoCount)
    {
      _ammoCount += 5;
      _ammoCount = Mathf.Min(_ammoCount, _maximumAmmoCount);
      _uiManager.UpdateAmmoCount(_ammoCount);
    }
  }
  
  public void CollectLife()
  {
    _lives += 1;
    RemoveDamage();
    _uiManager.UpdateLives(_lives);
  }

  public float GetThrusterTimeSeconds()
  {
    return _thrusterTimeSeconds;
  }
  
  public int GetScore()
  {
    return _score;
  }
  
  public int GetLives()
  {
    return _lives;
  }


  public void AddToScore(int points)
  {
    _score += points;
    _uiManager.UpdateScore(_score);
  }

  public int GetCurrentAmmoCount()
  {
    return _ammoCount;
  }
  
  public int GetMaximumAmmoCount()
  {
    return _maximumAmmoCount;
  }
  
  private void ReduceAmmoCount()
  {
    _ammoCount--;
    _uiManager.UpdateAmmoCount(_ammoCount);
  }
    
}


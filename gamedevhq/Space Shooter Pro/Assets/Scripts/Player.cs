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
  private int _score;
  
  [SerializeField]
  private UIManager _uiManager;
  
  void Start()
  {
    
    _spawnManager = GameObject.Find("Spawn_Manager").GetComponent<SpawnManager>();
    _uiManager = GameObject.Find("Canvas").GetComponent<UIManager>();
    if (_spawnManager == null)
    {
      Debug.LogError("Spawn manager is null from Player.");
    }
    if (_uiManager == null)
    {
      Debug.LogError("UI manager is null from Player.");
    }
    
    transform.position = new Vector3(0, 0, 0);
    
    _damageObjects = new List<GameObject>{_leftEngineDamage, _rightEngineDamage};
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
    transform.Translate(direction * _speed * Time.deltaTime);

    // Enforce minimum player bound on y to -3.8f.
    transform.position = new Vector3(
      transform.position.x, Mathf.Clamp(transform.position.y, -3.8f, 5.92f));

    // bound player on x axis to [11.3f, -11.3f]
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
      // TODO: this precludes gaining health so change this is that becomes a feature.
      _damageObjects.RemoveAt(randomIndex); // Prevent it from being enabled again.
    }
    
    _lives--;
    _uiManager.UpdateLives(_lives);

    if (_lives < 1)
    {
      Debug.Log("Player death");
      _spawnManager.OnPlayerDeath();
      Destroy(this.gameObject);
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

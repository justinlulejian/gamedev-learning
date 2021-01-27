﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class SpawnManager : MonoBehaviour
{
  [SerializeField]
  private GameObject[] _enemyTypes;
  [SerializeField]
  private GameObject _bossPrefab;
  [SerializeField]
  private GameObject _enemyContainerObj;

  private EnemyContainer _enemyContainer;
  // TODO(Improvement): Change the ammo pickup prefab to the ammo box sprite I have in the proj.
  [SerializeField]
  private GameObject _powerupContainer;
  private List<Powerup> _powerupObjContainer;
  [SerializeField]
  private GameObject[] _powerups;
  [SerializeField]
  private float[] _powerUpWeights;  // Weights to use when calculating spawn rate.
  
  [SerializeField]
  private bool _stopSpawning = false;
  
  [SerializeField]
  private Player _player;
  
  [SerializeField]
  private UIManager _uiManager;

  [SerializeField] 
  private WaveManager _waveManager;
  
  // Enemy spawn logic.
  // Since we use Fibonacci to calculate, numbers beyond 6 get very high. 
  // TODO(bug): Sometimes when _numberOfWavesRemaining == 1 there are two enemies/two waves that spawn?
  [SerializeField] 
  private int _numberOfEnemyWaves = 6;
  private int _waveNumber; // What wave we are currently on.
  [SerializeField] 
  private String _difficulty = "normal";
  private Dictionary<string, float> _difficultyToFactorMap = new Dictionary<string, float>()
    {{"easy", 0.5f}, {"normal", 1f}, {"insane", 2f}};
  private int _enemyWaveNumber = 1;

  // These should be double for precision on calculation later, but Mathf.Pow only takes floats?
  // NBD in any case.
  private static float _Phi = (Mathf.Sqrt(5f) + 1f) / 2f;
  private static float _phi = 1f / _Phi;

  private void Start()
  {
    _uiManager = GameObject.Find("Canvas").GetComponent<UIManager>();
    _player = GameObject.Find("Player").GetComponent<Player>();
    _powerupObjContainer = new List<Powerup>();
    _enemyContainer = _enemyContainerObj.GetComponent<EnemyContainer>();
    _waveManager = GameObject.Find("Wave_Manager").GetComponent<WaveManager>();

    if (_uiManager == null)
    {
      Debug.LogError("Couldn't find UiManager from SpawnManager");
    }
    if (_player == null)
    {
      Debug.LogError("Player is null from SpawnManager.");
    }
    if (_enemyContainer == null)
    {
      Debug.LogError("Enemy container is null from SpawnManager.");
    }
    if (_enemyTypes.Length == 0)
    {
      Debug.LogError("No enemy types were provide to SpawnManager. Enemies might not spawn.");
    }
    if (_waveManager == null)
    {
      Debug.LogError("Couldn't find WaveManager from SpawnManager");
    }
  }

  public void StartSpawning()
  {
    StartCoroutine(SpawnWaveRoutine());
  }



  private IEnumerator SpawnWaveRoutine()
  {
    while (_waveNumber <= _numberOfEnemyWaves && _stopSpawning == false)
    {
      yield return new WaitUntil(() => _waveManager.NoWavesRunning());

      _waveManager.SpawnWave(_waveNumber);
      _waveNumber++;
    }

    // Wait until all enemies in the final wave have been cleared before proceeding.
    yield return new WaitUntil(() => _waveManager.NoWavesRunning());
      // TODO(Improvement): Change music to boss music on spawn, then play success music on win.
    _waveManager.SpawnBossWave();
    
    // Display win for player since all waves appear to be done, but only if Player survived.
    yield return new WaitUntil(() => _enemyContainer.NoEnemies() && !_player.IsDestroyed);
    _stopSpawning = true;
    DestroyEnemiesAndPowerUps();
    _uiManager.GameWinUI();
  }
  
  private IEnumerator SpawnPowerupRoutine()
  {
    yield return new WaitForSeconds(3.0f);
    
    // TODO(Bug): where powerup will still spawn if we're waiting while player death
    while (_stopSpawning == false)
    {
      float randomSpawnTime = Random.Range(3, 8);
      GameObject newPowerup = Instantiate(ChooseWeightedRandomPowerUp(),
                                          new Vector3(Random.Range(-8f, 8f), 7, 0),
                                          Quaternion.identity);
      newPowerup.transform.parent = _powerupContainer.transform;
      _powerupObjContainer.Add(newPowerup.GetComponent<Powerup>());
      yield return new WaitForSeconds(randomSpawnTime);
    } 
  }

  private GameObject ChooseWeightedRandomPowerUp()
  {
    // TODO(Improvement): Do not spawn powerups that are useless to the player. E.g. if they have shields, max ammo,
    // max health, etc. do not spawn them. Powerups that time out should still be collectible though.
    
    // Linear scan algo from: https://blog.bruce-hill.com/a-faster-weighted-random-choice
    float remainingDistance = Random.value * _powerUpWeights.Sum();
    for (int i = 0; i < _powerUpWeights.Length; i++)
    {
      remainingDistance -= _powerUpWeights[i];
      if (remainingDistance < 0)
      {
        return _powerups[i];
      }
    }
    Debug.LogError("Weighted random choice of powerups failed to find a value.");
    // Fallback to non-weighted random choice if scan fails.
    return _powerups[Random.Range(0, _powerups.Length)];
  }

  public List<Powerup> GetAllOnScreenPowerUps()
  {
    List<Powerup> existentPowerups = new List<Powerup>();
    foreach (Powerup powerUp in _powerupObjContainer)
    {
      if (powerUp != null)
      {
        existentPowerups.Add(powerUp);
      }
    }
    return existentPowerups;
  }
  
  public List<Enemy> GetAllOnScreenEnemies()
  {
    return _enemyContainer.GetEnemies();
  }
  
  public void RemoveEnemyFromGame(Enemy enemy, float afterTime)
  {
    _enemyContainer.RemoveEnemy(enemy, afterTime);
  }

  public void RemovePowerUpFromGame(Powerup powerUp)
  {
    _powerupObjContainer.Remove(powerUp);
    // By destroying powerup it is removed from _powerupContainer.
    Destroy(powerUp.gameObject);
  }

  private void DestroyEnemiesAndPowerUps()
  {
    _enemyContainer.RemoveAllEnemies();
    
    foreach (Transform powerUp in _powerupContainer.transform)
    {
      Destroy(powerUp.gameObject);
    }
    // TODO: Delete all laser and missile objects otherwise they (funnily) just float around after
    // gameover if they are present when player dies. Now possible with weapons manager?.
  }

  public void OnPlayerDeath()
  {
    _stopSpawning = true;
    DestroyEnemiesAndPowerUps();
  }
}
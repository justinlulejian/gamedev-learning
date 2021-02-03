using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class SpawnManager : MonoBehaviour
{
  [SerializeField]
  private GameObject _enemyContainerObj;
  private EnemyContainer _enemyContainer;
  
  [SerializeField]
  private GameObject _powerupContainer;
  private List<Powerup> _powerupObjContainer;

  [SerializeField]
  private bool _stopSpawning = false;
  
  private Player _player;
  private WaveManager _waveManager;
  private GameManager _gameManager;
  
  // Enemy spawn logic.
  // Since we use Fibonacci to calculate, numbers beyond 6 get very high. 
  // TODO(bug): Sometimes when _numberOfWavesRemaining == 1 there are two enemies/two waves that spawn?
  [SerializeField] 
  private int _numberOfEnemyWaves = 6;
  // TODO: test what happens when _numberOfEnemyWaves == 0.
  private int _waveNumber; // What wave we are currently on.

  private void Start()
  {
    _player = GameObject.Find("Player").GetComponent<Player>();
    _powerupObjContainer = new List<Powerup>();
    _enemyContainer = _enemyContainerObj.GetComponent<EnemyContainer>();
    _waveManager = GameObject.Find("Wave_Manager").GetComponent<WaveManager>();
    _gameManager = GameObject.Find("Game_Manager").GetComponent<GameManager>(); 
    
    if (_player == null)
    {
      Debug.LogError("Player is null from SpawnManager.");
    }
    if (_enemyContainer == null)
    {
      Debug.LogError("Enemy container is null from SpawnManager.");
    }
    if (_waveManager == null)
    {
      Debug.LogError("Couldn't find WaveManager from SpawnManager");
    }
    if (_gameManager == null)
    {
      Debug.LogError("Couldn't find GameManager from UIManager");
    }

  }

  public void StartSpawning()
  {
    StartCoroutine(SpawnWavesRoutine());
  }

  private IEnumerator SpawnWavesRoutine()
  {
    while (_waveNumber <= _numberOfEnemyWaves && _stopSpawning == false)
    {
      yield return new WaitUntil(() => CanRequestAnotherWave());

      _waveManager.SpawnWave(_waveNumber);
      _waveNumber++;
    }

    // Wait until all enemies in the final wave have been cleared before proceeding to boss.
    yield return new WaitUntil(() => CanRequestAnotherWave());
    // TODO(Improvement): Change music to boss music on spawn, then play success music on win.
    _waveManager.SpawnBossWave();
    
    // Display win for player since all waves appear to be done, but only if Player survived.
    yield return new WaitUntil(() => CanRequestAnotherWave() || _player.IsDestroyed);
    _stopSpawning = true;

    if (_player.IsDestroyed)
    {
      _gameManager.GameOver();
    }
    DestroyEnemiesAndPowerUps();
    _gameManager.GameWin();
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

  // If the wave manager is running/spawning or if there are enemies on the screen we should not initiate another wave.
  private bool CanRequestAnotherWave()
  {
    return (_waveManager.WaveNotRunning() && GetAllOnScreenEnemies().Count + GetAllOnScreenPowerUps().Count == 0);
  }

  public void AddEnemyToGame(GameObject enemy)
  {
    _enemyContainer.AddEnemy(enemy.GetComponent<Enemy>());
  }
  
  public void AddPowerUpToGame(GameObject powerUp)
  {
    _powerupObjContainer.Add(_powerupContainer.GetComponent<Powerup>());
    powerUp.transform.parent = _powerupContainer.transform;
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

  // TODO: this should be given to wave manager as a "destroywave" method that cleans up anything
  // in the current wave that was spawned. Spawn manager can still handle weapons manager stuff though.
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

  public bool ShouldStopSpawning()
  {
    return _stopSpawning;
  }

  public void OnPlayerDeath()
  {
    _stopSpawning = true;
    DestroyEnemiesAndPowerUps();
  }
}
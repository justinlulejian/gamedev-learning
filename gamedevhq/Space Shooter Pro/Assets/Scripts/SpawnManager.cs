using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
  [SerializeField]
  private GameObject _enemyContainerObj;
  private EnemyContainer _enemyContainer;
  
  [SerializeField]
  private GameObject _powerupContainer;
  private List<Powerup> _powerupObjContainer;

  [SerializeField]
  private bool _stopSpawning;
  
  private Player _player;
  private WaveManager _waveManager;
  private GameManager _gameManager;
  private UIManager _uiManager;
  
  // Enemy spawn logic.
  // Since we use Fibonacci to calculate, numbers beyond 6 get very high. 
  [SerializeField] 
  private int _numberOfEnemyWaves = 6;
  private int _waveNumber; // What wave we are currently on.

  private void Start()
  {
    _player = GameObject.Find("Player").GetComponent<Player>();
    _powerupObjContainer = new List<Powerup>();
    _enemyContainer = _enemyContainerObj.GetComponent<EnemyContainer>();
    _waveManager = GameObject.Find("Wave_Manager").GetComponent<WaveManager>();
    _gameManager = GameObject.Find("Game_Manager").GetComponent<GameManager>(); 
    _uiManager = GameObject.Find("UI_Manager").GetComponent<UIManager>(); 
    
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
      Debug.LogError("Couldn't find GameManager from SpawnManager");
    }
    if (_uiManager == null)
    {
      Debug.LogError("Couldn't find UI Manager from SpawnManager");
    }
  }

  public void StartSpawning()
  {
    StartCoroutine(SpawnWavesRoutine());
    StartCoroutine(MonitorPlayerAmmoSurvivalSpawnRoutine());
  }

  private IEnumerator SpawnWavesRoutine()
  {
    // If there are enemy waves to spawn, and we've not spawned more waves than desired, and the game hasn't ended...
    while (_numberOfEnemyWaves > 0 && _waveNumber <= _numberOfEnemyWaves && _stopSpawning == false)
    {
      yield return new WaitUntil(() => CanRequestAnotherWave());

      _waveManager.SpawnWave(_waveNumber);
      _waveNumber++;
      _uiManager.UpdateWaveNumber(_waveNumber.ToString());
    }

    // Wait until all enemies in the final wave have been cleared before proceeding to boss.
    yield return new WaitUntil(() => CanRequestAnotherWave() && !_player.IsDestroyed);
    // TODO(Improvement): Change music to boss music on spawn, then play success music on win.
    _waveManager.SpawnBossWave();
    _uiManager.UpdateWaveNumber("Boss!");
    
    // Wait for the final boss wave to resolve, either all enemies destroyed, or player is destroyed (player
    // destroy handled elsewhere).
    yield return new WaitUntil(() => CanRequestAnotherWave() || GetAllOnScreenEnemies().Count == 0);
    _stopSpawning = true;
    
    // Display win for player since all waves appear to be done, but only if Player survived.
    if (!_player.IsDestroyed)
    {
      _gameManager.GameWin();
    }
    // Wipe the scene clean of enemies and PowerUps while waiting for user to setup scene.
    DestroyEnemiesAndPowerUps();
  }

  private bool NoAmmoPowerUpsAvailable()
  {
    List<Powerup> powerUps = GetAllOnScreenPowerUps();
    foreach (Powerup powerUp in powerUps)
    {
      // Ammo powerup. Powerup.cs <c>_powerUpID </c>
      if (powerUp.GetPowerUpID() == 3)
      {
        return false;
      }
    }
    return true;
  }

  // If the player is fighting waves and they run out of ammo spawn one for them so they can fight on.
  private IEnumerator MonitorPlayerAmmoSurvivalSpawnRoutine()
  {
    while (true)
    {
      yield return new WaitUntil(() => _player.GetCurrentAmmoCount() == 0);
      yield return new WaitUntil(() => NoAmmoPowerUpsAvailable());
      // This seems redundant, but since it runs so often it very often is past the above check which
      // ends up causing a double-spawn of ammo to occur unless we double-check here.
      if (_player.GetCurrentAmmoCount() == 0)
      {
        // Make it seem like the game isn't omniscient :P.
        yield return new WaitForSeconds(1.5f);
        _waveManager.SpawnAmmoPowerUp();
      }
    }
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
    _powerupObjContainer.Add(powerUp.GetComponent<Powerup>());
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

  public void DestroyEnemiesAndPowerUps()
  {
    _enemyContainer.RemoveAllEnemies();
    
    foreach (Transform powerUp in _powerupContainer.transform)
    {
      Destroy(powerUp.gameObject);
    }
  }

  public bool ShouldStopSpawning()
  {
    return _stopSpawning;
  }

  public void OnPlayerDeath()
  {
    _stopSpawning = true;
  }
}
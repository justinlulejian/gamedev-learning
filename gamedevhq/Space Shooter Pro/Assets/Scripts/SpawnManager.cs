using System.Collections;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class SpawnManager : MonoBehaviour
{
  [SerializeField]
  private GameObject _enemyPrefab;
  [SerializeField]
  private GameObject _enemyContainer;
  [SerializeField]
  private GameObject _powerupContainer;
  [SerializeField]
  private GameObject[] _powerups;
  [SerializeField]
  private float[] _powerUpWeights;  // Weights to use when calculating spawn rate.

  [SerializeField]
  private bool _stopSpawning = false;

  public void StartSpawning()
  {
    StartCoroutine(SpawnEnemyRoutine());
    StartCoroutine(SpawnPowerupRoutine());
  }

  private IEnumerator SpawnEnemyRoutine()
  {
    yield return new WaitForSeconds(3.0f);
    
    while (_stopSpawning == false)
    {
      Vector3 randomPosToSpawn = new Vector3(Random.Range(-8f, 8f), 7, 0);
      GameObject newEnemy = Instantiate(_enemyPrefab, randomPosToSpawn, Quaternion.identity);
      newEnemy.transform.parent = _enemyContainer.transform;
      yield return new WaitForSeconds(5.0f);
    }
  }

  private IEnumerator SpawnPowerupRoutine()
  {
    yield return new WaitForSeconds(3.0f);
    
    // TODO: Bug where powerup will still spawn if we're waiting while player death
    while (_stopSpawning == false)
    {
      float randomSpawnTime = Random.Range(3, 8);
      GameObject newPowerup = Instantiate(ChooseWeightedRandomPowerUp(),
                                          new Vector3(Random.Range(-8f, 8f), 7, 0),
                                          Quaternion.identity);
      newPowerup.transform.parent = _powerupContainer.transform;
      yield return new WaitForSeconds(randomSpawnTime);
    } 
  }

  private GameObject ChooseWeightedRandomPowerUp()
  {
    // TODO(Improvement): Do not spawn powerups that are useless to the player. E.g. if they have shields, max ammo,
    // max health, etc. do not spawn them. Powerup that time out should still be collectible though.
    
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

  public void OnPlayerDeath()
  {
    _stopSpawning = true;
    foreach (Transform enemy in _enemyContainer.transform)
    {
      Destroy(enemy.gameObject);
    }
    foreach (Transform powerUp in _powerupContainer.transform)
    {
      Destroy(powerUp.gameObject);
    }
    // TODO(bug): Delete all laser and missile objects otherwise they (funnily) just float around after
    // gameover.
  }
}
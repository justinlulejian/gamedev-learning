using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
      int randomPowerUp = Random.Range(0, 3);
      GameObject newPowerup = Instantiate(_powerups[randomPowerUp],
                                          new Vector3(Random.Range(-8f, 8f), 7, 0),
                                          Quaternion.identity);
      newPowerup.transform.parent = _powerupContainer.transform;
      yield return new WaitForSeconds(randomSpawnTime);
    } 
  }

  public void OnPlayerDeath()
  {
    Debug.Log("on player death called");
    _stopSpawning = true;
    foreach (Transform enemy in _enemyContainer.transform)
    {
      Destroy(enemy.gameObject);
      Debug.Log("destroyed enemy");
    }
    foreach (Transform powerup in _powerupContainer.transform)
    {
      Destroy(powerup.gameObject);
      Debug.Log("destroyed powerup");
    }
  }
}

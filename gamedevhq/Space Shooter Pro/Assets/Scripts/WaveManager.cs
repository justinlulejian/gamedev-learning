﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class WaveManager : MonoBehaviour
{
    
    // Difficulty selection by player in UI.
    private string _difficultySelection = MainMenu.difficultySelection;
    
    // Enemies
    [SerializeField]
    private GameObject _bossPrefab;
    // [SerializeField]
    // private GameObject _enemyContainerObject;
    // private EnemyContainer _enemyContainer;
    
    // Powerups
    // [SerializeField]
    // private GameObject _powerupContainerObject;
    // private List<GameObject> _powerupObjContainer;
    
    // Spawn chances
    // TODO(Improvement): Change the ammo pickup prefab to the ammo box sprite I have in the proj.
    [SerializeField] 
    private List<GameObject> _waveSpawnObjects;
    [SerializeField]
    private List<float> _waveSpawnWeights;  // Weights to use when calculating spawn rate.
    [SerializeField] 
    private List<GameObject> _bossWaveSpawnObjects;
    [SerializeField]
    private List<float> _bossWaveSpawnWeights;
    // TODO: replace with a custom struct that can then be serialized to unity editor??
    // TODO: idea: serialize an easy, normal,and hard object/fields to editor so that designed can
    // change the variable values on the fly. Requires me to know what I want to use as inputs. Will
    // def need tooltips to ensure that I explain how they affect spawn rates in all the scnearios.
    private Dictionary<string, float> _difficultyToFactorMap = new Dictionary<string, float>()
        {{"easy", 0.5f}, {"normal", 1f}, {"insane", 2f}};

    // Game object references
    private SpawnManager _spawnManager;
    
    // Static constants
    // TODO(Improvement): These should be double for precision on calculation later, but Mathf.Pow
    // only takes floats? Probably NBD in any case.
    private static float _Phi = (Mathf.Sqrt(5f) + 1f) / 2f;
    private static float _phi = 1f / _Phi;
    
    // Spawn loop control.
    // This controls how many things we start spawning with and it'll be multiplied by 
    // _numberOfObjectsToCreateForSpawnMultipler each time we spawn something. The result is that things
    // get more hectic as the waves progress with more things being spawn per spawn loop.
    private bool _waveRunning;
    private bool _waveSpawned;
    private int _numberOfObjectsToCreateForSpawn = 1;
    private float _numberOfObjectsToCreateForSpawnMultipler = 1.5f;
    [SerializeField] [Tooltip("The amount of seconds between waves.")]
    private float _timeBetweenWaves = 3.0f;
    [SerializeField] [Tooltip("Values less than 1.0 reduce the time between waves after each wave" +
                              " finished. Values great than 1.0 increase it.")]
    private float _timeBetweenWavesMultipler = 1.0f;

    void Start()
    {
        _spawnManager = GameObject.Find("Spawn_Manager").GetComponent<SpawnManager>();
        // _enemyContainer = _enemyContainerObject.GetComponent<EnemyContainer>();
        // _powerupObjContainer = new List<GameObject>();
        // StartCoroutine(MonitorWaveSpawnStatus());

        if (_spawnManager == null)
        {
            Debug.LogError("Spawn manager was missing from Wave Manager.");
        }
        // if (_enemyContainer == null)
        // {
        //     Debug.LogError("Enemy container is null from SpawnManager.");
        // }
        
        
        
        Debug.Log($"difficulty selection by user is: {_difficultySelection}");
        if (string.IsNullOrEmpty(_difficultySelection))
        {
            Debug.Log("Difficulty selection was not specified, defaulting to easy.");
            _difficultySelection = "easy";
        }
    }

    // private IEnumerator MonitorWaveSpawnStatus()
    // {
    //     yield return new WaitUntil(() => _waveRunning && _waveSpawned);
    //     yield return new WaitUntil(() => NumberOfObjectsInWaves() == 0);
    //     _waveRunning = false;
    //     _waveSpawned = false;
    // }

    public void SpawnWave(int waveNumber)
    { 
        StartCoroutine(SpawnWaveNumberRoutine(waveNumber));
        _waveRunning = true;
    }
    
    public void SpawnBossWave()
    {
        _spawnManager.AddEnemyToGame(Instantiate(_bossPrefab));
    }
    

    // TODO(checkpoint): should I duplicate this for boss wave or share the logic amongst the two
    // but provide the differences as parameters?
    private IEnumerator SpawnWaveNumberRoutine(int waveNumber)
    {
        int numberOfSpawnsInWave = NumberOfSpawnsForWave(waveNumber);
        while (numberOfSpawnsInWave > 0 && !_spawnManager.ShouldStopSpawning())
        {
            // TODO(checkpoint): It seems I can spawn all powerups and no enemies in a wave which is odd. I think I should
            // influence the choices so that if more than one thing is spawning some % are enemies and some % are powerUps
            // so that it can't be too biased? But then does that remove the need for linear algo?
            // for (int i = 0; i < _numberOfObjectsToCreateForSpawn; i++)
            // {
            GameObject objectToSpawn = ChooseWeightedRandomSpawn();
            GameObject objectSpawned = Instantiate(objectToSpawn,
                new Vector3(UnityEngine.Random.Range(-8f, 8f), 7, 0),
                Quaternion.identity);
            if (objectSpawned.GetComponent<Powerup>())
            {
                _spawnManager.AddPowerUpToGame(objectSpawned);
            }
            else
            {
                _spawnManager.AddEnemyToGame(objectSpawned);
            }

            numberOfSpawnsInWave--;
            // }
            // Every spawn we increase the number of things we spawn each time to increase difficulty as things progress
            // _numberOfObjectsToCreateForSpawn = Mathf.RoundToInt(
            //     _numberOfObjectsToCreateForSpawn *_numberOfObjectsToCreateForSpawnMultipler);
            // TODO: This timing should move to spawn manager. Wave manager just spawn things for a wave whereas spawn
            // manager decides when to spawn a wave.
            yield return new WaitForSeconds(_timeBetweenWaves * _timeBetweenWavesMultipler);
        }
        _waveRunning = false;
    }
    
    private GameObject ChooseWeightedRandomSpawn()
    {
        // TODO(Improvement): Do not spawn powerups that are useless to the player. E.g. if they have shields, max ammo,
        // max health, etc. do not spawn them. Powerups that time out should still be collectible though.
    
        // Linear scan algo from: https://blog.bruce-hill.com/a-faster-weighted-random-choice
        float remainingDistance = UnityEngine.Random.value * _waveSpawnWeights.Sum();
        for (int i = 0; i < _waveSpawnWeights.Count; i++)
        {
            remainingDistance -= _waveSpawnWeights[i];
            if (remainingDistance < 0)
            {
                return _waveSpawnObjects[i];
            }
        }
        Debug.LogError("Weighted random choice of powerups failed to find a value.");
        // Fallback to non-weighted random choice if scan fails.
        return _waveSpawnObjects[UnityEngine.Random.Range(0, _waveSpawnObjects.Count)];
    }

    public List<GameObject> EnemiesRemainingInWave()
    {
        List<GameObject> enemiesRemainingInWave = new List<GameObject>();
        return enemiesRemainingInWave;
    }
    
    public List<GameObject> PowerupsRemainingInWave()
    {
        List<GameObject> powerUpsRemainingInWave = new List<GameObject>();
        return powerUpsRemainingInWave;
    }
    
    // public int NumberOfObjectsInWaves()
    // {
    //     int numberOfPowerUps = _powerupObjContainer.Count
    //     return _powerupObjContainer.Count + _enemyContainer.EnemyCount();
    // }
    //
    // public bool WaveInProgress()
    // {
    //     // return _waveRunning && _waveSpawned;
    //     return NumberOfObjectsInWaves() > 0;
    // }
    //
    // public bool NoWavesRunning()
    // {
    //     return NumberOfObjectsInWaves() == 0;
    //     // return _waveRunning == false;
    // }
    //

    public bool WaveNotRunning()
    {
        return _waveRunning == false;
    }
    private int NumberOfSpawnsForWave(int waveNumber)
    {
        // Skip the first fib value being 0
        waveNumber += 2;
        // Formula: https://math.hmc.edu/funfacts/fibonacci-number-formula/
        // Graphical representation of difficulty levels to number of enemies
        // https://www.desmos.com/calculator/legniuqsnt
        return Mathf.CeilToInt(
            ((Mathf.Abs(
                 (Mathf.Pow(_Phi, waveNumber) - Mathf.Pow(_phi, waveNumber))
                 / Mathf.Sqrt(5)))
             * _difficultyToFactorMap[_difficultySelection]));
    }
}

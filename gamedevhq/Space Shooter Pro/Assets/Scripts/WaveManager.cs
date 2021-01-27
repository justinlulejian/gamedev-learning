using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class WaveManager : MonoBehaviour
{
    
    // Difficulty selection by player in UI.
    private string _difficultySelection = MainMenu.difficultySelection;
    
    // Enemies
    [SerializeField]
    private List<GameObject> _enemyTypes;
    [SerializeField]
    private GameObject _bossPrefab;
    [SerializeField]
    private GameObject _enemyContainerObj;
    private EnemyContainer _enemyContainer;
    
    // Powerups
    [SerializeField]
    private GameObject _powerupContainer;
    private List<Powerup> _powerupObjContainer;
    [SerializeField]
    private List<GameObject> _powerups;
    
    // Spawn chances
    [SerializeField] 
    private List<GameObject> _spawnableObjects;
    [SerializeField]
    private List<float> _spawnWeights;  // Weights to use when calculating spawn rate.
    // TODO: replace with a custom struct that can then be serialized to unity editor??
    // TODO: idea: serialize an easy, normal,and hard object/fields to editor so that designed can
    // change the variable values on the fly. Requires me to know what I want to use as inputs. Will
    // def need tooltips to ensure that I explain how they affect spawning.
    private Dictionary<string, float> _difficultyToFactorMap = new Dictionary<string, float>()
        {{"easy", 0.5f}, {"normal", 1f}, {"insane", 2f}};

    // Static constants
    // TODO(Improvement): These should be double for precision on calculation later, but Mathf.Pow
    // only takes floats? Probably NBD in any case.
    private static float _Phi = (Mathf.Sqrt(5f) + 1f) / 2f;
    private static float _phi = 1f / _Phi;
    
    
    // TODO(Improvement): Change the ammo pickup prefab to the ammo box sprite I have in the proj.
    
    
    void Start()
    {
        Debug.Log($"difficulty selection by user is: {_difficultySelection}");
        if (string.IsNullOrEmpty(_difficultySelection))
        {
            Debug.Log("Difficulty selection was not specified, defaulting to easy.");
            _difficultySelection = "easy";
        }
    }

    public void SpawnWave(int waveNumber)
    {
        StartCoroutine(SpawnWaveNumberRoutine(waveNumber));
    }
    
    public void SpawnBossWave()
    {
        StartCoroutine(SpawnBossWaveRoutine());
    }
    
    private IEnumerator SpawnBossWaveRoutine()
    {
        yield return null;
        // _enemyContainer.AddEnemy(Instantiate(_bossPrefab));
    }

    private IEnumerator SpawnWaveNumberRoutine(int waveNumber)
    {
        int numberOfEnemiesToSpawnForWave = CalculateNumberOfEnemiesInWave(waveNumber);
        while (numberOfEnemiesToSpawnForWave > 0)
        {
            _enemyContainer.AddEnemy(Instantiate(GetRandomEnemyType()));
            numberOfEnemiesToSpawnForWave--;
        }
        
        
        yield return null;
    }
    
    private GameObject GetRandomEnemyType()
    {
        if (_enemyTypes.Count == 0)
        {
            Debug.LogError("_enemyTypes is empty in spawn manager.");
            return null;
        }

        int randomEnemyTypeIndex = UnityEngine.Random.Range(0, _enemyTypes.Count);
        return _enemyTypes[randomEnemyTypeIndex];
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
    
    public List<GameObject> AllSpawnsInWave()
    {
        List<GameObject> allSpawnsInWave = new List<GameObject>();
        return allSpawnsInWave;
    }
    
    public bool WaveInProgress()
    {
        return AllSpawnsInWave().Count > 0;
    }
    
    public bool NoWavesRunning()
    {
        return AllSpawnsInWave().Count == 0;
    }
    
    private int CalculateNumberOfEnemiesInWave(int waveNumber)
    {
        // Formula: https://math.hmc.edu/funfacts/fibonacci-number-formula/
        // Graphical representation of difficulty levels to number of enemies
        // https://www.desmos.com/calculator/legniuqsnt
        return Mathf.CeilToInt(
            ((Mathf.Abs(
                 (Mathf.Pow(_Phi, waveNumber) - Mathf.Pow(_phi, waveNumber))
                 / Mathf.Sqrt(5)))
             * _difficultyToFactorMap[_difficultySelection]));
    }

    void Update()
    {
        
    }
}

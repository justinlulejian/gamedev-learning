using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;


public class WaveManager : MonoBehaviour
{
    // Prefabs
    [SerializeField] 
    private GameObject _ammoPowerupPrefab;
    
    // Difficulty selection by player in UI.
    private string _difficultySelection = MainMenu.difficultySelection;
    
    // Spawn percentages of enemies vs powerUps
    [SerializeField] [Tooltip("Percentage of things that will spawn per regular enemy wave will be this percentage " +
                              "enemies. The remainder is reserved for powerups.")]
    private float percentOfEnemiesPerWave = 70f;
    
    // Enemies
    [SerializeField]
    private GameObject _bossPrefab;
    [SerializeField] 
    private List<GameObject> _enemyTypesToSpawn;
    [SerializeField]
    private List<float> _enemyTypesSpawnWeights;
    
    // Powerups
    /// Triple shot, speed, missile, and shotgun shot. From Powerup.cs <c> _powerUpID </c>.
    private HashSet<int> undesiredPowerUpIds = new HashSet<int>() { 0, 1, 5, 7};
    // TODO(Improvement): Change the ammo pickup prefab to the ammo box sprite I have in the proj.
    [SerializeField] 
    private List<GameObject> _powerUpTypesToSpawn;
    [SerializeField]
    private List<float> _powerUpTypesSpawnWeights;
    
    // Boss wave
    // Boss wave has a separate set of objects that spawn customized to it (being the last wave).
    [SerializeField] 
    private List<GameObject> _bossWaveSpawnObjects;
    [SerializeField]
    private List<float> _bossWaveSpawnWeights;
    private Dictionary<string, float> _difficultyToFactorMap = new Dictionary<string, float>()
        {{"easy", 0.3f}, {"normal", .5f}, {"insane", 1f}};

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
    [SerializeField] [Tooltip("Values less than 1.0 reduce the time between waves after each wave " +
                              "finished. Values great than 1.0 increase it.")]
    private float _timeBetweenWavesMultipler = 1.0f;
    [SerializeField] 
    private float _timeBetweenObjectSpawnsDuringWave = .3f;

    void Start()
    {
        _spawnManager = GameObject.Find("Spawn_Manager").GetComponent<SpawnManager>();

        if (_spawnManager == null)
        {
            Debug.LogError("Spawn manager was missing from Wave Manager.");
        }

        if (percentOfEnemiesPerWave < 0f || percentOfEnemiesPerWave > 100f)
        {
            Debug.LogError("Percentage of enemies is greater than 100% or less than 0%! Wave spawning will " +
                           "probably not work as intended.");
        }
        
        
        
        if (string.IsNullOrEmpty(_difficultySelection))
        {
            Debug.LogError("Difficulty selection was not specified, defaulting to easy.");
            _difficultySelection = "easy";
        }
    }

    // A special function used to avoid the case where there are enemies left, the player has no ammo, and doesn't have
    // enough health to ram them and defeat them.
    public void SpawnAmmoPowerUp()
    {
        StartCoroutine(SpawnObjectsForWave(new List<GameObject>() {_ammoPowerupPrefab}));
    }

    public void SpawnWave(int waveNumber)
    { 
       SpawnWaveNumber(waveNumber);
    }
    
    // Filter out some duplicates spawns that don't make sense. E.g. double spawns of some weapon Power Ups.
    private bool ObjectUndesired(GameObject obj)
    {
        Powerup objectPowerup = obj.GetComponent<Powerup>();
        if (objectPowerup != null && undesiredPowerUpIds.Contains(objectPowerup.GetPowerUpID()))
        {
            List<Powerup> powerUps = _spawnManager.GetAllOnScreenPowerUps();
            foreach (Powerup powerup in powerUps)
            {
                if (objectPowerup.GetPowerUpID() == powerup.GetPowerUpID())
                {
                    return true;
                }
            }
        }
        
        return false;
    }
    
    // TODO(Improvement): replace with something like https://github.com/BlueRaja/Weighted-Item-Randomizer-for-C-Sharp.
    // And provide accurate guidance on how exactly to set the weights to get what is desired.
    private GameObject ChooseWeightedRandomSpawn(
        IReadOnlyList<GameObject> objectTypesOptions, IReadOnlyList<float> objectTypeSpawnWeights) 
    {
        // Linear scan algo from: https://blog.bruce-hill.com/a-faster-weighted-random-choice
        float remainingDistance = UnityEngine.Random.value * objectTypeSpawnWeights.Sum();
        for (int i = 0; i < objectTypeSpawnWeights.Count; i++)
        {
            remainingDistance -= objectTypeSpawnWeights[i];
            if (remainingDistance < 0)
            {
                return objectTypesOptions[i];
            }
        }
        Debug.LogError(
            $"Weighted random choice failed to find a value for objects: {objectTypesOptions.ToString()}.");
        // Fallback to non-weighted random choice if scan fails.
        return objectTypesOptions[UnityEngine.Random.Range(0, objectTypesOptions.Count)];
    }

    private GameObject ChooseWeightedRandomEnemy()
    {
        return ChooseWeightedRandomSpawn(_enemyTypesToSpawn, _enemyTypesSpawnWeights);
    }
    
    private GameObject ChooseWeightedRandomPowerup()
    {
        return ChooseWeightedRandomSpawn(_powerUpTypesToSpawn, _powerUpTypesSpawnWeights);
    }

    private IEnumerator SpawnObjectsForWave(List<GameObject> objectsToSpawn)
    {
        foreach (GameObject objectToSpawn in objectsToSpawn)
        {
            if (!_spawnManager.ShouldStopSpawning())
            {
                GameObject objectSpawned = Instantiate(objectToSpawn,
                    new Vector3(Random.Range(-8f, 8f), 7, 0),
                    Quaternion.identity);
                if (objectSpawned.GetComponent<Powerup>())
                {
                    _spawnManager.AddPowerUpToGame(objectSpawned);
                }
                else
                {
                    _spawnManager.AddEnemyToGame(objectSpawned);
                }
                yield return new WaitForSeconds(_timeBetweenObjectSpawnsDuringWave);
            }
        }
        // TODO(Improvement): This timing should probably move to spawn manager. Wave manager just spawn things for a
        // wave whereas spawn manager should decide when to spawn a wave?
        yield return new WaitForSeconds(_timeBetweenWaves * _timeBetweenWavesMultipler);
        _waveRunning = false;
    }
    

    private void SpawnWaveNumber(int waveNumber)
    {
        _waveRunning = true;
        int numberOfSpawnsInWave = NumberOfSpawnsForWave(waveNumber);
        List<GameObject> objectsToSpawn = new List<GameObject>();

        int EnemiesToSpawn = Mathf.RoundToInt(numberOfSpawnsInWave * (percentOfEnemiesPerWave * .01f));
        int PowerUpsToSpawn = numberOfSpawnsInWave - EnemiesToSpawn;

        for (int i = 0; i < EnemiesToSpawn; i++)
        {
            GameObject enemyToSpawn = ChooseWeightedRandomEnemy();
            while (ObjectUndesired(enemyToSpawn))
            {
                enemyToSpawn = ChooseWeightedRandomEnemy();
            }
            objectsToSpawn.Add(enemyToSpawn);
        }
        
        for (int i = 0; i < PowerUpsToSpawn; i++)
        {
            GameObject powerUpToSpawn = ChooseWeightedRandomPowerup();
            while (ObjectUndesired(powerUpToSpawn))
            {
                powerUpToSpawn = ChooseWeightedRandomPowerup();
            }
            objectsToSpawn.Add(powerUpToSpawn);
        }
        StartCoroutine(SpawnObjectsForWave(objectsToSpawn));
    }
        
    public void SpawnBossWave()
    {
        StartCoroutine(SpawnBossWaveRoutine());
    }
    
    private GameObject ChooseWeightedRandomBossPowerup()
    {
        return ChooseWeightedRandomSpawn(_bossWaveSpawnObjects, _bossWaveSpawnWeights);
    }

    private IEnumerator SpawnBossWaveRoutine()
    {
        _waveRunning = true;
        _spawnManager.AddEnemyToGame(Instantiate(_bossPrefab));
        while (!_spawnManager.ShouldStopSpawning())
        {
            GameObject powerUpToSpawn = ChooseWeightedRandomBossPowerup();
            while (ObjectUndesired(powerUpToSpawn))
            {
                powerUpToSpawn = ChooseWeightedRandomBossPowerup();
            }
            StartCoroutine(SpawnObjectsForWave(new List<GameObject>(){powerUpToSpawn}));
            yield return new WaitForSeconds(_timeBetweenWaves * _timeBetweenWavesMultipler);
        }
        _waveRunning = false;
    }
    
    public bool WaveNotRunning()
    {
        return _waveRunning == false;
    }
    private int NumberOfSpawnsForWave(int waveNumber)
    {
        // Skip the second fib value being 1 so enemies will only increase.
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

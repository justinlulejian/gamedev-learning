using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Starting wave routine.");
        StartCoroutine(StartWaveRoutine());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private IEnumerator StartWaveRoutine()
    {
        
        WaveManager waveManager = WaveManager.Instance;
        foreach (Wave wave in WaveManager.Instance.GetWaves())
        {
            // read from current wave data
            Debug.Log($"Spawning wave: {wave.waveName}");

            Debug.Log($"Waiting seconds before wave start: {wave._waitTimeBeforeWaveStart.ToString()}");
            yield return new WaitForSeconds(wave._waitTimeBeforeWaveStart);

            GameObject currentWaveObj = new GameObject(wave.waveName);
            
            // inst that wave
            foreach (var waveObject in wave._waveObjectsToSpawn)
            {
                Instantiate(waveObject, currentWaveObj.transform);
            }

            // when we are finished with that wave
            // wait X seconds
            Debug.Log($"Waiting seconds after wave end: {wave._waitTimeAfterWaveEnd.ToString()}");
            yield return new WaitForSeconds(wave._waitTimeAfterWaveEnd);

            // clean up current objects;
           Destroy(currentWaveObj);
        }
    }
}

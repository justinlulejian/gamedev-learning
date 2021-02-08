using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Wave.asset", menuName = "Waves/Wave")]
public class Wave : ScriptableObject
{
    public string waveName;
    public float _waitTimeBeforeWaveStart;
    public float _waitTimeAfterWaveEnd;
    public List<GameObject> _waveObjectsToSpawn;
    
    public void PrintWaveName()
    {
        
    }
}
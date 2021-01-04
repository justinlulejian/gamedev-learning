using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossEnemy : Enemy
{
    [SerializeField]
    private GameObject circleShotPrefab;
    [SerializeField]
    private GameObject laserBeamPrefab;
    
    // TODO:
    // 1. Get boss to spawn, then move slowly to middle of screen and sit there.
    // 2. When boss is hit by player weapons, there's some kind of damage feedback, a flash or explosion.
    // 3. Impl circleshot attack and laser beam attack.
    // If time: health bar that depletes over time, or just increasing number of damage sprites appended to it (enabled)
    // For fun: switch music to boss/enemy music from star ocean.
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

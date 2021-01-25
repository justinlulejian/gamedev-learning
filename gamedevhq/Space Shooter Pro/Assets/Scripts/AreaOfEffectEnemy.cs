using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaOfEffectEnemy : Enemy
{
    [SerializeField] private GameObject _aoeShotPrefab;
    [SerializeField] private float _aoeFireRate = 3f;
    
    // TODO:
    // 1) get shot and aoe explosion to register as hitting player and that AOE damages player, for some reason their
    // colliders are set as triggers but the player never enters the shot's triggerenter or the explosion enters player's
    // trigger enter. Both are is trigger = true, and in foreground 0.
    // 2) Give enemy an explosion prefab
    // 3) Have enemy do zig zag movement -- ease function if you want to be fancy, otherwise just pre-determine a min/max
    // x value and MoveTowards back and forth once it reaches each side. Make speed a factor in that movement and ensure
    // it still moves downwards when doing that.
    // 3) Either disable or enable shield prefab so it looks right.
    
    void Start()
    {
        base.Start();
        _aggroTowardsPlayer = false;
        _fireRate = _aoeFireRate;
        // This shouldn't be faster than the speed of the AOE shot, otherwise it'll catch up to it.
        // TODO: revert to 1 once done testing.
        _speed = .25f;
    }
    
    protected override EnemyMovementType ChooseMovementType()
    {
        return EnemyMovementType.StraightDown;
    }

    protected override void PeriodicFireLasers()
    {
        if (!IsDefeated() && Time.time > _canFire)
        {
            _canFire = Time.time + _fireRate;
            Instantiate(_aoeShotPrefab, transform.position + new Vector3(0, -1.25f, 0), Quaternion.identity);
        }
    }
    
    
}

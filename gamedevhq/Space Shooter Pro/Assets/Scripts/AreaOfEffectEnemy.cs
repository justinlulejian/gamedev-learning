using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaOfEffectEnemy : Enemy
{
    [SerializeField] private GameObject _aoeShotPrefab;
    [SerializeField] private float _aoeFireRate = 3f;
    [SerializeField] private GameObject _deathPrefab;

    void Start()
    {
        base.Start();
        _aggroTowardsPlayer = false;
        _fireRate = _aoeFireRate;
        
        // This shouldn't be faster than the speed of the AOE shot, otherwise it'll catch up to it.
        _speed = 1f;
    }
    
    protected override EnemyMovementType ChooseMovementType()
    {
        return EnemyMovementType.ZigZag;
    }

    protected override void PeriodicFireLasers()
    {
        if (!IsDefeated() && Time.time > _canFire)
        {
            _canFire = Time.time + _fireRate;
            Instantiate(_aoeShotPrefab, transform.position + new Vector3(0, -1.25f, 0), Quaternion.identity);
        }
    }

    public override void DestroyEnemy()
    { 
        base.WasDefeated();
        Instantiate(_deathPrefab, transform.position, Quaternion.identity);
        if (_audioSource.enabled)
        {
            _audioSource.Play();
        }
        Destroy(GetComponent<CircleCollider2D>());
        this.gameObject.SetActive(false);
        base.RemoveEnemyFromGame(2.8f);
    }
}

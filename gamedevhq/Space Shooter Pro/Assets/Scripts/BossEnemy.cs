using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossEnemy : Enemy
{
    [SerializeField]
    private GameObject _circleAttackPrefab;
    [SerializeField]
    private GameObject _laserBeamPrefab;
    [SerializeField]
    private float _specialAttackCooldown = 5f;
    [SerializeField]
    private GameObject _damagePrefab;
    [SerializeField]
    private GameObject _deathPrefab;
    private List<GameObject> _specialWeaponsActive;
    
    [SerializeField] 
    private int _bossLives = 10;

    private Collider2D _collider;
    
    protected override void Start()
    {
        base.Start();
        // Spawn off screen.
        _startPosition = transform.position = new Vector3(0, 8, 0);
        // Middle-ish of screen.
       _endPosition = new Vector3(0, 2.5f, 0);
       _collider = this.GetComponent<Collider2D>();
       // Causes the boss to move slower to _endPosition.
       _movementLerpTime = 7f;
       _specialWeaponsActive = new List<GameObject>();
       
       if (_collider == null) {
           Debug.LogError("Collider for boss is null.");
       }

       StartCoroutine(routine: AttacksRoutine());
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position != _endPosition)
        {
            MoveToEndPosition();
        }
        OscillateMovement();
        if (_specialWeaponsActive.Count == 0)
        {
            PeriodicFireLasers();
        }
    }

    private void MoveToEndPosition()
    {
        // Inspiration from https://chicounity3d.wordpress.com/2014/05/23/how-to-lerp-like-a-pro/
        _movementCurrentLerpTime += Time.deltaTime;
        if (_movementCurrentLerpTime > _movementLerpTime)
        {
            return;
        }
        
        float interpValue = _movementCurrentLerpTime / _movementLerpTime;
        interpValue = Mathf.Sin(interpValue * Mathf.PI * 0.5f);  // "sinerp"
        transform.position = Vector3.Lerp(_startPosition, _endPosition, interpValue);
        
    }

    private void OscillateMovement()
    {
        // TODO(Improvement): Make the boss move in small circles to simulate flight.
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if ((other.gameObject.CompareTag("Laser") && !other.GetComponent<Laser>().IsEnemyLaser)
            || other.CompareTag("Missile") || other.CompareTag("ShotgunShot"))
        {
            // Since the Laser is a trigger collider the best we can do is approximate where it hits with
            // ClosestPoint and show damage there. This could be improved by changing trigger colliders to
            // non-trigger physics colliders.
            Instantiate(
                _damagePrefab, _collider.ClosestPoint(other.transform.position), Quaternion.identity);
        }
        
        base.OnTriggerEntered2D(other);
    }

    protected override void PlayerDamageEnemy()
    {
        _bossLives--;
        if (_bossLives < 1)
        {
            PlayerEnemyKill();
        }
    }

    protected override void PlayerEnemyKill()
    {
        DestroyEnemy();
        if (_player)
        {
            _player.AddToScore(100);
        }
    }

    protected override void DestroyEnemy()
    {
        Instantiate(_deathPrefab, transform.position, Quaternion.identity);
        if (_audioSource.enabled)
        {
            _audioSource.Play();
        }
        Destroy(GetComponent<PolygonCollider2D>());
        base.WasDefeated();
        this.gameObject.SetActive(false);
        foreach (var weapon in _specialWeaponsActive)
        {
            if (weapon != null)
            {
                Destroy(weapon);
            }
        }
        base.RemoveEnemyFromGame(2.8f);
    }

    private IEnumerator AttacksRoutine()
    {
        StartCoroutine(CleanUpExpiredWeapons());
        // Wait until the boss settles into end position.
        yield return new WaitUntil(() => transform.position == _endPosition);
        while (true)
        {
            yield return new WaitUntil(() => _specialWeaponsActive.Count == 0);
            GameObject laserAttack = Instantiate(_laserBeamPrefab, transform.position + new Vector3(0, 1, 0),
                Quaternion.identity);
            _specialWeaponsActive.Add(laserAttack);
            yield return new WaitForSeconds(_specialAttackCooldown);
            GameObject circleAttack = Instantiate(_circleAttackPrefab, transform.position, Quaternion.identity);
            circleAttack.transform.parent = transform;
            _specialWeaponsActive.Add(circleAttack);
        }
    }

    // Clean up special weapons that have been destroyed in the last second.
    private IEnumerator CleanUpExpiredWeapons()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            for (int i = 0; i < _specialWeaponsActive.Count; i++)
            {
                if (_specialWeaponsActive[i] == null)
                {
                    _specialWeaponsActive.RemoveAt(i);
                }
            }
        }
    }
}

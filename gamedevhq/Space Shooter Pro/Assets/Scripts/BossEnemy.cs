using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class BossEnemy : Enemy
{
    [SerializeField]
    private GameObject _circleShotPrefab;
    [SerializeField]
    private GameObject _laserBeamPrefab;
    [SerializeField]
    private GameObject _damagePrefab;
    [SerializeField]
    private GameObject _deathPrefab;
    
    [SerializeField] 
    private int _bossLives = 10;

    private float _movementLerpTime = 7f;
    private float _movementCurrentLerpTime;
    private SpriteRenderer _spriteRenderer;
    
    // TODO:
    // 1. Get boss to spawn, then move slowly to middle of screen and sit there. Bonus: make is oscillate in a small circle.
    // 2. When boss is hit by player weapons, there's some kind of damage feedback, a flash or explosion.
    // 3. Impl circleshot attack and laser beam attack.
    // If time: health bar that depletes over time, or just increasing number of damage sprites appended to it (enabled)
    // For fun: switch music to boss/enemy music from star ocean.
    
    // Start is called before the first frame update
    void Start()
    {
        base.Start();
        // Spawn off screen.
        _startPosition = transform.position = new Vector3(0, 8, 0);
        // Middle-ish of screen.
       _endPosition = new Vector3(0, 1, 0);
       _spriteRenderer = this.GetComponent<SpriteRenderer>();
       
       if (_spriteRenderer == null) {
           Debug.LogError("Sprite renderer for boss is null.");
       }
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position != _endPosition)
        {
            MoveToEndPosition();
        }

        OscillateMovement();

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
        
    }
    
    // TODO: So apparently I just needed OnTriggerEnter2D...but if I don't define it here then it'll get
    // called on the Enemy base class. So I need to define it here, do my logic, then hand if off to Enemy
    // logic, but then make sure it calls my custom (overriden) damage logic.
    private void OnTriggerEnter2D(Collider2D other)
    {
      OnTriggerEntered2D(other);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Laser"))
        {
            Vector2 collisionPoint = other.GetContact(0).point;
            Instantiate(_damagePrefab, collisionPoint, Quaternion.identity);
            PlayerDamageEnemy();
        }

        base.OnTriggerEntered2D(other);
    }
    
    private void OnCollisionStay2D(Collision2D other)
    {
        base.OnTriggerStayed2D(other);
    }
    
    private void OnCollisionExit2D(Collision2D other)
    {
        base.OnTriggerExited2D();
    }

    protected override void PlayerDamageEnemy()
    {
        _bossLives--;
        if (_bossLives < 1)
        {
            Instantiate(_damagePrefab, transform.position, Quaternion.identity);
            if (_audioSource.enabled)
            {
                _audioSource.Play();
            }
            Destroy(GetComponent<PolygonCollider2D>());
            Destroy(this.gameObject, 2.8f);
        }
    }
}

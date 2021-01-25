using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ScriptExtensionMethods;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

public class Enemy : MonoBehaviour
{
  
  [SerializeField]
  private GameObject _laserPrefab;
  private float _canFire = -1f;
  private float _fireRate = 3.0f;
  private bool _defeated = false;
  // How many times the enemy has respawned at the top of the screen after reaching the bottom.
  private int _respawnCount = 0;

  [SerializeField] 
  protected float _speed = 4f;

  protected enum EnemyMovementType
  {
    StraightDown,
    SweepIn,
  }
  protected EnemyMovementType _enemyMovementType;
  private protected Vector3 _startPosition;
  // Not all movement types use the end position at the moment, though they could. Currently only SweepIn does.
  private protected Vector3 _endPosition;
  private protected float _movementLerpTime = 1f;
  private protected float _movementCurrentLerpTime;

  [SerializeField] 
  protected bool _aggroTowardsPlayer;
  [SerializeField] 
  private float _aggroRammingDistanceToPlayer = 5f;  // How close player must be for aggro enemy to attempt ramming.
  private Vector3 _aggroRamVelocity = Vector3.zero;
  private bool _aggroChasingPlayer;
  
  [SerializeField]
  private protected AudioSource _audioSource;

  private protected Player _player;

  private SpawnManager _spawnManager;

  private Animator _animator;

  // If we have more game objects with shields we should make a parent class/category with that functionality.
  [SerializeField] 
  private GameObject _shieldsPrefab;
  // Keep track of how long enemy has been been in collision with player to re-damage if we stay collided for too long.
  private float _playerEnemyCollideTimeTotal = 0f;
  // Staying collided longer than this time with enemy will redamage player.
  private const float _playerCollideTimeRedamage = .3f;

  private SpriteRenderer _spriteRenderer;

  public bool IsDefeated()
  {
    return _defeated;
  }

  public int GetRespawnCount()
  {
    return _respawnCount;
  }
  
  protected virtual void Start()
  {
    _player = GameObject.Find("Player").GetComponent<Player>();
    _spawnManager = GameObject.Find("Spawn_Manager").GetComponent<SpawnManager>();
    _animator = gameObject.GetComponent<Animator>();
    _audioSource = GetComponent<AudioSource>();
    _animator = gameObject.GetComponent<Animator>();
    _shieldsPrefab.SetActive(Random.value > 0.5);  // 0.0-0.5 == false, 0.5-1.0 == true.
    _enemyMovementType = ChooseMovementType();
    _startPosition = SetStartPositionBasedOnMovementType(_enemyMovementType);
    // _aggroTowardsPlayer = Random.value > 0.5;  // 0.0-0.5 == false, 0.5-1.0 == true.
    _spriteRenderer = this.GetComponent<SpriteRenderer>();
    
    if (_enemyMovementType == EnemyMovementType.SweepIn)
    {
      _endPosition = CalculateNewSweepEndPosition(_startPosition);
    } 
    transform.position = _startPosition;
    transform.rotation = Quaternion.identity;
    StartCoroutine(FireOnPowerUpsAndPlayerBehindRoutine());
    StartCoroutine(AggroEnemySpriteColorRoutine());

    // TODO(?): Is !obj same as == null?
    if (!_player)
    {
      Debug.LogError("Player is null from Enemy.");
    }
    if (_spawnManager == null)
    {
      Debug.LogError("Spawn manager is null from Enemy.");
    }
    _animator = gameObject.GetComponent<Animator>();
    if (!_animator)
    {
      Debug.LogError("Animator is null for Enemy.");
    }
    if (_audioSource == null)
    {
      Debug.LogError("_laserAudioSource was null when creating enemy");
    }
    if (_laserPrefab == null)
    {
      Debug.LogError("_laserPrefab was null when creating enemy");
    }
    if (_shieldsPrefab == null)
    {
      Debug.LogError("_shieldsPrefab was null when creating enemy");
    }
    if (_spriteRenderer == null)
    {
      Debug.LogError("Sprite renderer was null when creating enemy");
    }
  }
  
  private Vector3 CalculateNewSweepEndPosition(Vector3 startPosition)
  {
    // Subtracting 2 from startPosition.y so enemies don't move entirely off-screen.
    return new Vector3(startPosition.x * -1f, Random.Range(startPosition.y - 2f, -6.5f), startPosition.z);
  }

  protected virtual void Update()
  {
    // When destroying enemies _speed is set to 0 which causes movement to be weird for some movement types. 
    if (IsDefeated())
    {
      return;
    }
    CalculateMovement();
    PeriodicFireLasers();
  }

  protected virtual EnemyMovementType ChooseMovementType()
  {
    Array movementValues = Enum.GetValues(typeof(EnemyMovementType));
    System.Random random = new System.Random();
    return (EnemyMovementType) movementValues.GetValue(random.Next(movementValues.Length));
  }

  private Vector3 SetStartPositionBasedOnMovementType(EnemyMovementType enemyMovementType)
  {
    Vector3 startPosition = new Vector3();
    
    switch (enemyMovementType)
    {
      case EnemyMovementType.StraightDown:
        // TODO(Improvement): adjust random pos created to ensure enemy sprites don't overlap on one
        // another.
        // Spawn in a random position along top of screen.
        startPosition = new Vector3(Random.Range(-8f, 8f), 7, 0);
        // startPosition = transform.position;
        break;
      case EnemyMovementType.SweepIn:
        float sideOfMap = Random.value > 0.5f ? -11f : 11f;  // -11 is left, 11 is right.
        startPosition = new Vector3(sideOfMap, 8, 0);
        break;
    }

    return startPosition;
  }

  private bool WithinRammingDistanceToPlayer()
  {
   return Vector3.Distance(
      this.transform.position, _player.transform.position) <= _aggroRammingDistanceToPlayer;
  }

  // Blink the enemy red when chasing aggro to show status.
  private IEnumerator AggroEnemySpriteColorRoutine()
  {
    while (true)
    {
      while (_aggroChasingPlayer && !IsDefeated()) 
      {
        _spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(.1f);
        _spriteRenderer.color = Color.white;
        yield return new WaitForSeconds(.1f);
      }
      // Either we haven't been aggro yet, enemy is out of aggro range, or it's been destroyed so we reset color to
      // ensure they don't stay red.
      _spriteRenderer.color = Color.white;
      yield return new WaitUntil(() => _aggroChasingPlayer == true);
    }
}

  private void AttemptRamPlayer()
  {
    // Rotate towards player. -90 angle since enemy forward orientation is upwards on the game screen.
    MovementExtensions.RotateTowards(this.transform, _player.transform, 10f, -90f);
    
    // "Ram" move towards player to attempt collision.
    transform.position =
      Vector3.SmoothDamp(
        transform.position, _player.transform.position, ref _aggroRamVelocity, .75f);
  }

  protected virtual void CalculateMovement()
  {
    if (_aggroTowardsPlayer && WithinRammingDistanceToPlayer())
    {
      _aggroChasingPlayer = true;
      AttemptRamPlayer();
      return;
    // Being here means we were just chasing player, but are no longer since we're not within ramming distance anymore.  
    } else if (_aggroTowardsPlayer && _aggroChasingPlayer)
    {
      // Reset chasing status and orientation to default if player goes out of range of aggro ramming.
      _aggroChasingPlayer = false;
      _aggroRamVelocity = Vector3.zero;
      // Reset values used for movement so they can continue to move more naturally as they were before chasing.
      _movementCurrentLerpTime = 0f;
      // TODO(Improvement): I can't think of a good way to continue a EnemyMovementType.SweepIn movement after stopping
      // chasing so for now let's just have them continue down. Would be better to recalc and SweepIn to a new natural
      // point to be consistent.
      _enemyMovementType = EnemyMovementType.StraightDown;
    }

    // If the enemy has been rotated by chasing, return them back to original rotation.
    if (!this.transform.rotation.Equals(Quaternion.identity))
    {
      MovementExtensions.RotateTowardsQuaternion(this.transform, Quaternion.identity, 10f);
    }
    
    switch (_enemyMovementType)
    { 
      case EnemyMovementType.StraightDown:
        StraightDownMovement(); 
        break;
      case EnemyMovementType.SweepIn:
        SweepInMovement();
        break;
    }
  }

  protected virtual void StraightDownMovement()
  {
    Vector3 moveShip = Vector3.down * (_speed * Time.deltaTime);
    transform.Translate(moveShip);

    // If the enemy goes out of bounds downward to left and right (for AvoidEnemy).
    if (transform.position.y <= -5f || transform.position.x >= 11f || transform.position.x <= -11f)
    {
      // Respawn back at the top of the screen.
      float randomX = Random.Range(-8f, 8f);
      transform.position = new Vector3(randomX, 7, 0);
      transform.rotation = Quaternion.identity;
      _respawnCount++;
    }
  }

  private void SweepInMovement()
  {
    // Inspiration from https://chicounity3d.wordpress.com/2014/05/23/how-to-lerp-like-a-pro/.
    // TODO(Improvement): Without dividing by some value, Time.deltaTime increases too quickly and causes the enemy to
    // move too fast in comparison to other movement types. 15f is an arbitrary values that slowed it down, but more
    // thought could be used here on how to mathematically make the speeds similar across movement types.
    // Perhaps increase _movementlerpTime inversely with speed?
    _movementCurrentLerpTime += ((Time.deltaTime / 15f) * _speed);  
    // We've reached the end position.
    if (_movementCurrentLerpTime > _movementLerpTime) {
      // Flips start position to opposite side (left/right) of map.
      _startPosition = transform.position = new Vector3(_startPosition.x * -1f, _startPosition.y, _startPosition.z);
      _endPosition = CalculateNewSweepEndPosition(_startPosition);
      _movementCurrentLerpTime = 0f;
      _respawnCount++;
      return;
    }
  
    float interpValue = _movementCurrentLerpTime / _movementLerpTime;
    interpValue = Mathf.Sin(interpValue * Mathf.PI * 0.5f);  // "sinerp"
    transform.position = Vector3.Lerp(_startPosition, _endPosition, interpValue);
  }
  
  private protected void PeriodicFireLasers()
  {
    // TODO(bug): lasers can still fire during/after the death animation, we should check for
    // that start of that animation and not proceed with firing.
    if (!_defeated && Time.time > _canFire)
    {
      FireLasers(Vector3.down);
    }
  }
  
  private void FireLasers(Vector3 direction)
  {
    _fireRate = Random.Range(3f, 7f);
    _canFire = Time.time + _fireRate;
    Vector3 laserPosition = transform.position;
    // Ensure lasers come out of back of ship rather than front.
    if (direction == Vector3.up)
    {
      laserPosition += new Vector3(0, 2.75f, 0);
    }
    GameObject enemyLaser = Instantiate(_laserPrefab, laserPosition, Quaternion.identity);
    Laser[] lasers = enemyLaser.GetComponentsInChildren<Laser>();
    foreach (var laser in lasers)
    {
      laser.IsEnemyLaser = true;
      laser.LaserDirection = direction;
    }
  }

  private IEnumerator FireOnPowerUpsAndPlayerBehindRoutine()
  {
    // Prevent preternatural ability for enemies to instantly destroy fire on spawn.
    yield return new WaitForSeconds(1.5f);
    while (!_defeated)
    {
      // Fire on power ups that are in front of them.
      if (ObjectsInDirectionOfEnemy(
        _spawnManager.GetAllOnScreenPowerUps().Select(p => p.gameObject).ToList(), Vector3.down))
      {
        FireLasers(Vector3.down);
      }
      // TODO(bug): If player destroyed by enemy/boss this can null reference.
      // Fire on player behind them.
      if (ObjectsInDirectionOfEnemy(new List<GameObject> {_player.gameObject}, Vector3.up))
      {
        FireLasers(Vector3.up);
      }
      // TODO(Improvement): Rather than a static timer, maybe use WaitUntil either of the detection methods
      // above returns true?
      yield return new WaitForSeconds(2f);
    }
  }

  // If any of the objects provided are in either in front (Vector3.down) or behind (Vector3.up)
  // of the enemy specified then returns true, otherwise false.
  private bool ObjectsInDirectionOfEnemy(List<GameObject> gameObjects, Vector3 direction)
  {
    foreach (var target in gameObjects)
    {
      // TODO(Improvement): Could this be more elegantly done with Vector3.Angle/SignedAngle?
      // https://math.stackexchange.com/a/2587852
      float angleToObject = Mathf.Rad2Deg * (Mathf.Atan2(
        target.transform.position.y - this.gameObject.transform.position.y,
        target.transform.position.x - this.gameObject.transform.position.x));
      if (direction == Vector3.down)
      {
        // This is a 10 degree cone in front of the enemy. Deduced visually that lasers would hit targets.
        if (angleToObject > -95f && angleToObject < -85f)
        {
          return true;
        }
      } else if (direction == Vector3.up) 
      {
        // This is a 10 degree cone behind the enemy. Deduced visually that lasers would hit targets.
        if (angleToObject > 85f && angleToObject < 95f)
        {
          return true;
        }
      }
    }
    return false;
  }

  protected GameObject GetGameObjectFromCollisionOrCollider<T>(T coll)
  {
    GameObject gameObject;
    if (typeof(T) == typeof(Collider2D))
    {
      Collider2D otherCollider2d = (Collider2D)(object)coll;
      gameObject = otherCollider2d.gameObject;
    } else if (typeof(T) == typeof(Collision2D))
    {
      Collision2D otherCollision2d = (Collision2D)(object)coll;
      gameObject = otherCollision2d.gameObject;
    }
    else
    {
      Debug.LogError("Enemy trigger enter method called with invalid collision/collider.");
      throw new NotImplementedException();
    }

    return gameObject;
  } 

  private void OnTriggerEnter2D(Collider2D other)
  {
    OnTriggerEntered2D(other);
  }

  protected void OnTriggerEntered2D<T>(T otherColl2d)
  {
    GameObject other = GetGameObjectFromCollisionOrCollider(otherColl2d);
    
    if (other.CompareTag("Player"))
    {
      Player player = other.transform.GetComponent<Player>();
      if (player != null)
      {
        player.Damage();
      }
      PlayerDamageEnemy();
    } else if (other.CompareTag("Laser"))
    {
      Laser laser = other.GetComponent<Laser>();
      if (laser != null)
      {
        // Do not let other enemy lasers destroy other enemies.
        if (laser.IsEnemyLaser)
          return;
      }
      PlayerDamageEnemy();
      Destroy(other);
    } else if (other.CompareTag("Missile") || other.CompareTag("ShotgunShot"))
    {
      PlayerDamageEnemy();
      Destroy(other);
    }
  }

  private void OnTriggerStay2D(Collider2D other)
  {
    OnTriggerStayed2D(other);
  }

  protected void OnTriggerStayed2D<T>(T otherColl2d)
  {
    GameObject other = GetGameObjectFromCollisionOrCollider(otherColl2d);
    // If the player stays collided with the enemy it will continue to damage the enemy and the player.
    if (other.CompareTag("Player"))
    {
      _playerEnemyCollideTimeTotal += Time.deltaTime;
      if (_playerEnemyCollideTimeTotal >= _playerCollideTimeRedamage) 
      {
        PlayerDamageEnemy();
        Player player = other.transform.GetComponent<Player>();
        if (player != null)
        {
          player.Damage();
          _playerEnemyCollideTimeTotal = 0f;
        }
      }
    }
  }

  private void OnTriggerExit2D(Collider2D other)
  {
    OnTriggerExited2D();
  }
  
  protected void OnTriggerExited2D()
  {
    _playerEnemyCollideTimeTotal = 0f;
  }

  protected virtual void PlayerDamageEnemy()
  {
    if (_shieldsPrefab.activeSelf)
    {
      _shieldsPrefab.SetActive(false);
      return;
    }
    PlayerEnemyKill();
  }
  
  protected virtual void PlayerEnemyKill()
  {
    DestroyEnemy();
    if (_player)
    {
      _player.AddToScore(10);
    }
  }

  protected void WasDefeated()
  {
    _defeated = true;
  }

  protected void RemoveEnemyFromGame(float afterTime)
  {
    _spawnManager.RemoveEnemyFromGame(this, afterTime);
  }

  protected virtual void DestroyEnemy()
  {
    _animator.SetTrigger("OnEnemyDeath");
    WasDefeated();
    _speed = 0f;
    // TODO(Improvement): try to make this work in the future, at the moment it never gets to setting animFinished
    // as true.
    // StartCoroutine("PlayDeathAnimationThenDestroy");
    if (_audioSource.enabled)
    {
      _audioSource.Play();
    }
    Destroy(GetComponent<Collider2D>());
    RemoveEnemyFromGame(2.8f);
  }
  
  // TODO(Improvement): This is a possible alternative way to play the death anim and destroy the
  // object as soon as the animation finishes playing but it wasn't working.
  // private IEnumerator PlayDeathAnimationThenDestroy()
  // {
  //   this.gameObject.tag = "";
  //   Debug.Log("Waiting for Death anim of enemy to finish.");
  //   // yield return new WaitUntil(
  //   //   () => _animator.GetCurrentAnimatorStateInfo(
  //   //     0).normalizedTime > 1);
  //   yield return new WaitUntil(DeathAnimationFinishedPlaying);
  //   Debug.Log("Death anim for enemy finished playing. Destroying enemy.");
  //   Destroy(this.gameObject);
  // }
  //
  // private bool DeathAnimationFinishedPlaying()
  // {
  //   bool animFinished = false;
  //   for (int i = 0; i < _animator.layerCount; i++)
  //   {
  //     AnimatorStateInfo animState = _animator.GetCurrentAnimatorStateInfo(i);
  //     if (animState.IsName("OnDeathPlaying") && animState.normalizedTime >= 1.0f)
  //     {
  //       animFinished = true;
  //     }
  //   }
  //   return animFinished;
  // }
}

﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
  // How many times the enemy has repspawned at the top of the screen after reaching the bottom.
  private int _respawnCount = 0;  
  
  [SerializeField]
  private float _speed = 4f;
  
  [SerializeField]
  private AudioSource _audioSource;

  private Player _player;

  private Animator _animator;

  // If we have more game objects with shields we should make a parent class/category with that functionality.
  [SerializeField] 
  private GameObject _shieldsPrefab;
  // Keep track of how long enemy has been been in collision with player to re-damage if we stay collided for too long.
  private float _playerEnemyCollideTimeTotal = 0f;
  // Staying collided longer than this time by enemy will redamage player.
  private const float _playerCollideTimeRedamage = .3f; 

  public bool IsDefeated()
  {
    return _defeated;
  }

  public int GetRespawnCount()
  {
    return _respawnCount;
  }
  
  private void Start()
  {
    _player = GameObject.Find("Player").GetComponent<Player>();
    _animator = gameObject.GetComponent<Animator>();
    _audioSource = GetComponent<AudioSource>();
    _animator = gameObject.GetComponent<Animator>();
    _shieldsPrefab.SetActive(Random.value > 0.5);  // 0.0-0.5 == false, 0.5-1.0 == true.
    StartCoroutine(FireOnVisiblePowerUpsRoutine());

    if (!_player)
    {
      Debug.LogError("Player is null from Enemy.");
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
  }

  void Update()
  {
    CalculateMovement();
    PeriodicFireLasers();
  }

  private void CalculateMovement()
  {
    Vector3 moveShip = Vector3.down * (_speed * Time.deltaTime);
    transform.Translate(moveShip);

    if (transform.position.y <= -5f)
    {
      float randomX = Random.Range(-8f, 8f);
      transform.position = new Vector3(randomX, 7, 0);
      _respawnCount++;
    }
  }

  private void PeriodicFireLasers()
  {
    // TODO(bug): lasers can still fire during/after the death animation, we should check for
    // that start of that animation and not proceed with firing.
    if (!_defeated && Time.time > _canFire)
    {
      FireLasers();
    }
  }
  
  private void FireLasers()
  {
    _fireRate = Random.Range(3f, 7f);
    _canFire = Time.time + _fireRate;
    GameObject enemyLaser = Instantiate(_laserPrefab, transform.position, Quaternion.identity);
    Laser[] lasers = enemyLaser.GetComponentsInChildren<Laser>();
    foreach (var laser in lasers)
    {
      laser.IsEnemyLaser = true;
    }
  }
  
  private IEnumerator FireOnVisiblePowerUpsRoutine()
  {
    // Prevent preternatural ability for enemies to instantly destroy powerups on spawn.
    yield return new WaitForSeconds(1.5f);
    while (!_defeated)
    {
      if (PowerUpInFrontOfEnemy())
      {
        FireLasers();
      }
      yield return new WaitForSeconds(0.5f);
    }
  }

  private bool PowerUpInFrontOfEnemy()
  {
    GameObject[] powerUps = GameObject.FindGameObjectsWithTag("PowerUp");
    GameObject powerUpInFrontOfEnemy = null;
    foreach (var powerUp in powerUps)
    {
      // TODO(Improvement): Could this be more elegantly done with Vector3.Angle/SignedAngle?
      // https://math.stackexchange.com/a/2587852
      float angleToPowerUp = Mathf.Rad2Deg * (Mathf.Atan2(
        powerUp.transform.position.y - this.gameObject.transform.position.y,
        powerUp.transform.position.x - this.gameObject.transform.position.x));
      // This is a 10 degree cone in front of the enemy. Deduced visually that lasers would hit.
      if (angleToPowerUp > -95f && angleToPowerUp < -85f )
      {
        return true;
      }
    }
    return false;
  }

  private void OnTriggerEnter2D(Collider2D other)
  {
    if (other.CompareTag("Player"))
    {
      Player player = other.transform.GetComponent<Player>();
      if (player != null)
      {
        player.Damage();
      }
      PlayerDamageEnemy(other);
    } else if (other.CompareTag("Laser"))
    {
      Laser laser = other.GetComponent<Laser>();
      if (laser != null)
      {
        // Do not let other enemy lasers destroy other enemies.
        if (laser.IsEnemyLaser)
          return;
      }
      PlayerDamageEnemy(other);
      Destroy(other.gameObject);
    } else if (other.CompareTag("Missile") || other.CompareTag("ShotgunShot"))
    {
      PlayerDamageEnemy(other);
      Destroy(other.gameObject);

    }
  }
  
  private void OnTriggerStay2D(Collider2D other)
  {
    // If the player stays collided with the enemy it will continue to damage the enemy and the player.
    if (other.CompareTag("Player"))
    {
      _playerEnemyCollideTimeTotal += Time.deltaTime;
      if (_playerEnemyCollideTimeTotal >= _playerCollideTimeRedamage) 
      {
        PlayerDamageEnemy(other);
        Player player = other.transform.GetComponent<Player>();
        if (player != null)
        {
          player.Damage();
        }
      }
    }
  }

  private void OnTriggerExit2D(Collider2D other)
  {
    _playerEnemyCollideTimeTotal = 0f;
  }

  private void PlayerDamageEnemy(Collider2D other)
  {
    if (_shieldsPrefab.activeSelf)
    {
      _shieldsPrefab.SetActive(false);
      return;
    }
    PlayerEnemyKill(other);
  }
  
  private void PlayerEnemyKill(Collider2D other)
  {
    DestroyEnemy();
    if (_player)
    {
      _player.AddToScore(10);
    }
  }

  private void DestroyEnemy()
  {
    _animator.SetTrigger("OnEnemyDeath");
    _defeated = true;
    _speed = 0f;
    // TODO(Improvement): try to make this work in the future, at the moment it never gets to setting animFinished
    // as true.
    // StartCoroutine("PlayDeathAnimationThenDestroy");
    if (_audioSource.enabled)
    {
      _audioSource.Play();
    }
    Destroy(GetComponent<Collider2D>());
    Destroy(this.gameObject, 2.8f);
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

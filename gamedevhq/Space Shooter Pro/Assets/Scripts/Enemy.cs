using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class Enemy : MonoBehaviour
{
  
  [SerializeField]
  private GameObject _laserPrefab;
  private float _canFire = -1f;
  private float _fireRate = 3.0f;
  
  [SerializeField]
  private float _speed = 4f;
  
  [SerializeField]
  private AudioSource _audioSource;

  private Player _player;

  private Animator _animator;

  private void Start()
  {
    _player = GameObject.Find("Player").GetComponent<Player>();
    _animator = gameObject.GetComponent<Animator>();
    _audioSource = GetComponent<AudioSource>();

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
  }

  void Update()
  {
    CalculateMovement();
    FireLasers();
  }

  private void CalculateMovement()
  {
    Vector3 moveShip = Vector3.down * (_speed * Time.deltaTime);
    transform.Translate(moveShip);

    if (transform.position.y <= -5f)
    {
      float randomX = Random.Range(-8f, 8f);
      transform.position = new Vector3(randomX, 7, 0);
    }
  }

  private void FireLasers()
  {
    if (Time.time > _canFire)
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
  }

  private void OnTriggerEnter2D(Collider2D other)
  {
    if (other.tag == "Player")
    {
      Player player = other.transform.GetComponent<Player>();
      if (player != null)
      {
        player.Damage();
      }
      _animator.SetTrigger("OnEnemyDeath");
      _speed = 0f;
      // TODO: make this work in the future, at the moment it never gets to setting animFinished
      // as true.
      // StartCoroutine("PlayDeathAnimationThenDestroy");
      _audioSource.Play();
      Destroy(GetComponent<Collider2D>());
      Destroy(this.gameObject, 2.8f);
      
    } else if (other.tag == "Laser")
    {
      Laser laser = other.GetComponent<Laser>();
      if (laser != null)
      {
        // Do not let other enemy lasers destroy other enemies.
        if (laser.IsEnemyLaser)
          return;
      }
      Destroy(other.gameObject);
      if (_player)
      {
        _player.AddToScore(10);
      }
      _animator.SetTrigger("OnEnemyDeath");
      _speed = 0f;
      // StartCoroutine("PlayDeathAnimationThenDestroy");
      _audioSource.Play();
      Destroy(GetComponent<Collider2D>());
      Destroy(this.gameObject, 3.1f);
    }
  }
  
  // TODO: This is a possible alternative way to play the death anim and destroy the
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

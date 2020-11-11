﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Enemy : MonoBehaviour
{
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
    if (!_animator)
    {
      Debug.LogError("Animator is null for Enemy.");
    }
    if (_audioSource == null)
    {
      Debug.LogError("_laserAudioSource was null when creating enemy");
    }
  }

  void Update()
  {
   
    Vector3 moveShip = Vector3.down * _speed * Time.deltaTime;
    transform.Translate(moveShip);

    if (transform.position.y <= -5f)
    {
      float randomX = Random.Range(-8f, 8f);
      transform.position = new Vector3(randomX, 7, 0);
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
      // TODO: make this work in the future, at the moment it never gets to setting the animFinished
      // as true.
      // StartCoroutine("PlayDeathAnimationThenDestroy");
      if (!_audioSource.isPlaying)
      {
        _audioSource.Play();
      }
      Destroy(this.gameObject, 2.8f);
      
    } else if (other.tag == "Laser")
    {
      Destroy(other.gameObject);
      if (_player)
      {
        _player.AddToScore(10);
      }
      _animator.SetTrigger("OnEnemyDeath");
      _speed = 0f;
      // StartCoroutine("PlayDeathAnimationThenDestroy");
      if (!_audioSource.isPlaying)
      {
        _audioSource.Play();
      }
      Destroy(this.gameObject, 3.1f);
    }
  }

  private IEnumerator PlayDeathAnimationThenDestroy()
  {
    this.gameObject.tag = "";
    Debug.Log("Waiting for Death anim of enemy to finish.");
    // yield return new WaitUntil(
    //   () => _animator.GetCurrentAnimatorStateInfo(
    //     0).normalizedTime > 1);
    yield return new WaitUntil(DeathAnimationFinishedPlaying);
    Debug.Log("Death anim for enemy finished playing. Destroying enemy.");
    Destroy(this.gameObject);
  }

  private bool DeathAnimationFinishedPlaying()
  {
    bool animFinished = false;
    for (int i = 0; i < _animator.layerCount; i++)
    {
      AnimatorStateInfo animState = _animator.GetCurrentAnimatorStateInfo(i);
      if (animState.IsName("OnDeathPlaying") && animState.normalizedTime >= 1.0f)
      {
        animFinished = true;
      }
    }
    return animFinished;
  }

  private IEnumerator PlayDestroyedSound()
  {
    if (!_audioSource.isPlaying)
    {
      _audioSource.Play();
      yield return new WaitForSeconds(3.0f);
    }
  }

  private void OnDestroy()
  {
    // TODO: this doesn't work, enemy just immediately disappears. I'm doing this since
    // SpawnManager.OnPlayerDeath loops and destroys all the enemy's on screen bypassing
    // the explosion audio playing when we collide with laser or player...
    // StartCoroutine(PlayDestroyedSound());
  }
}

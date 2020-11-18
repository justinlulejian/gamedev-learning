﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class Laser : MonoBehaviour
{
  [SerializeField]
  private float _speed = 8.0f;
  {
    CalculateMovementAndOrDestroy();
  }

  private void CalculateMovementAndOrDestroy()
  {
    Move(IsEnemyLaser ? Vector3.down : Vector3.up);
  }

  private void Move(Vector3 direction)
  {
    transform.Translate(direction * (_speed * Time.deltaTime));

    if (transform.position.y > 8f || transform.position.y < -8f)
    {
      // TODO: this is meant to destroy triple shots, but it seems if you spam it you can get a few
      // triple shots to remain in the scene and they should be getting deleted.
      if (transform.parent != null)
      {
        Destroy(transform.parent.gameObject);
      }
      Destroy(this.gameObject);
    }
  }

  public bool IsEnemyLaser { get; set; } = false;

  // private void OnTriggerEnter2D(Collider2D other)
  // {
  //   if (other.tag == "Player" && IsEnemyLaser)
  //   {
  //     Player player = other.GetComponent<Player>();
  //     if (player != null)
  //     {
  //       player.Damage();
  //     }
  //   }
  // }


  //   public void PlaySound()
//   {
//     if (_audioSource == null)
//     {
//       Debug.LogError("laser audio source was null in laser playsound, weird!");
//     }
//     _audioSource.Play();
//   }
}

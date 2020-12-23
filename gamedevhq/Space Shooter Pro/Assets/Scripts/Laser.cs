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
  
  public bool IsEnemyLaser { get; set; } = false;
  
  private void Update()
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
      // TODO(bug): this is meant to destroy triple shots, but it seems if you spam it you can get
      // a few triple shots to remain in the scene and they should be getting deleted.
      if (transform.parent != null)
      {
        Destroy(transform.parent.gameObject);
      }
      Destroy(this.gameObject);
    }
  }

}

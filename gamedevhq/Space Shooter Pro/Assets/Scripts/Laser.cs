using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class Laser : MonoBehaviour
{
  [SerializeField]
  private float _speed = 8.0f;
  
  void Update()
  {
    CalculateMovementAndOrDestroy();
  }

  private void CalculateMovementAndOrDestroy()
  {
    if (IsEnemyLaser)
    {
      Move(Vector3.down);
    }
    else
    {
      Move(Vector3.up);
    }
  }

  private void Move(Vector3 direction)
  {
    transform.Translate(direction * (_speed * Time.deltaTime));

    if (transform.position.y > 8f || transform.position.y < -8f)
    {
      // TODO: this is meant to destroy triple shots, but it seems if you spam it you can get a few
      // to remain in the scene and they should be getting deleted.
      if (transform.parent != null)
      {
        Destroy(transform.parent.gameObject);
      }
      Destroy(this.gameObject);
    }
  }

  public bool IsEnemyLaser { get; set; } = false;

  private void OnTriggerEnter2D(Collider2D other)
  {
    if (other.CompareTag("Player") && IsEnemyLaser)
    {
      Debug.Log("Enemy laser hit player");
      Player player = other.GetComponent<Player>();
      if (player != null)
      {
        player.Damage();
      }
    }
  }
}

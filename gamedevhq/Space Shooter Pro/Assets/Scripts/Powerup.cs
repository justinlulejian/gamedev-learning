using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Rendering;
using Debug = UnityEngine.Debug;

public class Powerup : MonoBehaviour
{
  [SerializeField]
  private float _speed = 3.5f;

  [SerializeField] 
  private AudioClip _pickupAudioClip;

  private Renderer _renderer;

  // ID for powerups
  // 0 == triple
  // 1 == speed
  // 2 == shields
  [SerializeField]
  private int _powerupID;

  void Update()
  {
    transform.Translate(Vector3.down * (_speed * Time.deltaTime));
    if (transform.position.y <= -4.5f)
    {
      Destroy(this.gameObject);
    }
  }

    private void OnTriggerEnter2D(Collider2D other)
  {
    if (other.tag == "Player")
    {
      Player player = other.transform.GetComponent<Player>();
      AudioSource.PlayClipAtPoint(_pickupAudioClip, transform.position);
      if (player != null)
      {
        switch (_powerupID)
        {
          case 0:
            player.TripleShotActive();
            break;
          case 1:
            player.SpeedBoostPowerupActive();
            break;
          case 2:
            player.ShieldsPowerupActive();
            break;
          default:
            break;
        }
      }
      Destroy(this.gameObject);
    }
  }
}

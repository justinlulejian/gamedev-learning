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
  private float _speed = 3f;

  [SerializeField] 
  private AudioClip _pickupAudioClip;

  // ID for powerups
  // 0 == triple
  // 1 == speed
  // 2 == shields
  // 3 == ammo collectable
  // 4 == health collectable
  // 5 == missile
  // 6 == negative powerup
  // 7 == shotgun shot 
  [SerializeField]
  private int _powerupID;
  
  private SpawnManager _spawnManager;
  private bool _playerCollecting = false;
  [SerializeField] 
  private float _playerCollectingSpeedMultiplier = 2f;
  private Vector3 _lastPlayerPosition;

  protected void Start()
  {
    _spawnManager = GameObject.Find("Spawn_Manager").GetComponent<SpawnManager>();
    
    if (_pickupAudioClip == null)
    {
      Debug.LogError("Powerup does not have pickup audio clip.");
    }
    if (_spawnManager == null)
    {
      Debug.LogError("Spawn manager is null from Powerup.");
    }
  }

  protected virtual void Update()
  {
    if (_playerCollecting)
    {
      // Move towards player at an increased speed.
      transform.position = Vector3.MoveTowards(
        this.transform.position, _lastPlayerPosition,
        (_speed * _playerCollectingSpeedMultiplier) * Time.deltaTime);
    }
    else
    {
      transform.Translate(Vector3.down * (_speed * Time.deltaTime));
      if (transform.position.y <= -4.5f)
      {
        _spawnManager.RemovePowerUpFromGame(this);
      }
    }
    // Reset so player does not have to explicitly indicate it wants to stop collecting.
    _playerCollecting = false;

  }

  public void PlayerCollecting(bool collecting, Vector3 playerPosition)
  {

    _playerCollecting = collecting;
    _lastPlayerPosition = playerPosition;

  }

    private void OnTriggerEnter2D(Collider2D other)
  {
    if (other.CompareTag("Player"))
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
          case 3:
            player.CollectAmmo();
            break;
          case 4:
            if (player.GetLives() < 3)  // Prevent increasing lives beyond maximum.
            {
              player.CollectLife();
            }
            break;
          case 5:
            player.MissilePowerupActive();
            break;
          case 6:
            player.SpeedDecreasePowerupActive();
            break;
          case 7:
            player.ShotgunPowerupActive();
            break;
        }
      }
      _spawnManager.RemovePowerUpFromGame(this);
    } else if (other.CompareTag("Laser"))
    {
      // Only destroy if this is an enemy laser.
      if (
        other.transform.parent != null && 
        other.transform.parent.CompareTag("EnemyLaser"))
      {
        _spawnManager.RemovePowerUpFromGame(this);
      }
    }
  }
}

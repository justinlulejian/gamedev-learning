using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Powerup : MonoBehaviour
{
  [SerializeField]
  private float _speed = 3.5f;

  // ID for powerups
  // 0 == triple
  // 1 == speed
  // 2 == shields
  [SerializeField]
  private int _powerupID;
    
  void Update()
  {
    transform.Translate(Vector3.down * _speed * Time.deltaTime);
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
      if (player != null)
      {
        switch (_powerupID)
        {
          case 0:
            player.TripleShotActive();
            Debug.Log("Collected triple shot powerup");
            break;
          case 1:
            player.SpeedBoostPowerupActive();
            Debug.Log("Collected speed powerup");
            break;
          case 2:
            player.ShieldsPowerupActive();
            Debug.Log("Collected shield powerup");
            break;
          default:
            Debug.Log("Collected a powerup with an unknown ID:" + _powerupID);
            break;
        }
      }
      Destroy(this.gameObject);
    }
  }
}

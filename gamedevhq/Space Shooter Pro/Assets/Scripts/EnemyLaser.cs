using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyLaser : MonoBehaviour
{
    // TODO(Improvement): This was as second attempt to see if I could get the enemy laser to do just
    // one damage for the two laser prefabs, it didn't do any damage at all.
    // private void OnTriggerEnter2D(Collider2D other)
    // {
    //     Debug.Log("Enemy laser hit: " + other.tag);
    //     if (other.CompareTag("Player"))
    //     {
    //         Debug.Log("Enemy laser hit player");
    //         Player player = other.GetComponent<Player>();
    //         if (player != null)
    //         {
    //             player.Damage();
    //         }
    //     }
    // }
}

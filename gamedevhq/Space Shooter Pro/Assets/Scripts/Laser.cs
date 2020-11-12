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
  
  // private AudioSource _audioSource;

  // private void Start()
  // {
  //   _audioSource = GetComponent<AudioSource>();
  //   
  //   if (_audioSource == null)
  //   {
  //     // TODO: this gets hit but the sound still plays...why?
  //     Debug.LogError("laser audio source was null when creating laser");
  //   }
  // }

  public Vector3 direction;
  
  // private AudioSource _audioSource;

  // private void Start()
  // {
  //   _audioSource = GetComponent<AudioSource>();
  //   
  //   if (_audioSource == null)
  //   {
  //     // TODO: this gets hit but the sound still plays...why?
  //     Debug.LogError("laser audio source was null when creating laser");
  //   }
  // }

  void Update()
  {
    transform.Translate(ddirection * (_speed * Time.deltaTime));

    if (transform.position.y > 8f)
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

//   public void PlaySound()
//   {
//     if (_audioSource == null)
//     {
//       Debug.LogError("laser audio source was null in laser playsound, weird!");
//     }
//     _audioSource.Play();
//   }
}

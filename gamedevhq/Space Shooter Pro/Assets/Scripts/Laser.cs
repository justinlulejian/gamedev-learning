using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour
{
  [SerializeField]
  private float _speed = 8.0f;

  void Update()
  {
    transform.Translate(Vector3.up * _speed * Time.deltaTime);

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
}

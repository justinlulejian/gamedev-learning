using DefaultNamespace;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class Laser : PlayerProjectile
{
  [SerializeField]
  private float _speed = 8.0f;
  
  public bool IsEnemyLaser { get; set; } = false;
  
  public Vector3 LaserDirection { get; set; } = Vector3.down;

  private void Update()
  {
    CalculateMovementAndOrDestroy();
  }

  private void CalculateMovementAndOrDestroy()
  {
    Move(LaserDirection);
  }

  private void Move(Vector3 direction)
  {
    transform.Translate(direction * (_speed * Time.deltaTime));

    if (transform.position.y > 8f || transform.position.y < -8f)
    {
      // TODO(bug): this is meant to destroy triple shots, but it seems if you spam it you can get
      // a few triple shots to remain in the scene but they should be getting deleted. This also makes it difficult
      // to set these as child objects of other objects for tracking purposes since they'll destroy the container
      // they're in at this point.
      if (transform.parent != null)
      {
        Destroy(transform.parent.gameObject);
      }
      Destroy(this.gameObject);
    }
  }

}

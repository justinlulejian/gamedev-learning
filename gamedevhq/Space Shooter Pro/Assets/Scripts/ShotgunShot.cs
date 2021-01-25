using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;

public class ShotgunShot : PlayerProjectile
{
    [SerializeField]
    private float _speed = 4.0f;
    
    [SerializeField]
    private GameObject shotPrefab;

    private GameObject leftShot;
    private GameObject rightShot;
    private GameObject frontShot;

    private List<GameObject> shotsRemaining;
    private Dictionary<GameObject, Vector3> shotsToDirection;

    private bool _isReady;

    protected override void Start()
    {
        // See GetShotgunShots() for why this is necessary.
        if (_isReady)
        {
            return;
        }

        ReadyShotgunShots();
    }

    private void ReadyShotgunShots()
    {
        base.Start();
        if (shotPrefab == null)
        {
            Debug.LogError("ShotgunShot shot prefab appears to not be assigned.");
        }
        // TODO(Improvement): To allow for flexibility and simpler code, allow for number of shots
        // to be specified. Then divide the 180 deg in front of player by shot number and send the
        // shots along those paths. Could then instantiate them in a loop.
        
        // Whereas in TripleShot we set the lasers as child objects, I generate them dynamically here so that I can 
        // flexibly move them in different directions and allow the number of shots to be expanded in the future.
        leftShot = Instantiate(
            shotPrefab,
            transform.position, Quaternion.identity);
        rightShot = Instantiate(
            shotPrefab,
            transform.position, Quaternion.identity);
        frontShot = Instantiate(
            shotPrefab,
            transform.position, Quaternion.identity);
        shotsRemaining = new List<GameObject>() {leftShot, rightShot, frontShot};
        foreach (GameObject shot in shotsRemaining)
        {
            shot.transform.parent = this.transform;
        }
        shotsToDirection = new  Dictionary<GameObject, Vector3>()
        {
            {leftShot, new Vector3(-1, 1, 0)},   // Move diagonal upwards to the left. 
            {rightShot, new Vector3(1, 1, 0)},   // Move diagonal upwards to the right. 
            {frontShot, Vector3.up},
        };
        if (shotsRemaining == null || shotsRemaining.Count == 0)
        {
            Debug.LogError("ShotgunShot couldn't find any child shot objects.");
        }
        _isReady = true;
    }

    private void Update()
    {
        CalculateMovementAndOrDestroy();
    }

    private void CalculateMovementAndOrDestroy()
    {
        // Can't be foreach since we remove shotsRemaining in MoveShot if they go out of game bounds.
        for (int i = 0; i < shotsRemaining.Count; i++)
        {
            MoveShot(shotsRemaining[i], shotsToDirection[shotsRemaining[i]]);
        }
        
        if (shotsRemaining.Count == 0)
        {
            Destroy(this.gameObject);
        }
    }

    private void MoveShot(GameObject shot, Vector3 direction)
    {
        // Enemy object can destroy shot so this avoid a ref exception trying to move
        // a destroyed object.
        // TODO(Improvement): Could I restructure to avoid this null check on every update
        // loop? Maybe add as child objects instead?
        if (shot == null)
        {
            shotsRemaining.Remove(shot);
            return;
        }
        
        shot.transform.Translate(direction * (_speed * Time.deltaTime));

        if (shot.transform.position.y > 8f || shot.transform.position.y < -8f)
        {
            Destroy(shot);
            shotsRemaining.Remove(shot);
        }
    }
    
    public List<Transform> GetShotgunShots()
    {
        // TODO(Improvement): This is necessary because Player might call this method before Start() has been called
        // here which will then cause the WeaponManager to not know about the shots, and then cause the AvoidEnemy to
        // not known about them to avoid them. This allows up to workaround that, but it's not elegant. It's not worth
        // changing how Unity script ordering works just for a couple scripts though.
        if (!_isReady)
        {
            ReadyShotgunShots();
        }
        
        List<Transform> childShots = new List<Transform>();
        for (int i = 0; i < this.transform.childCount; i++)
        {
            childShots.Add(this.transform.GetChild(i));
        }
        
        return childShots;
    }

    protected override void OnDestroy()
    {
        foreach (Transform shotgunShot in GetShotgunShots())
        {
            _weaponManager.RemovePlayerShot(shotgunShot.transform);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class BossCircleAttack : MonoBehaviour
{
    [SerializeField] private GameObject _circleAttackBullet;
    [SerializeField] private float _speed = 2f;
    // The separation of each bullet in the circle from one another by angle. The lower the value the more bullets will
    // appear and higher the value less bullets will appear.
    [SerializeField] private float _bulletAngleSpread = 30f;
    [SerializeField] private AudioSource _attackAudio;

    private GameObject _boss;

    // How far away the bullets will spawn from boss.
    [SerializeField] private float _bulletSpawnRadius = 2f;

    private HashSet<GameObject> _bulletsSpawned;
    // TODO: 
    // Eighth: do audio for this, but not per shot, just once in this attack (scifi shot 9 play once)
    // Ninth: finish laser attack audio (scifi shot 7 on loop)
    // tenth: make shotgun shots rotate around the circle before they shoot or simpler just have them breath
    // smaller and larger as they move.


    // Start is called before the first frame update
    void Start()
    {
        _boss = transform.parent.gameObject;
        Vector3 bossPosition = _boss.transform.position;
        _bulletsSpawned = new HashSet<GameObject>();
        _attackAudio = this.transform.GetComponent<AudioSource>();
        
        if (_boss == null) {
            Debug.LogError("Boss circle attack cannot find boss, attack will not spawn relative to it.");
        }
        if (_attackAudio == null) {
            Debug.LogError("Attack audio source not found on circle shot.");
        }

        Dictionary<float, Vector3> bulletSpawnPositionsAndRotations = CalculateBulletSpawnPositions(_bulletAngleSpread);

        foreach (KeyValuePair<float, Vector3> keyValuePair in bulletSpawnPositionsAndRotations)
        {
            Quaternion bulletRotation = Quaternion.Euler(0, 0, keyValuePair.Key);
            GameObject bullet =
                Instantiate(_circleAttackBullet, bossPosition + keyValuePair.Value, bulletRotation);
            bullet.transform.parent = this.transform;
            _bulletsSpawned.Add(bullet);
        }

    }

    // Generate a series of positions where bullets will be spawn in a circle around the boss. There will be (360 /
    // angle step) positions that will be generated.
    private Dictionary<float, Vector3> CalculateBulletSpawnPositions(float angleStep)
    {
        Dictionary<float, Vector3> spawnPositions = new Dictionary<float, Vector3>();
        for (float angle = angleStep; angle <= 360f; angle += angleStep)
        {
            Vector3 spawnPosition = new Vector3();
            spawnPosition.x = _bulletSpawnRadius * Mathf.Cos(angle * Mathf.Deg2Rad);
            spawnPosition.y = _bulletSpawnRadius * Mathf.Sin(angle * Mathf.Deg2Rad);
            // Z rotation angles are 0 degree up, 90 left, 180 down, 270 right, 360 up. I want the bullets to shoot out
            // in the direction of angle like in a normal circle with 0 degree right, 90 up, 180 left, 270 down, and 360
            // right so we subtract 90 to convert to the proper direction we want.
            float eulerZ =  angle - 90;
            spawnPositions[eulerZ] = spawnPosition;
            Debug.Log($"Generated a bullet at angle {angle.ToString()}");
        }
        
        return spawnPositions;
    }

    // Update is called once per frame
    void Update()
    {
        MoveBulletsOrDestroy();
    }

    private void MoveBulletsOrDestroy()
    {
        if (_bulletsSpawned.Count == 0)
        {
            Destroy(this.gameObject);  
        }
        
        foreach (GameObject bullet in _bulletsSpawned)
        {
            bullet.transform.Translate(Vector3.up * (_speed * Time.deltaTime));
        }

        List<GameObject> bulletsToRemove = new List<GameObject>();
        foreach (GameObject bullet in _bulletsSpawned)
        {
            if (OutOfPlayArea(bullet))
            {
                bulletsToRemove.Add(bullet);
                Destroy(bullet);
            }
        }

        foreach (GameObject bullet in bulletsToRemove)
        {
            _bulletsSpawned.Remove(bullet);
        }
    }

    // TODO(Improvement): Create a generic helper function for this so we can use it in multiple places.
    private bool OutOfPlayArea(GameObject obj)
    {
        if (obj.transform.position.x < -11f || obj.transform.position.x > 11f)
        {
            return true;
        }
        if (obj.transform.position.y < -8f || obj.transform.position.y > 8f)
        {
            return true;
        }

        return false;
    }
}

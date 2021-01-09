using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class BossLaserAttack : MonoBehaviour
{
    [SerializeField] private GameObject _laserChargePrefab;
    [SerializeField] private GameObject _laserShotPrefab;

    private GameObject _laserChargeInstance;
    private GameObject _laserShotInstance;
    private float _currentChargingTime;
    private float _currentShotTime;
    private float _totalTimeToCharge = 5f;
    private float _totalTimeToShoot = 3f;
    
    void Start()
    {
        StartCoroutine(LaserChargeRoutine());
    }

    private IEnumerator LaserChargeRoutine()
    {
        GameObject laserChargeInstance = Instantiate(
            _laserChargePrefab, transform.position + new Vector3(0, -3, 0), Quaternion.identity);
        laserChargeInstance.transform.parent = this.transform;
        yield return new WaitForSeconds(_totalTimeToCharge);
        Destroy(laserChargeInstance);   
        StartCoroutine(LaserShotRoutine());
    }

    private IEnumerator LaserShotRoutine()
    {
        GameObject laserShotInstance = Instantiate(
            _laserShotPrefab, transform.position + new Vector3(0, -5.5f, 0), Quaternion.identity);
        laserShotInstance.transform.parent = this.transform;
        yield return new WaitForSeconds(_totalTimeToShoot);
        Destroy(laserShotInstance);
        Destroy(this.gameObject);
    }
}

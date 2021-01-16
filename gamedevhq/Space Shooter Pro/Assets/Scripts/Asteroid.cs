using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class Asteroid : MonoBehaviour
{
    [SerializeField] 
    private float _rotationSpeed = 19f;

    [SerializeField] 
    private GameObject _explosionPrefab;
    
    private SpawnManager _spawnManager;

    private void Start()
    {
        _spawnManager = GameObject.Find("Spawn_Manager").GetComponent<SpawnManager>();
        if (!_spawnManager)
        {
            Debug.LogError("Asteroid couldn't find spawn manager script object.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.forward * (_rotationSpeed * Time.deltaTime));
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Laser") || other.CompareTag("Missile") || other.CompareTag("ShotgunShot"))
        {
            Instantiate(_explosionPrefab, transform.position, Quaternion.identity);
            Destroy(other.gameObject);
            Destroy(this.gameObject, 0.25f);
            
        }
    }

    private void OnDestroy()
    {
        if (_spawnManager == null)
        {
            Debug.Log("Spawnmanager was null in Asteroid onDestroy not calling start" +
                           " spawning. May be due to unexpected scene end.");
            return;
        }
        _spawnManager.StartSpawning();
    }
}

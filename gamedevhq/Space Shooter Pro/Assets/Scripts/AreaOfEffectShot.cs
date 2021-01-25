using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaOfEffectShot : MonoBehaviour
{
    [SerializeField] private float _speed = 2.5f;
    [Tooltip("The time until the shot will explode and spread it's AOE.")]
    [SerializeField] private float _detonationTimer = 2f;

    private Transform _aoeExplosion;
    
    private float _currentDetonationTimer;
    private bool _isDetonated;

    private void Start()
    {
        try
        {
          _aoeExplosion = transform.GetChild(0);
        }
        catch (IndexOutOfRangeException e)
        {
           Debug.LogError("Area of effect shot could not find child explosion object.");
        }
    }

    private void Update()
    {
        if (!_isDetonated)
        {
            if (_currentDetonationTimer >= _detonationTimer)
            {
                AoEDetonation();
            }
        
            transform.Translate(Vector3.down * (_speed * Time.deltaTime));
            _currentDetonationTimer += Time.deltaTime;
        }
    }

    public void AoEDetonation()
    {
        _aoeExplosion.gameObject.SetActive(true);
        _isDetonated = true;
        Destroy(GetComponent<CircleCollider2D>());
        Destroy(GetComponent<SpriteRenderer>());
        StartCoroutine(DestroyOnExplosionCompletion(_aoeExplosion.gameObject));
    }

    // Destroy this controller once the AOE has finished.
    private IEnumerator DestroyOnExplosionCompletion(GameObject aoeExplosion)
    {
        yield return new WaitUntil(() => aoeExplosion == null);
        Destroy(this.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            AoEDetonation();
        }
    }
}

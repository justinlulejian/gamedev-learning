using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaOfEffectExplosion : MonoBehaviour
{
    [Tooltip("The scale multipler for how large the AOE explosion will become before stopping.")] 
    [SerializeField]
    private float _scaleMultiplier = 6f;
    [Tooltip("The time the AOE will remain active to damage the player.")]
    [SerializeField] 
    private float _aoeTimer = 3f;
    
    private float _currentLerpScaleTime;
    private Vector3 _startScale;
    private Vector3 _endScale;

    private void Start()
    {
        _startScale = transform.localScale;
        _endScale = _startScale * _scaleMultiplier;
    }
    
    // https://easings.net/#easeInSine
    private static float EaseInSine(float t)
    {
        return 1f - Mathf.Cos((t * Mathf.PI) / 2f);
    }

    private void Update()
    {
        ExpandAoE();
    }

    // Grow the AoE from it's initial size to a large size to show it exploding outward.
    private void ExpandAoE()
    {
        _currentLerpScaleTime += Time.deltaTime;
        // Anim completed.
        if (_currentLerpScaleTime > _aoeTimer)
        {
            Destroy(this.gameObject);
        }

        float interpValue = _currentLerpScaleTime / _aoeTimer;
        interpValue = EaseInSine(interpValue);
        transform.localScale = Vector3.Lerp(_startScale, _endScale, interpValue);
    }

    private void OnDestroy()
    {
        
        if (transform.parent.gameObject != null)
        {
            Destroy(transform.parent.gameObject);
        }
    }
}

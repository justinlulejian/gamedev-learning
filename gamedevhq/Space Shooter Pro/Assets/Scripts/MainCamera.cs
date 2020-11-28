using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class MainCamera : MonoBehaviour
{

    private Transform _cameraTransform;

    [SerializeField] 
    private float _cameraShakeTime = 0f;

    private Vector3 _initialCameraPosition;

    private void Awake()
    {
        if (_cameraTransform == null)
        {
            _cameraTransform = this.transform;
        }
    }

    private void OnEnable()
    {
        _initialCameraPosition = transform.position;  
    }

    private void Update()
    {
        if (_cameraShakeTime > 0f)
        {
            transform.position = _initialCameraPosition + UnityEngine.Random.insideUnitSphere * .05f;
            _cameraShakeTime -= Time.deltaTime;
        }
        else
        {
            _cameraShakeTime = 0f;
            transform.position = _initialCameraPosition;
        }
        
    }

    
    /// <summary>
    /// Shake camera for a specified amount of time.
    /// </summary>
    /// <param name="shakeTime"> Time duration in seconds to shake camera.</param>
    public void ShakeCamera(float shakeTime)
    {
        _cameraShakeTime = shakeTime;
    }

}

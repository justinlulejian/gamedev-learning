using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossLaserShot : MonoBehaviour
{
    private Vector3 _scaleIncrement = new Vector3(-0.05f, -0.05f, 0);

    private void Start()
    {
        throw new NotImplementedException();
    }

    void Update()
    {
        // Grow/shrink sprite as an animation.
        transform.localScale += _scaleIncrement;
        if (transform.localScale.x < 5.0f || transform.localScale.x > 5.5f)
        {
            _scaleIncrement = -_scaleIncrement;
        }
        
    }
}

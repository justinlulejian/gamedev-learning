using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossLaserCharging : MonoBehaviour
{
    private Vector3 _scaleIncrement = new Vector3(-0.05f, -0.05f, 0);

    void Update()
    {
        // Grow/shrink sprite as an animation.
        transform.localScale += _scaleIncrement;
        if (transform.localScale.x < 1.0f || transform.localScale.x > 2.0f)
        {
            _scaleIncrement = -_scaleIncrement;
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotgunShotPowerup : Powerup
{
    private Vector3 _scaleIncrement = new Vector3(-0.05f, -0.05f, 0);

    protected override void Update()
    {
        // Grow/shrink sprite as an animation.
        transform.localScale += _scaleIncrement;
        if (transform.localScale.x < 2.0f || transform.localScale.x > 4.0f)
        {
            _scaleIncrement = -_scaleIncrement;
        }
        
        base.Update();
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotgunShotPowerup : Powerup
{
    private SpriteRenderer _renderer;
    private Vector3 scaleIncrement = new Vector3(-0.05f, -0.05f, -0.05f);
    private float originalScaleValue;

    private void Start()
    {
        // Assuming uniform scale.
        originalScaleValue = transform.localScale.x;
        base.Start();
    }

    void Update()
    {
        // Grow/shrink sprite as an animation.
        transform.localScale += scaleIncrement;
        if (transform.localScale.x < 2.0f || transform.localScale.x > 4.0f)
        {
            scaleIncrement = -scaleIncrement;
        }
        
        base.Update();
    }
}

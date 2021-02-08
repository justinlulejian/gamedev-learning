using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeRed : MonoBehaviour
{
    private Material _material;
    private void OnEnable()
    {
        _material = GetComponent<MeshRenderer>().material;
        // register events
        Player.onTurnRed += PlayerOn_onTurnRed;
    }

    private void PlayerOn_onTurnRed()
    {
        _material.color = Color.red;
    }

    private void OnDisable()
    {
        // deregister events
        Player.onTurnRed -= PlayerOn_onTurnRed;
    }
}

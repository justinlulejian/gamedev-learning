using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cube : MonoBehaviour
{
    private MeshRenderer _meshRenderer;

    private void Start()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
        if (_meshRenderer == null)
        {
            Debug.Log("meshrenderer is null on cube");
        }
       
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I) && _meshRenderer.enabled)
        {
            StartCoroutine(HideCubeForSeconds(5f));
        }
    }

    private IEnumerator HideCubeForSeconds(float seconds)
    {
        _meshRenderer.enabled = false;
        Debug.Log("Set meshrenderer to false");
        yield return new WaitForSeconds(seconds);
        _meshRenderer.enabled = true;
        Debug.Log("Set meshrenderer to true");
    }
}

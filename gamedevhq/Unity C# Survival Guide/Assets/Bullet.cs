using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnEnable()
    {
        Invoke("Hide", 1);
    }

    void Hide()
    {
        Debug.Log("hiding bullet");
        this.gameObject.SetActive(false);
    }
}

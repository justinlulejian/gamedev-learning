using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private float _speed;
    
    public float Speed {
        get
        {
            return _speed;
        }
        private set
        {
            _speed = value;
        } 
    }

    public string Name { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        Speed = 1.0f;
        Debug.Log($"Speed is {Speed.ToString()}");
        
        Name = "Name";
        Debug.Log($"Name is {Name}");
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

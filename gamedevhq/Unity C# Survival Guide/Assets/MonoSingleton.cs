using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
{
    private static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                // Debug.Log(typeof(T).ToString());
                // Debug.Log($"Creating new single of type: {typeof(T).ToString()}.");
                // Instance = new GameObject(typeof(T).ToString()).AddComponent<T>();
            }

            return _instance;
        }
        private set => _instance = value;
    }

    private void Awake()
    {
        _instance = this as T;
    }

    public virtual void Init() {}
}

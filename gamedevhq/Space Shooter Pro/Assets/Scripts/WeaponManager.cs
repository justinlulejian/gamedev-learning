using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    private List<Transform> _playerShots = new List<Transform>();

    public void AddPlayerShot(Transform playerShot)
    {
        _playerShots.Add(playerShot);
    }
    
    public void RemovePlayerShot(Transform playerShot)
    {
        _playerShots.Remove(playerShot);
    }

    public List<Transform> GetPlayerShots()
    {
        return _playerShots;
    }
}

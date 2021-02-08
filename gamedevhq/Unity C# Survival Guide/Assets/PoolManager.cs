using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PoolManager : MonoSingleton<PoolManager>
{
    [SerializeField]
    private static List<GameObject> _bulletPool;
    [SerializeField] private GameObject _bulletContainer;

    [SerializeField] private GameObject _bulletPrefab;

    private void Start()
    {
        _bulletPool = GenerateBullets(10);
    }

    private List<GameObject> GenerateBullets(int numBullets)
    {
        List<GameObject> bullets = new List<GameObject>();
        for (int i = 0; i < numBullets; i++)
        {
            GameObject bullet = Instantiate(_bulletPrefab);
            bullet.transform.parent = _bulletContainer.transform;
            bullet.SetActive(false);
            bullets.Add(bullet);
        }
        return bullets;
    }

    public GameObject RequestBullet()
    {
        // TODO: this is empty/exception once we exhaust all items, might need an if before doing first to prevent it?
        var inactiveBullet = _bulletPool.FirstOrDefault(b => !b.activeInHierarchy);
        if (inactiveBullet != null)
        {
            inactiveBullet.SetActive(true);
            return inactiveBullet;
        }
        
        _bulletPool = GenerateBullets(1);
        return RequestBullet();

    }
}

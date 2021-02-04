using System.Collections.Generic;
using UnityEngine;

public class EnemyContainer : MonoBehaviour
{
    private List<Enemy> _enemies;
    void Start()
    {
        _enemies = new List<Enemy>();
    }

    public List<Enemy> GetEnemies()
    {
        return _enemies;
    }

    public int EnemyCount()
    {
        return _enemies.Count;
    }

    public void AddEnemy(Enemy enemy)
    {
        _enemies.Add(enemy);
        enemy.transform.parent = this.transform;
    }
    
    public void AddEnemy(GameObject enemy)
    {
        _enemies.Add(enemy.GetComponent<Enemy>());
        enemy.transform.parent = this.transform;
    }

    public void RemoveEnemy(Enemy enemy)
    {
        _enemies.Remove(enemy);
        Destroy(enemy.gameObject);
    }
    
    public void RemoveEnemy(Enemy enemy, float afterTime)
    {
        _enemies.Remove(enemy);
        Destroy(enemy.gameObject, afterTime);
    }
    
    public void RemoveAllEnemies()
    {
        for (int i = 0; i < _enemies.Count; i++)
        {
            Enemy enemy = _enemies[i];
            _enemies.RemoveAt(i);
            Destroy(enemy.gameObject);
        }
    }
    
    public bool NoEnemies()
    {
        return _enemies.Count == 0;
    }
}

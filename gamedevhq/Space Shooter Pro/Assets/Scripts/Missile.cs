using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Missile : MonoBehaviour
{
    
    [SerializeField]
    private float _speed = 8f;

    private GameObject _nearestEnemy = null;
    private Enemy _nearestEnemyObj = null;
    private int _nearestEnemyRespawnCount;
    
    void Start()
    {
        FindNearEnemyAndRespawnCount();
    }

    private void FindNearEnemyAndRespawnCount()
    {
        _nearestEnemy = FindNearestEnemy();
        if (_nearestEnemy != null)
        {
            _nearestEnemyObj = _nearestEnemy.GetComponent<Enemy>();
            _nearestEnemyRespawnCount = _nearestEnemyObj.GetRespawnCount();
        }
        // TODO(Improvement): Mark the enemy in some way that indicates it's the one being tracked by the
        // missile, a crosshair or something that matches the theme?
    }
    
    // TODO(Improvement): Missiles should refind next nearest enemy has already been targeted by another
    // missile/weapon.
    private GameObject FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject closestEnemy = null;
        float distanceToClosestEnemy = Mathf.Infinity;
        foreach (var enemy in enemies)
        {
            Enemy enemyObj = enemy.GetComponent<Enemy>();
            // Prevent missiles from moving towards enemy during death anim.
            if (!enemyObj.IsDefeated())
            {
                float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);
                if (distanceToEnemy < distanceToClosestEnemy)
                {
                    distanceToClosestEnemy = distanceToEnemy;
                    closestEnemy = enemy;
                }
            }
        }
        return closestEnemy;
    }

    private void Update()
    {
        CalculateMovementAndOrDestroy();
    }
    
    private void CalculateMovementAndOrDestroy()
    {
        // TODO(bug): If missile is fired before enemies appear it doesn't start tracking when they do appear.
        // Seek the enemy if one is present, otherwise just fly in a line upwards.
        MoveToEnemyOrDestroy();
    }

    private void MoveToEnemyOrDestroy()
    {
        float distanceToEnemy = Mathf.Infinity;
        if (_nearestEnemy != null)
        {
            distanceToEnemy = Vector3.Distance(transform.position, _nearestEnemy.transform.position);
        }

        // If the missile goes off-screen or collides with enemy it should be destroyed.
        if (
            transform.position.y > 8f || transform.position.y < -8f || 
            Mathf.Approximately(distanceToEnemy, 0f))
        {
            Destroy(this.gameObject);
        }

        // TODO(Improvement): Faster check than null?
        if (_nearestEnemy == null)
        {
            transform.Translate(Vector3.up * (_speed * Time.deltaTime));
            return;
        }
        
        MoveTowardsEnemy();
    }

    private void MoveTowardsEnemy()
    {
        // TODO(Improvement): Supposedly better to use transform.Translate but that causes the missile to
        // get close then move away. 
        // Below was from https://docs.unity3d.com/ScriptReference/Vector3.MoveTowards.html
        // But this mentions translate being better: http://answers.unity.com/answers/1089825/view.html
        Transform missileTransform = transform;
        Transform enemyTransform = _nearestEnemy.transform;

        // If the enemy respawns at the top of the screen re-find the nearest enemy so we don't track
        // the same enemy when it loops back to the top of the screen.
        if (_nearestEnemyObj.GetRespawnCount() > _nearestEnemyRespawnCount)
        {
            FindNearEnemyAndRespawnCount();
        }
        
        transform.position = Vector3.MoveTowards(
            missileTransform.position, enemyTransform.position, _speed * Time.deltaTime);
        
        // Rotate missile to point towards enemy. From http://answers.unity.com/comments/651932/view.html.
        Vector3 vectorToTarget = enemyTransform.position - missileTransform.position;
        float angle = (Mathf.Atan2(vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg) - 90;
        Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);
        transform.rotation = Quaternion.Slerp(missileTransform.rotation, q, Time.deltaTime * 20.0f);
    }
}

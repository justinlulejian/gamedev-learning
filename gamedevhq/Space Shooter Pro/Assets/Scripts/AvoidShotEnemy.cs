using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

// This Enemy avoids shots that are within _avoidDistanceTrigger and that are actually travelling towards it. Otherwise
// it behaves like a normal enemy.
public class AvoidShotEnemy : Enemy
{
    [SerializeField] 
    private float _avoidDistanceTrigger = 5f;
    
    private WeaponManager _weaponManager;

    protected override void Start()
    {
        base.Start();
        _weaponManager = GameObject.Find("Weapon_Manager").GetComponent<WeaponManager>();
        
        if (_weaponManager == null) {
            Debug.LogError("Weapons manager is null when creating player avoid shot enemy.");
        }
    }

    protected override void CalculateMovement()
    {
        List<Transform> playerShots = _weaponManager.GetPlayerShots();
        Transform closestPlayerShot = ClosestPlayerShotInAvoidRange(playerShots);
        
        // TODO: confirm this being null returns false.
        if (closestPlayerShot && PlayerShotOnCollisionCourse(closestPlayerShot))
        {
            // TODO: Add if check if cooldown has passed for moving. Then reset cooldown if it has passed. We can use
            // player fire logic, but it'll only be subtracted from if we're not actively trying to move away from a
            // shot already.
            AvoidPlayerShots(closestPlayerShot);
        }
        
        // TODO: see how this interacts before/during/after avoiding.
        StraightDownMovement();
    }

    private Transform ClosestPlayerShotInAvoidRange(List<Transform> playerShots)
    {
        // If I return null, will and if check later return false for it? Is that best practice?
        Transform closestPlayerShot = null;
        float closestPlayerShotDistance = Mathf.Infinity;
        foreach (Transform playerShot in playerShots)
        {
            // TODO: see in debug why this could be null? Shouldn't it be getting removed on destroy?
            if (playerShot)
            {
                float playerShotDistance = Vector3.Distance(playerShot.position, this.transform.position);
                if (playerShotDistance <= _avoidDistanceTrigger)
                {
                    if (playerShotDistance <= closestPlayerShotDistance)
                    {
                        closestPlayerShot = playerShot;
                    }
                }
            }
            
        }
        return closestPlayerShot;
    }

    // TODO(bug?): Many of the unity examples do a raycast in FixedUpdate since Physics calcs should be performed there
    // but how I've structured this doesn't seem to allow that. Is that still okay?
    private bool PlayerShotOnCollisionCourse(Transform playerShot)
    {
        ContactFilter2D contactFilter = new ContactFilter2D().NoFilter();
        // 30 is arbitrarily chosen since I don't think we'll ever have more than 30 colliders on screen at one time.
        RaycastHit2D[] hits = new RaycastHit2D[30];
        Physics2D.Raycast(playerShot.position, Vector3.up, contactFilter:contactFilter, results:hits);
        foreach (RaycastHit2D hit in hits)
        {
            // TODO: For some reason it takes two shots to destroy enemy...
            if (hit.transform == this.transform)
            {
                Debug.DrawRay(playerShot.position, transform.TransformDirection(Vector3.up) * hit.distance, Color.yellow);
                Debug.Log("Player shot would have hit enemy.");
                return true;
            }
        }
        Debug.DrawRay(playerShot.position, transform.TransformDirection(Vector3.up) * 1000, Color.red);
        Debug.Log("Player shot would NOT have hit enemy.");
        return false;
    }

    // TODO(Improvement): Multiple shots could be coming at this enemy, an increased difficulty setting could enable
    // this avoid to also try to avoid multiple nearby shots too.
    // Jump the enemy to the left/right but out of the path of player shot. 
    private void AvoidPlayerShots(Transform playerShots)
    {
        // https://docs.unity3d.com/ScriptReference/Physics.Raycast.html
        // First draw a virtual line in the direction the shot(s) are going based on their position and rotation. 
        
        // Calculate a movement spot that is left or right of enemy (decide randomly) where (by either drawing a circle
        // around this enemy or by accessing the polygon collider info) none of those lines will intersect that.
        
        // Initiate movement towards that with a movement type that "jumps" (quick movement at beginning, slower towards
        // end, lerp?). 
        
        // Detect when we've made it to the end movement and indicate that so that we can then start subtracting from
        // the cooldown time.
    }

    private void AvoidPlayerShotsAnim()
    {
        // TODO: Also OnEnemyDestroy anim spawns really big vs size of enemy, can we scale it down?
        // Spin the sprite on the rotation.y access when it avoids a shot. Will do a 360deg rotation quickly. Slerp?
    }
}

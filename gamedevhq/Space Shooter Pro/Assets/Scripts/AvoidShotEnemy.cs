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

    // private bool _avoidingPlayerShot;
    private Vector3 _avoidMovePosition;
    private bool _avoidAnimRunning;
    private float _movementCurrentSlerpTime;
    private float _movementSlerpTime;
    private Quaternion _origRotation;
    private Quaternion _destRotation;
    
    private WeaponManager _weaponManager;

    protected override void Start()
    {
        base.Start();
        _avoidMovePosition = transform.position + new Vector3(3, 0, 0);
        _movementSlerpTime = _movementLerpTime = 1.0f;
        _origRotation = this.transform.rotation;
        Debug.Log($"tranform.rotation orig: {transform.rotation.eulerAngles.ToString()}");
        _destRotation =  Quaternion.Euler(0, 360, 0) * transform.rotation;
        Debug.Log($"_destRotation: {_destRotation.eulerAngles.ToString()}");
        Debug.Log($"Q.Euler(0,360,0): {Quaternion.Euler(0,360,0).eulerAngles.ToString()}");
        // StartCoroutine(AnimateAroundAxis(this.transform, Vector3.up, 360, _movementSlerpTime));

        
        _weaponManager = GameObject.Find("Weapon_Manager").GetComponent<WeaponManager>();
        
        if (_weaponManager == null) {
            Debug.LogError("Weapons manager is null when creating player avoid shot enemy.");
        }
    }

    // TODO: Next: figure out the right/best way to have the avoid consistently avoid the lasers and then expand/test to
    // shotgunshot and missile (missile may just always win, nbd). I'd like it if the adjustment was dynamic so that
    // it'll just move enough out of the way to miss rather than a static distance, but if too complicated then just do
    // a static amount that works at least for lasers/shotgunshot. Also make sure cooldown exists because that'll prevent
    // them from indef avoid shots.
    protected override void CalculateMovement()
    {
        
        List<Transform> playerShots = _weaponManager.GetPlayerShots();
        Transform closestPlayerShot = ClosestPlayerShotInAvoidRange(playerShots);
        
        // TODO: confirm this being null returns false.
        if (closestPlayerShot && PlayerShotOnCollisionCourse(closestPlayerShot))
        {
            Debug.Log("Avoid enemy will try to avoid.");
            // TODO: Add if check if cooldown has passed for moving. Then reset cooldown if it has passed. We can use
            // player fire logic, but it'll only be subtracted from if we're not actively trying to move away from a
            // shot already.
            AvoidPlayerShots(closestPlayerShot);
            return;
        } 
        Debug.Log("Avoid enemy not trying to avoid.");
        // TODO: see how this interacts before/during/after avoiding works.
        // StraightDownMovement();
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
                Debug.Log("Player shot will hit enemy.");
                return true;
            }
        }
        Debug.DrawRay(playerShot.position, transform.TransformDirection(Vector3.up) * 1000, Color.red);
        Debug.Log("Player shot will NOT hit enemy.");
        return false;
    }

    // TODO(Improvement): Multiple shots could be coming at this enemy, an increased difficulty setting could enable
    // this avoid to also try to avoid multiple nearby shots too.
    // Jump the enemy to the left/right but out of the path of player shot. 
    private void AvoidPlayerShots(Transform playerShot)
    {

        // https://easings.net/#easeOutCirc
        float easeOutCircT(float t)
        {
            return Mathf.Sqrt(1.0f - Mathf.Pow(t - 1.0f, 2.0f));
        }
        
        _movementCurrentLerpTime += Time.deltaTime;
        if (_movementCurrentLerpTime > _movementLerpTime)
        {
            _movementCurrentLerpTime = 0.0f;
            _startPosition = transform.position;
            _avoidMovePosition = transform.position + new Vector3(3, 0, 0);
            return;
        }

        float interpValue = _movementCurrentLerpTime / _movementLerpTime;
        interpValue = easeOutCircT(interpValue);
        Debug.Log($"Interp value: {interpValue.ToString()}");
        transform.position = Vector3.Lerp(_startPosition, _avoidMovePosition, interpValue);
        
        AvoidPlayerShotsAnim();

        // _avoidingPlayerShot = true;
        // Vector3 avoidMovePosition = transform.position + new Vector3(10f, 0, 0);
        // transform.position = Vector2.MoveTowards(
        //     transform.position, avoidMovePosition, _speed * Time.deltaTime);
        
        Debug.Log(
            $"Avoid enemy trying to move away to pos: {_avoidMovePosition.ToString()} from {transform.position.ToString()}");
        // if (transform.position == _avoidMovePosition)
        // {
        //     _avoidingPlayerShot = false;
        // }
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
        
        if (_movementCurrentSlerpTime > _movementSlerpTime)
        {
            Debug.Log("Believe we've finished rotating.");
            // Just to be safe, set the rotation back to the original to make sure we start fresh for next time.
            transform.rotation = _origRotation;
            _movementCurrentSlerpTime = 0.0f;
            return;
        }
        // Can't use Quaternion.Slerp since by it takes the shortest path to the angle. E.g A rotation of 300 degrees
        // will instead rotate -60 degrees instead of nearly a full rotation. Also I think the fact that the same Euler
        // angle can be represented in multiple ways has some influence here. So instead we set the angle interpolated
        // over the time desired (_movementSlerpTime) per frame -- essentially doing it in little steps instead as a
        // workaround.
        // Adapted from
        // https://forum.unity.com/threads/by-pass-the-shortest-route-aspect-of-quaternion-slerp.459429/#post-2982421
        transform.rotation = _origRotation * Quaternion.AngleAxis(
            360 * _movementCurrentSlerpTime / _movementSlerpTime, Vector3.up);  // Vector3.up in 2D is y.
        _movementCurrentSlerpTime += Time.deltaTime;
    }
    
    // private IEnumerator AnimateAroundAxis(Transform trans, Vector3 axis, float changeInAngle, float duration)
    // {
    //     var start = trans.rotation;
    //     float t = 0f;
    //     while(t < duration)
    //     {
    //         trans.rotation = start * Quaternion.AngleAxis(changeInAngle * t / duration, axis);
    //         yield return null;
    //         t += Time.deltaTime;
    //     }
    //     trans.rotation = start * Quaternion.AngleAxis(changeInAngle, axis);
    // }

}

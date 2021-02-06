using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This Enemy avoids shots that are within _avoidDistanceTrigger and that are actually travelling towards it. Otherwise
// it behaves like a normal enemy.
public class AvoidShotEnemy : Enemy
{
    [SerializeField] 
    private float _avoidDistanceTrigger = 5f;

    [SerializeField] 
    private GameObject _deathPrefab;

    private Vector3 _avoidMovePosition ;
    private Vector3 _avoidMoveDistance = new Vector3(3, 0, 0);
    private bool _avoidAnimRunning;
    private float _movementCurrentSlerpTime;
    private float _animSlerpTime;
    private Quaternion _origRotation;

    [SerializeField] 
    private float _avoidCooldownWaitTime = 3f;
    private bool _avoidActionsAndCooldownComplete = true;
    private bool _tryAvoidPlayerShot;
    private bool _avoidMovementComplete = true;
    private bool _avoidAnimComplete = true;
    
    private WeaponManager _weaponManager;
    
    protected override void Start()
    {
        base.Start();
        _animSlerpTime = _movementLerpTime = 1.0f;  // Avoid movement and anim should complete at the same time.
        _origRotation = this.transform.rotation;
        _weaponManager = GameObject.Find("Weapon_Manager").GetComponent<WeaponManager>();
        // -1 == left, 1 == right
        float avoidDirection = UnityEngine.Random.value > 0.5 ? -1f : 1f;
        _avoidMoveDistance = new Vector3(3 * avoidDirection, 0, 0);
        _aggroTowardsPlayer = false;
        
        if (_weaponManager == null) {
            Debug.LogError("Weapons manager is null when creating player avoid shot enemy.");
        }
    }

    protected override EnemyMovementType ChooseMovementType()
    {
        return EnemyMovementType.StraightDown;
    }

    protected override void CalculateMovement()
    {
        List<Transform> playerShots = _weaponManager.GetPlayerShots();
        Transform playerShotInAvoidRange = ClosestPlayerShotInAvoidRange(playerShots);

        // TODO(Improvement): I feel like the string of &&s here is me missing something that would make this more
        // elegant.
        if (playerShotInAvoidRange && PlayerShotOnCollisionCourse(playerShotInAvoidRange) &&
            _avoidActionsAndCooldownComplete)
        {
            _tryAvoidPlayerShot = true;
            _avoidActionsAndCooldownComplete = false;
        }

        // This loop is separate from above to ensure we don't tie the movement and anim to whether a shot is in range.
        if (_tryAvoidPlayerShot && !_avoidActionsAndCooldownComplete)
        {
            _tryAvoidPlayerShot = false;  // Prevent us from running this loop again until the coroutine finishes.
            StartCoroutine(AvoidPlayerShotMovementAnimAndCooldownRoutine());
        }
        
        StraightDownMovement();
    }

    private Transform ClosestPlayerShotInAvoidRange(List<Transform> playerShots)
    {
        // If I return null, will and if check later return false for it? Is that best practice?
        Transform closestPlayerShot = null;
        float closestPlayerShotDistance = Mathf.Infinity;
        foreach (Transform playerShot in playerShots)
        {
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
        Physics2D.Raycast(
            playerShot.position, playerShot.up, contactFilter:contactFilter, results:hits);
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.transform == this.transform)
            {
                
                // Debug.DrawLine(playerShot.position, transform.position, Color.yellow, duration:3f);
                return true;
            }
        }
        return false;
    }

    private IEnumerator AvoidPlayerShotMovementAnimAndCooldownRoutine()
    {
        StartCoroutine(AvoidPlayerShotRoutine());
        StartCoroutine(AvoidPlayerShotAnimRoutine());
        // TODO(Improvement): I feel like this is an abuse of coroutines for what I'm doing...
        yield return new WaitUntil(() => _avoidMovementComplete && _avoidAnimComplete);
        yield return new WaitForSeconds(_avoidCooldownWaitTime);
        _avoidActionsAndCooldownComplete = true;
        yield return null;
    }
    
    // Customize lerp to move quickly at first, then slow down towards end -- like a "dash".
    // https://easings.net/#easeOutCirc
    private float EaseOutCircT(float t)
    {
        return Mathf.Sqrt(1.0f - Mathf.Pow(t - 1.0f, 2.0f));
    }

    private IEnumerator AvoidPlayerShotRoutine()
    {
        float movementCurrentLerpTime = 0f;
        Vector3 startPosition = transform.position;
        Vector3 avoidMovePosition = startPosition + _avoidMoveDistance;
        while (movementCurrentLerpTime < _movementLerpTime)
        {
            AvoidPlayerShot(startPosition, avoidMovePosition, ref movementCurrentLerpTime);
            yield return null;  // Wait a frame before continuing. 
        }

        _avoidMovementComplete = true;
        yield return null;
    }

    // TODO(Improvement): Multiple shots could be coming at this enemy, an increased difficulty setting could enable
    // this avoid to also try to avoid multiple nearby shots too.
    // Jump the enemy to the left/right but out of the path of player shot. 
    private void AvoidPlayerShot(Vector3 startPosition, Vector3 avoidMovePosition, ref float movementCurrentLerpTime)
    {
        movementCurrentLerpTime += Time.deltaTime;
        // Movement completed.
        if (movementCurrentLerpTime > _movementLerpTime)
        {
            return;
        }

        float interpValue = movementCurrentLerpTime / _movementLerpTime;
        interpValue = EaseOutCircT(interpValue);
        transform.position = Vector3.Lerp(startPosition, avoidMovePosition, interpValue);
    }
    
    private IEnumerator AvoidPlayerShotAnimRoutine()
    {
        float animCurrentSlerpTime = 0f;
        while (animCurrentSlerpTime < _animSlerpTime)
        { 
            AvoidPlayerShotsAnim(ref animCurrentSlerpTime);
            yield return null;  // Wait a frame before continuing. 
        }
        _avoidAnimComplete = true;
        yield return null;
    }

    private void AvoidPlayerShotsAnim(ref float animCurrentSlerpTime)
    {
        animCurrentSlerpTime += Time.deltaTime;
        
        // Anim completed.
        if (animCurrentSlerpTime > _animSlerpTime)
        {
            // Just to be safe, set the rotation back to the original to make sure we start fresh for next time.
            transform.rotation = _origRotation;
            return;
        }
        // Can't use Quaternion.Slerp since by it takes the shortest path to the angle. E.g A rotation of 300 degrees
        // will instead rotate -60 degrees instead of 300 degrees. Also I think the fact that the same Euler angle can
        // be represented in multiple ways has some influence here. So instead we set the angle interpolated over the
        // time desired (_animSlerpTime) per frame -- essentially doing it in little steps instead as a workaround.
        // Adapted from
        // https://forum.unity.com/threads/by-pass-the-shortest-route-aspect-of-quaternion-slerp.459429/#post-2982421
        transform.rotation = _origRotation * Quaternion.AngleAxis(
            360 * animCurrentSlerpTime / _animSlerpTime, Vector3.up);  // Vector3.up in 2D is y.
    }

    public override void DestroyEnemy()
    {
        base.WasDefeated();
        Instantiate(_deathPrefab, transform.position, Quaternion.identity);
        if (_audioSource.enabled)
        {
            _audioSource.Play();
        }
        Destroy(GetComponent<PolygonCollider2D>());
        // TODO(Improvement): This causes a bit of a delay in when the enemy sprite disappears and when the explosion
        // starts. Destroying the SpriteRenderer with a delay instead of SetActive(false) causes player to get damaged
        // if they go into explosion after shooting them so that doesn't work as-is. Maybe it's because I need to
        // destroy CircleCollider rather than PolygonCollider2D?
        this.gameObject.SetActive(false);
        base.RemoveEnemyFromGame(2.8f);
    }
}

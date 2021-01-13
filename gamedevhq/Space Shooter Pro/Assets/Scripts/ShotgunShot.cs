using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotgunShot : MonoBehaviour
{
    [SerializeField]
    private float _speed = 4.0f;
    
    [SerializeField]
    private GameObject shotPrefab;

    private GameObject leftShot;
    private GameObject rightShot;
    private GameObject frontShot;

    private List<GameObject> shotsRemaining;
    private Dictionary<GameObject, Vector3> shotsToDirection;

    private void Start()
    {
        if (shotPrefab == null)
        {
            Debug.LogError("ShotgunShot shot prefab appears to not be assigned.");
        }
        // TODO(Improvement): To allow for flexibility and simpler code, allow for number of shots
        // to be specified. Then divide the 180 deg in front of player by shot number and send the
        // shots along those paths. Could then instantiate them in a loop.
        leftShot = Instantiate(
            shotPrefab,
            transform.position, Quaternion.identity);
        rightShot = Instantiate(
            shotPrefab,
            transform.position, Quaternion.identity);
        frontShot = Instantiate(
            shotPrefab,
            transform.position, Quaternion.identity);
        shotsRemaining = new List<GameObject>() {leftShot, rightShot, frontShot};
        shotsToDirection = new  Dictionary<GameObject, Vector3>()
       {
           {leftShot, new Vector3(-1, 1, 0)},   // Move diagonal upwards to the left. 
           {rightShot, new Vector3(1, 1, 0)},   // Move diagonal upwards to the right. 
           {frontShot, Vector3.up},
       };
       if (shotsRemaining == null || shotsRemaining.Count == 0)
       {
           Debug.LogError("ShotgunShot couldn't find any child shot objects.");
       }
    }

    private void Update()
    {
        CalculateMovementAndOrDestroy();
    }

    private void CalculateMovementAndOrDestroy()
    {
        // Can't be foreach since we remove shotsRemaining in MoveShot if they go out of game bounds.
        for (int i = 0; i < shotsRemaining.Count; i++)
        {
            MoveShot(shotsRemaining[i], shotsToDirection[shotsRemaining[i]]);
        }
        
        if (shotsRemaining.Count == 0)
        {
            Destroy(this.gameObject);
        }
    }

    private void MoveShot(GameObject shot, Vector3 direction)
    {
        // Enemy object can destroy shot so this avoid a ref exception trying to move
        // a destroyed object.
        // TODO(Improvement): Could I restructure to avoid this null check on every update
        // loop? Maybe add as child objects instead?
        if (shot == null)
        {
            shotsRemaining.Remove(shot);
            return;
        }
        
        shot.transform.Translate(direction * (_speed * Time.deltaTime));

        if (shot.transform.position.y > 8f || shot.transform.position.y < -8f)
        {
            Destroy(shot);
            shotsRemaining.Remove(shot);
        }
    }

    public List<GameObject> GetShotgunShots()
    {
        List<GameObject> _existentShotgunShots = new List<GameObject>();
        // TODO: Test what happens in calling code when one of the shotguns shots is destroyed, is it really not
        // sent to caller?
        if (leftShot)
        {
            _existentShotgunShots.Add(leftShot);
        }
        if (frontShot)
        {
            _existentShotgunShots.Add(frontShot);
        }
        if (rightShot)
        {
            _existentShotgunShots.Add(rightShot);
        }

        return _existentShotgunShots;
    }
}

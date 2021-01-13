using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class AvoidShotEnemy : Enemy
{
    [SerializeField] 
    private float _avoidDistanceTrigger = 5f;
    
    protected override void CalculateMovement()
    {
        List<Transform> playerShots = base._player.GetProjectileTransforms();
        if (PlayerShotInAvoidRange(playerShots))
        {
            // TODO: Add if check if cooldown has passed for moving. Then reset cooldown if it has passed. We can use
            // player fire logic, but it'll only be subtracted from if we're not actively trying to move away from a
            // shot already.
            AvoidPlayerShots(playerShots);
        }
    }

    private bool PlayerShotInAvoidRange(List<Transform> playerShots)
    {
        // If I return null, will and if check later return false for it? Is that best practice?
        foreach (Transform playerShot in playerShots)
        {
            if (Vector3.Distance(playerShot.position, this.transform.position) <= _avoidDistanceTrigger)
            {
                return true;
            }
        }
        return false;
    }

    // Multiple shots could be coming at this enemy so we try to intelligently have it avoid one or multiple by having
    // it guess where they are going and moving out of their paths.
    private void AvoidPlayerShots(List<Transform> playerShots)
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
}

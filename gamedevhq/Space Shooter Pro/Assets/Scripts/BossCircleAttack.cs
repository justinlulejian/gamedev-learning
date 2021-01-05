using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossCircleAttack : MonoBehaviour
{
    // TODO: The impl for this will involve some math and sin and cos.
    // First: Pick a radius value that should be enough to be outside the boss on X/Y axis
    // then pick a angle step. Every angle step between 0 and 360[exclusive] use cos(angle) to get the x
    // coord, sin(angle) to get the y coord. angle is then the angle step value, but it might need to be
    // translated since it appears facing down = 0 and upside down is 180.
    // Second: once you have all three values for item, spawn as shotgun shot bullet as as x,y, z(angle).
    // Third: once they're spawned confirm that 1) they'll visually at the right angle steps and 2) that
    // the green direction arrow is either pointing along the angle we wanted or the opposite since either
    // will work and we can just adjust accordingly.
    // Fourth: once that's done, calc their movement to move "forward" along that angle and confirm they
    // do.
    // Fifth: wire up damage to player and that they'll destroy if they go out of bounds.
    // Sixth: decide if you want to make this fire multiple times, or
    // Seventh: check to destroy the parent object if all child objects are destroyed (hit player, or went off map)
    // Eighth: do audio for this, but not per shot, just once in this attack
    // Ninth: finish laser attack audio.
    // Bonus: make shotgun shots rotate around the circle before they shoot or simpler just have them breath
    // smaller and larger.


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

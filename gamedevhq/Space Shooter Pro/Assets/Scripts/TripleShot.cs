using System;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using UnityEngine;

public class TripleShot : PlayerProjectile
{
    public List<Laser> GetChildLasers()
    {
        // TODO(Improvement): This is somewhat inefficient since it can be called via Update from Player, however I
        // I can't move this to Start since this Start get's called after the Player's Update loop has finished because
        // we instantiate the tripleshot and then immediately call this method. Which doesn't allow time for Start here
        // to run.
        List<Laser> childLasers = this.gameObject.GetComponentsInChildren<Laser>().ToList();
        
        // TODO: Test what happens in calling code when one of the lasers is destroyed, is it really not
        // sent to caller?
        List<Laser> _existentChildLasers = new List<Laser>();
        foreach (Laser laser in childLasers)
        {
            if (laser)
            {
                _existentChildLasers.Add(laser);
            }
        }
        return _existentChildLasers;
    }
}

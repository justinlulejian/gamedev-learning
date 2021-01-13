using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TripleShot : MonoBehaviour
{
    private List<GameObject> _childLasers = new List<GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        _childLasers = this.gameObject.GetComponentsInChildren<GameObject>().ToList();
    }

    public List<GameObject> GetChildLasers()
    {
        // TODO: Test what happens in calling code when one of the lasers is destroyed, is it really not
        // sent to caller?
        List<GameObject> _existentChildLasers = new List<GameObject>();
        foreach (GameObject laser in _childLasers)
        {
            if (laser)
            {
                _existentChildLasers.Add(laser);
            }
        }
        return _existentChildLasers;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class Player : MonoBehaviour
{


  // Start is called before the first frame update
  void Start()
  {

  }

  // Update is called once per frame
  void Update()
  {
    points = 0;

    switch(points)
    {
      case 50:
        Debug.Log("Points 50!");
        break;
      case 100:
        Debug.Log("Points 1001");
        break;
      default:
        Debug.Log("You need 50 or 100!");
        break;
    }

  }
}

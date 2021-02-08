using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class Player : MonoBehaviour
{

    public static event Action onTurnRed;

  // Start is called before the first frame update
  void Start()
  {
  }

  // Update is called once per frame
  void Update()
  {
      if (Input.GetKeyDown(KeyCode.Space))
      {
          // raise event that space key pressed
          onTurnRed?.Invoke();
      }
  }
}

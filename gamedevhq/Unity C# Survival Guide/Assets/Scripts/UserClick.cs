using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UserClick : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown((int)MouseButton.LeftMouse))
        {
            Ray rayOrigin = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit rayCastHitInfo = new RaycastHit();
            if (Physics.Raycast(rayOrigin, out rayCastHitInfo))
            {
                if (rayCastHitInfo.collider.tag == "Cube")
                {
                    ICommand click = new ClickCommand(rayCastHitInfo.collider.gameObject, UnityEngine.Random.ColorHSV());
                    click.Execute();
                    CommandManager.Instance.AddCommand(click);
                }
            }
        }
    }
}

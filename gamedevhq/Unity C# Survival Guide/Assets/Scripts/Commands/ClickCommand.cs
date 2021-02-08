using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickCommand: ICommand
{
    public GameObject _cube;
    private Color _color;
    private MeshRenderer _cubeMeshRenderer;
    private Color _previousColor;
    
    public ClickCommand(GameObject cube, Color color)
    {
        _cube = cube;
        _cubeMeshRenderer = _cube.GetComponent<MeshRenderer>();
        _color = color;
    }
     
    public void Execute()
    {
        // change color of cube to random.
        _previousColor = _cubeMeshRenderer.material.color;
        _cubeMeshRenderer.material.color = _color;
    }

    public void Undo()
    {
        _cubeMeshRenderer.material.color = _previousColor;
    }
}

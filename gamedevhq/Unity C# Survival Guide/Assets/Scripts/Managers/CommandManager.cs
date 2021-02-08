using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CommandManager : MonoSingleton<CommandManager>
{
    private List<ICommand> _commandBuffer = new List<ICommand>();
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddCommand(ICommand command)
    {
        Debug.Log($"Adding command to buffer");
        _commandBuffer.Add(command);
    }

    public void PlayCommands()
    {
        StartCoroutine(PlayCommandsRoutine());
    }

    private IEnumerator PlayCommandsRoutine()
    {
        Debug.Log($"Re-executing all commands in order.");
        foreach (var command in _commandBuffer)
        {
            command.Execute();
            yield return new WaitForSeconds(1f);
        }
    }
    
    public void RewindCommands()
    {
        StartCoroutine(RewindCommandsRoutine());
    }
    
    private IEnumerator RewindCommandsRoutine()
    {
        Debug.Log($"command buffer has {_commandBuffer.Count.ToString()} items.");
        for (int i = _commandBuffer.Count - 1; i > -1; i--)
        {
            Debug.Log($"Undoing action at index: {i.ToString()}");
            _commandBuffer.ElementAt(i).Undo();
            yield return new WaitForSeconds(1f);
        }
    }

    public void DoneCommands()
    {
        var cubes = GameObject.FindGameObjectsWithTag("Cube");
        foreach (var cube in cubes)
        {
            cube.GetComponent<MeshRenderer>().material.color = Color.white;
        }
    }

    public void ResetCommands()
    {
        _commandBuffer.Clear();
    }
    
    
    
}

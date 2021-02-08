using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Main : MonoBehaviour
{
    public static Action onCompleteRoutine;

    private string[] names = new string[] { "jon", "julie", "justin"};
    private int[] quizGrades = new int[] { 10,11,99,30,68,69,73,44,55,55,66,77,98,95};
    
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(MyRoutine(onCompleteRoutine));

        var passingGrades = quizGrades.Where(g => g > 69).OrderByDescending(g => g).Reverse();
        var averageGrade = quizGrades.Average();
        var averagePassingGrade = quizGrades.Where(g => g > 69).Average();
        foreach (var grade in passingGrades)
        {
            Debug.Log($"passing grade: {grade.ToString()}");
        }
        Debug.Log($"average grade: {averageGrade.ToString()}");
        Debug.Log($"average passing grade: {averagePassingGrade.ToString()}");

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public IEnumerator MyRoutine(Action onComplete)
    {
        Debug.Log("This is main routine waiting for 3 seconds.");
        yield return new WaitForSeconds(3.0f);
        if (onComplete != null)
        {
            onComplete();
        }
    }
}

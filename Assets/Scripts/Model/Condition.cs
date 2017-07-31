using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

[System.Serializable]
public class Condition
{
    public List<Param> parameters = new List<Param>();
    public string conditionString = "";
    public bool ConditionValue
    {
        get
        {
            ExpressionSolver.CalculateBool(conditionString, parameters);
            return true;
        }
    }

}




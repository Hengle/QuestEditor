using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;


public class Path
{
	public string text = "";
	public bool auto = false;
	public List<Condition> conditions = new List<Condition>();
    public List<ParamChanges> changes = new List<ParamChanges>();
	public State aimState;

    public Path(State aimState)
    {
        this.aimState = aimState;
    }

    public Path()
    {
    }
}



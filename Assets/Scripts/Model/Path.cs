using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;


public class Path
{
	public string text;
	public bool auto = false;
	public Condition condition;
	public State aimState;

    public Path(State aimState)
    {
        this.aimState = aimState;
    }

    public Path()
    {
    }
}



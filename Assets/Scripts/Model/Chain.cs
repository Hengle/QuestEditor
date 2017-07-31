using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

[System.Serializable]
public class Chain
{
	public string name;
	private State startState;
    public State StartState
    {
        get
        {
            return states[0];
        }
        set
        {
            states.Remove(value);
            states.Insert(0, value);
        }
    }
	public List<State> states = new List<State>();

	public Chain(int guid)
	{
		name = "New chain";
		startState = new State (guid);
		states.Add (startState);
	}
}

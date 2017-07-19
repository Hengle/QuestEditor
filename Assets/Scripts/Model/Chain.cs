using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

[System.Serializable]
public class Chain
{
	public string name;
	public State startState;
	public List<State> states = new List<State>();

	public Chain()
	{
		name = "New chain";
		startState = new State ();
		states.Add (startState);
	}
}

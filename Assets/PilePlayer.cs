using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PilePlayer : MonoBehaviour {

	private Queue<Chain> chains = new Queue<Chain>();

	public void Add(Chain ch)
	{
		chains.Enqueue (ch);
	}

	public void Add(List<Chain> ch)
	{
		foreach(Chain c in ch)
		{
			chains.Enqueue (c);
		}
	}

	public Chain GetChain()
	{
		return chains.Dequeue ();
	}
}

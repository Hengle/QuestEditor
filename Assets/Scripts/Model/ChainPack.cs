using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ChainPack {

	[SerializeField]
	public List<Chain> chains = new List<Chain>();

	[SerializeField]
	public string name;
    public string tags;

    public ChainPack()
	{
		name = "New pack";
        tags = "";
	}
}

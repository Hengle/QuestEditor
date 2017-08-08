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
	public int ChainPackGUID;

	public ChainPack(int guid)
	{
		name = "New pack";
        tags = "";
		ChainPackGUID = guid;
	}

	public ChainPack()
	{
	}
}

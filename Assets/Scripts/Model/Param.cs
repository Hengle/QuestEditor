using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Param
{
	public string name;
	public bool showing;
	public string tags;
	public string description;
	public Sprite image;
	public Chain usableChain;
	public bool activating;
	public bool manualActivationWithConditions = false;
	public Condition manualUsingCondition = new Condition();
	public Dictionary<Condition, Chain> autoActivatedChains = new Dictionary<Condition, Chain>();
	public Vector2 scrollPosition;

	public Param()
	{
		name = "new param";
		showing = false;
		description = "";
		activating = false;
		tags = "";
	}
}
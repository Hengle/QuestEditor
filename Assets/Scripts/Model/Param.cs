using UnityEngine;

[System.Serializable]
public class Param
{
	public string name;
	public bool showing;
	public string description;
	public Sprite image;
	public Chain usableChain;
	public bool activating;
	public int ucid;

	public Param()
	{
		name = "new param";
		showing = false;
		description = "";
		activating = false;
	}
}
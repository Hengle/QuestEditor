using UnityEngine;
using System.Collections.Generic;
using System;

[System.Serializable]
public class Param
{
    public delegate void ParamActivation(Chain chain);
    public event ParamActivation OnParamActivation;

   
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
    public float pValue;
    public float PValue
    {
        get
        {
            return pValue;
        }
        set
        {
            if (pValue != value)
            {
                pValue = value;
                CheckConditions();
            }
            else
            {
                pValue = value;
            }
        }
    }

    private void CheckConditions()
    {
        foreach (KeyValuePair<Condition, Chain> pair in autoActivatedChains)
        {
            if (pair.Key.ConditionValue)
            {
                OnParamActivation(pair.Value);
            }
        }
    }

    public Param()
	{
		name = "new param";
		showing = false;
		description = "";
		activating = false;
		tags = "";
	}
}
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
	private int usableChainGuid;
	public Chain usableChain
	{
		get
		{
			return GUIDManager.GetChainByGuid (usableChainGuid);
		}
		set
		{
			usableChainGuid = value.ChainGuid;
		}
	}
	public bool activating;
	public bool manualActivationWithConditions = false;
	public Condition manualUsingCondition = new Condition();
	public Dictionary<Condition, Chain> autoActivatedChains
	{
		get
		{
			Dictionary<Condition, Chain> ret = new Dictionary<Condition, Chain>();
			foreach(KeyValuePair<Condition, int> kvp in autoActivatedChainsGUIDS)
			{
				ret.Add (kvp.Key, GUIDManager.GetChainByGuid(kvp.Value));
			}
			return ret;
		}
		set
		{
			autoActivatedChainsGUIDS = new Dictionary<Condition, int> ();
			foreach(KeyValuePair<Condition, Chain> kvp in value)
			{
				autoActivatedChainsGUIDS.Add (kvp.Key, kvp.Value.ChainGuid);
			}
		}
	}

	public void RemoveAutoActivatedChain(Condition cond)
	{
		autoActivatedChainsGUIDS.Remove (cond);
	}
	public void AddAutoActivatedChain(Condition cond, Chain c)
	{
		autoActivatedChainsGUIDS.Add (cond, c.ChainGuid);
	}

	private Dictionary<Condition, int> autoActivatedChainsGUIDS = new Dictionary<Condition, int> ();
	//use guid
	public Vector2 scrollPosition;
	public int paramGUID;
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

	public Param(int Guid)
	{
		this.paramGUID = Guid;
		name = "new param";
		showing = false;
		description = "";
		activating = false;
		tags = "";
	}
}
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

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
	public int usableChainGuid;
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
			foreach(ConditionChain kc in autoActivatedChainsGUIDS)
			{
				ret.Add (kc.c, GUIDManager.GetChainByGuid(kc.guid));
			}
			return ret;
		}
		set
		{
			autoActivatedChainsGUIDS = new List<ConditionChain>();
            foreach (KeyValuePair<Condition, Chain> kvp in value)
			{
				autoActivatedChainsGUIDS.Add (new ConditionChain(kvp.Key, kvp.Value.ChainGuid));
			}
		}
	}

    public void SetAutoActivatedChain(int id, Chain c)
    {
        autoActivatedChainsGUIDS[id].guid = c.ChainGuid;
    }

	public void RemoveAutoActivatedChain(int id)
	{
		autoActivatedChainsGUIDS.RemoveAt (id);
	}
	public void AddAutoActivatedChain(Condition cond, Chain c)
	{
        Debug.Log(c.ChainGuid);
		autoActivatedChainsGUIDS.Add (new ConditionChain(cond, c.ChainGuid));
	}

    public List<ConditionChain> autoActivatedChainsGUIDS = new List<ConditionChain>();
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
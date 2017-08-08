using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ResourceManager : MonoBehaviour {

    public delegate void ParamChanging(Param p);
    public event ParamChanging OnParamChanging;

    public PathGame pathGame;
    private List<Param> parameters = new List<Param>();
    private ChainPlayer chainPlayer;
    private ChainPlayer ChainPlayer
    {
        get
        {
            if (chainPlayer == null)
            {
                chainPlayer = FindObjectOfType<ChainPlayer>();
            }
            return chainPlayer;
        }
    }

	public void SetParam(string name, float value, bool check = true)
    {
		if(check){
			parameters.First(x => x.name == name).PValue = value;
		}
		else{
			parameters.First(x => x.name == name).pValue = value;
		}
		foreach(ConditionChange pch in parameters.First(x => x.name == name).autoActivatedChangesGUIDS)
		{
			if(ExpressionSolver.CalculateBool(pch.condition.conditionString, pch.condition.Parameters))
			{
				foreach(ParamChanges pcha in pch.changes)
				{
					SetParam (pcha.aimParam.name, pcha.changeString, pcha.parameters, false);	
				}
			}
		}
		OnParamChanging.Invoke(parameters.First(x => x.name == name));
    }

    public void CheckParam(string name)
    {
        OnParamChanging.Invoke(parameters.First(x => x.name == name));
    }

    public void Init(PathGame pathGame)
    {
        this.pathGame = pathGame;
            parameters = pathGame.parameters;
            foreach (Param p in parameters)
            {
                p.pValue = 0;
                if (ChainPlayer)
                {
				p.OnParamActivation += ChainPlayer.ActivateStateFromParam;
                }
                OnParamChanging.Invoke(p);
            }
    }
		

	public void SetParam(string name, string evaluationString, List<Param> evaluationParameters = null, bool check = true)
    {
        float value = 0;
        value = ExpressionSolver.CalculateFloat(evaluationString, evaluationParameters);
		SetParam (name, value, check);
    }

    public float GetParam(string name)
    {
        return parameters.First(x => x.name == name).PValue;
    }

    private void OnDestroy()
    {
        foreach (Param p in parameters)
        {
            if (ChainPlayer)
            {
				p.OnParamActivation -= ChainPlayer.ActivateStateFromParam;
            }
        }
    }
}

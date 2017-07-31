using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ResourceManager : MonoBehaviour {

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

    public void SetParam(string name, float value)
    {
        parameters.First(x => x.name == name).PValue = value;
    }

    public void SetParam(string name, string evaluationString, List<Param> evaluationParameters = null)
    {
        float value = 0;
        //value = evaluate(evaluationString, evaluationParameters);
        parameters.First(x => x.name == name).PValue = value;
    }

    public float GetParam(string name)
    {
        return parameters.First(x => x.name == name).PValue;
    }

    private void Awake()
    {
        if (pathGame)
        {
            parameters = pathGame.parameters;
            foreach (Param p in parameters)
            {
                p.pValue = 0;
                if (ChainPlayer)
                {
                    p.OnParamActivation += ChainPlayer.PlayChain;
                }
            }
            GUIDManager.SetInspectedGame(pathGame);
        }
    }

    private void OnDestroy()
    {
        foreach (Param p in parameters)
        {
            if (ChainPlayer)
            {
                p.OnParamActivation -= ChainPlayer.PlayChain;
            }
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChainPlayer : MonoBehaviour
{
    public delegate void StateActivation(State state);
    public  event StateActivation OnStateActivation;
    private State waitingState = null;
    private State currentState;

    public void PlayChain(Chain chain)
    {
        ActivateState(chain.StartState);
    }

    public void GoByPath(Path p)
    {
        foreach (ParamChanges ch in p.changes)
        {
            FindObjectOfType<ResourceManager>().SetParam(ch.aimParam.name, ch.changeString, ch.parameters);
        }
        ActivateState(p.aimState);
    }

    private void ActivateState(State startState)
    {
        currentState = startState;
        foreach(Path c in startState.pathes)
        {
            if (c.auto)
            {
                if (c.condition.ConditionValue)
                {
                    if (c.waitInput)
                    {
                        waitingState = c.aimState;
                    }
                    else
                    {
                        ActivateState(c.aimState);
                    }
                    return;
                }
            }
        }
        if (startState.pathes.Count == 0)
        {
            waitingState = FindObjectOfType<PilePlayer>().GetChain().StartState;
        }

        if (OnStateActivation!=null)
        {
            OnStateActivation(startState);
        }
    }

    public void ActivateWaitingInputState()
    {
        if (waitingState!=null)
        {
            ActivateState(waitingState);
            waitingState = null;
        }
    }
}

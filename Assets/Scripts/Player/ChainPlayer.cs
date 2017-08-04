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

	private Stack<State> brokenStates = new Stack<State>();

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

        foreach (ParamChanges ch in p.changes)
        {
            ch.aimParam.PValue = ch.aimParam.PValue;
        }
    }

	public void ActivateStateFromParam(State startState)
	{
		if(GUIDManager.GetChainByStateGuid(startState.stateGUID).returnAfterEnd)
		{
			Debug.Log ("push "+currentState.description);
			brokenStates.Push (currentState);
		}
		ActivateState (startState);
	}

	public void ActivateState(State startState)
    {

        currentState = startState;
        foreach(Path c in startState.pathes)
        {
            if (c.auto)
            {
                Debug.Log("___");
                Debug.Log(c.condition.conditionString);
                foreach (Param p in c.condition.Parameters)
                {
                    Debug.Log(p.PValue);
                }

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
			if (brokenStates.Count > 0) {
				State s = brokenStates.Pop ();
				waitingState = s;
			} else {
				waitingState = FindObjectOfType<PilePlayer> ().GetChain ().StartState;
			}
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

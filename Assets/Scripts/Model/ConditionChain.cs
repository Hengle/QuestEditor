using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ConditionChain {
    public Condition c;
    public int guid;

    public ConditionChain(Condition key, int chainGuid)
    {
        this.c = key;
        this.guid = chainGuid;
    }
}

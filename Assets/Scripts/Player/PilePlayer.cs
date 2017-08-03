using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PilePlayer : MonoBehaviour
{

    private Queue<Chain> chains = new Queue<Chain>();
    private List<Chain> droped = new List<Chain>();

    public bool shuffleAfterEnding;

    public void Shuffle()
    {
        chains = new Queue<Chain>(chains.ToList().OrderBy(a => Guid.NewGuid()).ToList());
    }

    public void Init(bool shufflePileAfterEnd)
    {
        shuffleAfterEnding = shufflePileAfterEnd;
    }

    public void Add(ChainPack pack)
    {
        Add(pack.chains);
    }

    public void Add(Chain ch)
    {
        chains.Enqueue(ch);
    }

    public void Add(List<Chain> ch)
    {
        foreach (Chain c in ch)
        {
            chains.Enqueue(c);
        }
    }

    public Chain GetChain()
    {
        if (chains.Count == 0)
        {
            if (shuffleAfterEnding)
            {
                chains = new Queue<Chain>(droped);
                Shuffle();
            }
            else
            {
                return null;
            }
        }
        Chain c = chains.Dequeue();
        droped.Add(c);
        return c;
    }
}
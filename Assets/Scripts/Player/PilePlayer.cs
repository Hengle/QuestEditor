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

	public void ShuffleDrop()
	{
		droped = new List<Chain>(droped.OrderBy(a => Guid.NewGuid()).ToList());
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

	public void ApplyChanger (PileChanger pileChanger)
	{
		switch(pileChanger.changeType)
		{
		case PileChanger.ChangeType.Shuffle:
			Shuffle ();
				break;
		case PileChanger.ChangeType.Remove:
			switch(pileChanger.aim){
			case PileChanger.PileChangeAim.Drop:
					if (!pileChanger.withRepeat) {
						List<Chain> addingChains = GUIDManager.getChainPackByGuid (pileChanger.chainPileGUID).chains;
						addingChains.RemoveAll (c => droped.FindAll (dc => dc.ChainGuid == c.ChainGuid).Contains(c));
						droped.RemoveAll (c=>addingChains.Contains(c));
						ShuffleDrop ();
					} 
					else 
					{
						droped.RemoveAll (c => droped.FindAll (dc => dc.ChainGuid == c.ChainGuid).Contains(c));
						ShuffleDrop ();
					}
					break;
				case PileChanger.PileChangeAim.Pile:
						if (!pileChanger.withRepeat) {
							List<Chain> addingChains = GUIDManager.getChainPackByGuid (pileChanger.chainPileGUID).chains;
					addingChains.RemoveAll (c => chains.ToList().FindAll (dc => dc.ChainGuid == c.ChainGuid).Contains(c));
					chains = new Queue<Chain>(chains.ToList().RemoveAll (c=>addingChains.Contains(c)));
							Shuffle();
						} 
						else 
						{
					chains = new Queue<Chain>(chains.ToList().RemoveAll (c => chains.ToList().FindAll (dc => dc.ChainGuid == c.ChainGuid).Contains(c)));
							ShuffleDrop ();
						}
					break;
				case PileChanger.PileChangeAim.Both:
						if (!pileChanger.withRepeat) {
							List<Chain> addingChains = GUIDManager.getChainPackByGuid (pileChanger.chainPileGUID).chains;
							addingChains.RemoveAll (c => droped.FindAll (dc => dc.ChainGuid == c.ChainGuid).Contains(c));
							addingChains.RemoveAll (c => chains.ToList().FindAll (dc => dc.ChainGuid == c.ChainGuid).Contains(c));
					droped.RemoveAll (c=>addingChains.Contains(c));
					chains = new Queue<Chain>(chains.ToList().RemoveAll (c=>addingChains.Contains(c)));
							Shuffle();
							ShuffleDrop();
						} 
						else 
						{
					chains = new Queue<Chain>(chains.ToList().RemoveAll (c => chains.ToList().FindAll (dc => dc.ChainGuid == c.ChainGuid).Contains(c)));
							ShuffleDrop ();
					droped.RemoveAll (c => droped.FindAll (dc => dc.ChainGuid == c.ChainGuid).Contains(c));
							ShuffleDrop ();
						}
					break;
			}
			break;
		case PileChanger.ChangeType.Add:
			switch(pileChanger.aim){
			case PileChanger.PileChangeAim.Drop:
				if (!pileChanger.withRepeat) {
					List<Chain> addingChains = GUIDManager.getChainPackByGuid (pileChanger.chainPileGUID).chains;
					addingChains.RemoveAll (c => droped.FindAll (dc => dc.ChainGuid == c.ChainGuid).Contains(c));
					droped.RemoveAll (c=>addingChains.Contains(c));
					ShuffleDrop ();
				} 
				else 
				{
					droped.RemoveAll (c => droped.FindAll (dc => dc.ChainGuid == c.ChainGuid).Contains(c));
					ShuffleDrop ();
				}
				break;
			case PileChanger.PileChangeAim.Pile:
				if (!pileChanger.withRepeat) {
					List<Chain> addingChains = GUIDManager.getChainPackByGuid (pileChanger.chainPileGUID).chains;
					addingChains.RemoveAll (c => chains.ToList().FindAll (dc => dc.ChainGuid == c.ChainGuid).Contains(c));
					chains = new Queue<Chain>(chains.ToList().RemoveAll (c=>addingChains.Contains(c)));
					Shuffle();
				} 
				else 
				{
					chains = new Queue<Chain>(chains.ToList().RemoveAll (c => chains.ToList().FindAll (dc => dc.ChainGuid == c.ChainGuid).Contains(c)));
					ShuffleDrop ();
				}
				break;
			case PileChanger.PileChangeAim.Both:
				if (!pileChanger.withRepeat) {
					List<Chain> addingChains = GUIDManager.getChainPackByGuid (pileChanger.chainPileGUID).chains;
					addingChains.RemoveAll (c => droped.FindAll (dc => dc.ChainGuid == c.ChainGuid).Contains(c));
					addingChains.RemoveAll (c => chains.ToList().FindAll (dc => dc.ChainGuid == c.ChainGuid).Contains(c));
					droped.RemoveAll (c=>addingChains.Contains(c));
					chains = new Queue<Chain>(chains.ToList().RemoveAll (c=>addingChains.Contains(c)));
					Shuffle();
					ShuffleDrop();
				} 
				else 
				{
					chains = new Queue<Chain>(chains.ToList().RemoveAll (c => chains.ToList().FindAll (dc => dc.ChainGuid == c.ChainGuid).Contains(c)));
					ShuffleDrop ();
					droped.RemoveAll (c => droped.FindAll (dc => dc.ChainGuid == c.ChainGuid).Contains(c));
					ShuffleDrop ();
				}
				break;
			}
			break;
		}
	}
}
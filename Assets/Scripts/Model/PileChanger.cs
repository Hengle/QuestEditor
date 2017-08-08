using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PileChanger{

	public int chainPileGUID;

	public enum ChangeType
	{
		Shuffle,
		Add,
		Remove
	}

	public ChangeType changeType;

	public enum PileChangeAim
	{
		Both,
		Pile,
		Drop
	}

	public PileChangeAim aim;

	public bool withRepeat = false;

	public PileChanger(ChainPack cp)
	{
		chainPileGUID = cp.ChainPackGUID;
	}

	public PileChanger()
	{
	}
}

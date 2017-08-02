using System;
using System.Collections.Generic;
using UnityEngine;

public class GUIDManager
{
    private static PathGame inspectedgame;

    public static void SetInspectedGame(PathGame g)
    {
        inspectedgame = g;
    }

   

    public static int GetStateGUID()
    {
        int r = UnityEngine.Random.Range(0,999999);
        if (inspectedgame)
        {
            foreach (ChainPack p in inspectedgame.chainPacks)
            {
                foreach (Chain c in p.chains)
                {
                    foreach (State s in c.states)
                    {
                        if (s.stateGUID == r)
                        {
                            return GetStateGUID();
                        }
                    }
                }
            }
        }
        return r;
    }

    public static State GetStateByGuid(int aimStateGuid)
    {
        foreach (ChainPack p in inspectedgame.chainPacks)
        {
            foreach (Chain c in p.chains)
            {
                foreach (State s in c.states)
                {
                    if (s.stateGUID == aimStateGuid)
                    {
                        return s;
                    }
                }
            }
        }
        return null;
    }

	public static int GetChainGUID()
	{
		int r = UnityEngine.Random.Range(0,999999);
		if (inspectedgame)
		{
			foreach (ChainPack p in inspectedgame.chainPacks)
			{
				foreach (Chain c in p.chains)
				{

					if (c.ChainGuid == r)
						{
						return GetChainGUID();
						}
					}
				}
			}
		return r;
	}

	public static Chain GetChainByGuid(int aimChainGuid)
	{
		foreach (ChainPack p in inspectedgame.chainPacks)
		{
			foreach (Chain c in p.chains)
			{
				if (c.ChainGuid == aimChainGuid)
					{
						return c;
					}
			}
		}
		return null;
	}

	public static int GetItemGUID()
	{
		int r = UnityEngine.Random.Range(0,999999);
		if (inspectedgame)
		{
			foreach (Param p in inspectedgame.parameters)
			{
						if (p.paramGUID == r)
						{
							return GetItemGUID();
						}
			}
		}
		return r;
	}

	public static Param GetItemByGuid(int aimParamGuid)
	{
		foreach (Param p in inspectedgame.parameters)
		{
			if (p.paramGUID == aimParamGuid)
					{
						return p;
					}
		}
		return null;
	}
}
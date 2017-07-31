using System;
using System.Collections.Generic;

public class GUIDManager
{
    private static PathGame inspectedgame;

    public static void SetInspectedGame(PathGame g)
    {
        inspectedgame = g;
    }

    public static int GetGUID()
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
                            return GetGUID();
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
}
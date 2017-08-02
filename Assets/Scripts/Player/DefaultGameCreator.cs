using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultGameCreator : MonoBehaviour {

    public PathGame game;

	// Update is called once per frame
	void Start () {
        PilePlayer pp = FindObjectOfType<PilePlayer>();
        foreach (ChainPack cp in game.chainPacks)
        {
            pp.Add(cp);
        }
        pp.Shuffle();
        GUIDManager.SetInspectedGame(game);
        FindObjectOfType<ChainPlayer>().PlayChain(pp.GetChain());
        GUIDManager.SetInspectedGame(game);
    }
}

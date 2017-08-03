using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour {

    public QuestVisualizer visualizer;
    public PathGame pathGame;
    public bool launchOnStart;
    public bool initWithPacks;
    public bool shufflePileAfterEnd;
    public string[] packsTags;

    private void Start()
    {
        Play(pathGame);
    }

    private void Play(PathGame pathGame)
    {
        this.pathGame = pathGame;
        if (pathGame)
        {
            GUIDManager.SetInspectedGame(pathGame);
            PilePlayer pp = gameObject.AddComponent<PilePlayer>();
            pp.Init(shufflePileAfterEnd);
            ChainPlayer cp = gameObject.AddComponent<ChainPlayer>();
            ResourceManager rm = gameObject.AddComponent<ResourceManager>();
            if (visualizer)
            {
                visualizer.Init(rm);
            }

            rm.Init(pathGame);
            InputListener il = gameObject.AddComponent<InputListener>();

            if (initWithPacks)
            {
                foreach (ChainPack chainPack in pathGame.chainPacks)
                {
                    if (chainPack.tags == "")
                    {
                        pp.Add(chainPack);
                    }
                    else
                    {
                        foreach (string s in chainPack.tags.Split(','))
                        {
                            if (packsTags.Contains(s))
                            {
                                pp.Add(chainPack);
                            }
                        }
                    }
                }
                pp.Shuffle();
                GUIDManager.SetInspectedGame(pathGame);
                FindObjectOfType<ChainPlayer>().PlayChain(pp.GetChain());
                GUIDManager.SetInspectedGame(pathGame);
            }
        }
    }
}

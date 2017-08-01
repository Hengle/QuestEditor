using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemsVisualizer : MonoBehaviour {

    public ResourceManager res;
    public GameObject itemPrefab;
    public string[] showingTags;

    private void Awake()
    {
        res.OnParamChanging += ChangingParam;
    }

    private void ChangingParam(Param p)
    {


        if (p.tags.Length>0)
        {
            foreach (string s in p.tags.Split(','))
            {
                if (!showingTags.ToList().Contains(s))
                {
                    return;
                }
            }
        }

        if (!p.showing)
        {
            return;
        }

        foreach (ItemButton ib in GetComponentsInChildren<ItemButton>())
        {
            if (ib.parameter == p)
            {
                ib.UpdateValue();
                return;
            }
        }

        GameObject newButton = Instantiate(itemPrefab);
        newButton.transform.SetParent(transform);
        newButton.GetComponent<ItemButton>().Init(p);
    }
}

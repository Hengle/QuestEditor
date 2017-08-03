using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ItemsVisualizer : MonoBehaviour {

    private ResourceManager resourceManager;
    private ResourceManager res;

    public GameObject itemPrefab;
    public string[] showingTags;

    public void Init(ResourceManager res)
    {
        this.res = res;
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
        newButton.transform.localScale = Vector2.one;
        newButton.GetComponent<ItemButton>().Init(p);
        newButton.GetComponent<Button>().onClick.AddListener(()=> { GetComponentInParent<ItemsVisualizer>().HideMenu(); });
    }

    public void HideMenu()
    {
        GetComponentInParent<Animator>().SetTrigger("Open");
    }
}

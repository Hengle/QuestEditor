using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestVisualizer : MonoBehaviour {

    public Image stateImage;
    public Text stateText;
    public Transform variantsContent;
    public GameObject variantPrefab;

    public void Init(ResourceManager res)
    {
        FindObjectOfType<ChainPlayer>().OnStateActivation+=ActivateState;
        GetComponentInChildren<ItemsVisualizer>().Init(res);
    }

    private void ActivateState(State state)
    {
        foreach (Transform t in variantsContent)
        {
            Destroy(t.gameObject);
        }
        stateImage.sprite = state.image;
        stateText.text = state.description;
        foreach (Path p in state.pathes)
        {
            if (p.condition.ConditionValue)
            {
                GameObject b = Instantiate(variantPrefab);
                b.GetComponentInChildren<Text>().text = p.text;
                b.GetComponent<Button>().onClick.AddListener(delegate() 
                {
                    FindObjectOfType<ChainPlayer>().GoByPath(p);
                });
                b.transform.SetParent(variantsContent);
                b.transform.localScale = Vector3.one;
            }
        }
    }
}

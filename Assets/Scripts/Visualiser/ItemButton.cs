using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemButton : MonoBehaviour {

    public Param parameter;
    public Image img;
    public Text counter; 


    public void Init(Param parameter)
    {
        this.parameter = parameter;
        img.sprite = parameter.image;
        GetComponent<Button>().onClick.AddListener(()=>
        {
            FindObjectOfType<ChainPlayer>().PlayChain(parameter.usableChain);
        });
        UpdateValue();
    }

    public void UpdateValue()
    {
        counter.text = parameter.PValue.ToString();

        GetComponent<Button>().interactable = parameter.manualActivationWithConditions && ExpressionSolver.CalculateBool(parameter.manualUsingCondition.conditionString, parameter.manualUsingCondition.parameters);


        if (parameter.PValue <= 0)
        {
            Destroy(gameObject);
        }

        if (parameter.PValue == 1)
        {
            counter.enabled = false;
        }
        else
        {
            counter.enabled = true;
        }
    }

    void OnMouseEnter()
    {
        PopupInfo.ShowInfo(parameter.description, transform.position);
    }

    void OnMouseExit()
    {
        PopupInfo.HideInfo();
    }
}

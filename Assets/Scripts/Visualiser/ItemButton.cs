using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

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

        GetComponent<Button>().interactable = parameter.activating;

        if (parameter.manualActivationWithConditions && parameter.activating)
        {
            if (ExpressionSolver.CalculateBool(parameter.manualUsingCondition.conditionString, parameter.manualUsingCondition.Parameters))
            {
                GetComponent<Button>().interactable = true;
            }
            else
            {
                GetComponent<Button>().interactable = false;
            }
        }
       


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
    public void OnPointerExit(PointerEventData eventData)
    {
        PopupInfo.HideInfo();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        PopupInfo.ShowInfo(parameter.description, transform.position);
    }
}

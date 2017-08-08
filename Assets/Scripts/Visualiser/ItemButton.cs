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
				ActivateItem();
        });
        UpdateValue();
    }

	public void ActivateItem(){
		if(parameter.manualActivationWithState)
		{
			FindObjectOfType<ChainPlayer>().ActivateStateFromParam(parameter.usableState);
		}
		if(parameter.withChange)
		{
			foreach(ParamChanges pch in parameter.manualUsingChange)
			{
				FindObjectOfType<ResourceManager>().SetParam(pch.aimParam.name, pch.changeString, pch.parameters, false);
				GetComponentInParent<ItemsVisualizer> ().CheckButtons ();
			}
		}
	}

    public void UpdateValue()
    {
        counter.text = parameter.PValue.ToString();

        GetComponent<Button>().interactable = parameter.activating;

        if ((parameter.manualActivationWithState||parameter.withChange) && parameter.activating)
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

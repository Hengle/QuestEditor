using HoloToolkit.Unity;
using System;
using UnityEngine;
using UnityEngine.UI;

public class PopupInfo: Singleton<PopupInfo>
{
    private static GameObject popup;

    public static void ShowInfo(string description, Vector3 position)
    {
        popup = Instantiate(Resources.Load("Prefabs/Visualiser/Popup") as GameObject);
        popup.GetComponentInChildren<Text>().text = description;
        popup.transform.SetParent(FindObjectOfType<Canvas>().transform);
        popup.transform.position = position;
    }

    public static void HideInfo()
    {
        Destroy(popup);
    }
}
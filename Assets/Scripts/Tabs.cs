using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tabs {


    public static int DrawTabs(string[] options, int selected)
    {
        const float DarkGray = 0.4f;
        const float LightGray = 0.9f;
        const float StartSpace = 10;

        GUILayout.Space(StartSpace);
        Color storeColor = GUI.backgroundColor;
        Color highlightCol = new Color(LightGray, LightGray, LightGray);
        Color bgCol = new Color(DarkGray, DarkGray, DarkGray);

        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.padding.bottom = 8;

        GUILayout.BeginHorizontal();
        {   //Create a row of buttons
            for (int i = 0; i < options.Length; ++i)
            {
                GUI.backgroundColor = i == selected ? highlightCol : bgCol;
                if (GUILayout.Button(options[i], buttonStyle))
                {
                    selected = i; //Tab click
                }
            }
        }
        GUILayout.EndHorizontal();
        //Restore color
        GUI.backgroundColor = storeColor;
        //Draw a line over the bottom part of the buttons (ugly haxx)
        var texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, highlightCol);
        texture.Apply();
        GUI.DrawTexture(new Rect(0, buttonStyle.lineHeight + buttonStyle.border.top + buttonStyle.margin.top + StartSpace, Screen.width, 4), texture);

        return selected;
    }
}

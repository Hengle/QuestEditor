﻿using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditorInternal;


public class MyWindow : EditorWindow
{
    public enum EditorMode
    {
        packs,
        chains,
        parameters
    }

	private GUISkin QuestCreatorSkin
	{
		get
		{
			return Resources.Load ("Skins/QuestEditorSkin") as GUISkin;
		}
	}

    public Vector2 ParamsScrollPosition { get; private set; }

    public PathGame game = new PathGame();
	Vector2 lastMousePosition;
	Chain currentChain;
	ChainPack currentPack;
	ReorderableList packList;
	string[] toolbarStrings = new string[] {"Packs", "Chains"};
	int toolbarInt = 0, selectedChain = 0;
	Vector2 packsScrollPosition = Vector2.zero;
	Vector2 chainsScrollPosition = Vector2.zero;
	EditorMode chainEditorMode = EditorMode.packs;
	Rect makingPathRect = new Rect(Vector2.one*0.12345f, Vector2.one*0.12345f);
    bool makingPath = false;
    State pathAimState;
    Path startPath;
    private State inspectedState;
    private int inspectedPath;
	private Param deletingParam;

    [MenuItem("Window/Quest creator")]
	static void Init()
	{
		MyWindow window = (MyWindow)EditorWindow.GetWindow(typeof(MyWindow), false,  "Quest creator", true); 
		window.Show();
	}

	void OnGUI()
	{
        GUILayout.BeginVertical ();
        chainEditorMode = (EditorMode)Tabs.DrawTabs(new string[] { "packs", "chains", "parameters" }, (int)chainEditorMode);
        switch (chainEditorMode)
        {
            case EditorMode.chains:
                DrowChainsWindow();
                break;
            case EditorMode.packs:
                DrowPacksWindow();
                break;
            case EditorMode.parameters:
                DrowParamsWindow();
                break;
        }
        GUILayout.EndVertical();
       
    }

    private void DrowParamsWindow()
    {
		Event evt = Event.current;

		if (evt.button == 1 && evt.type == EventType.MouseDown)
		{
			GenericMenu menu = new GenericMenu();
			menu.AddItem(new GUIContent("Add param"), false, CreateParam);
			menu.ShowAsContext();

		}

		ParamsScrollPosition = GUILayout.BeginScrollView(ParamsScrollPosition, false, true, GUILayout.Width(position.width-5), GUILayout.Height(position.height-35));
		GUILayout.BeginVertical();
		int i = 0;
		Vector2 rectSize = new Vector2 ((position.width-40)/3, ((position.width-40)/3)/1.5f);

        foreach (Param param in game.parameters)
        {
			if(i%3==0)
			{
				GUILayout.BeginHorizontal ();
			}
			EditorGUI.DrawRect (new Rect(new Rect(i%3*(rectSize.x+5), Mathf.FloorToInt(i/3)*(rectSize.y+5/1.5f), rectSize.x, rectSize.y)), Color.gray/2);

			DoParamWindow (game.parameters[i], rectSize.x+5);
			//GUILayout.EndArea ();
			if(i%3==2 || i== game.parameters.Count-1)
			{
				GUILayout.EndHorizontal();
			}
			i++;
        }
		if(deletingParam!=null)
		{
			game.parameters.Remove (deletingParam);
			deletingParam = null;
		}
		GUILayout.EndVertical ();
        GUILayout.EndScrollView();
    }

	void DoParamWindow(Param p, float w)
	{
		GUILayout.BeginHorizontal (GUILayout.Width(w));
		GUILayout.BeginVertical (GUILayout.Width(w-5), GUILayout.Height(w/1.5f));
		GUILayout.Space (5);
		GUILayout.BeginHorizontal ();
		p.name = GUILayout.TextField (p.name);
		if(GUILayout.Button((Texture2D)Resources.Load("Icons/cancel") as Texture2D, GUILayout.Width(15), GUILayout.Height(15)))
		{
			deletingParam = p;
		}
		GUILayout.EndHorizontal();
		p.showing = !GUILayout.Toggle (!p.showing, "hidden");
		if(p.showing)
		{
			p.description = GUILayout.TextArea (p.description,GUILayout.Height(45));
			p.image = (Sprite)EditorGUILayout.ObjectField (p.image, typeof(Sprite), false);
		}
		p.activating = GUILayout.Toggle (p.activating, "activating");

		if(p.activating)
		{
			List<Chain> chains = new List<Chain> ();
			foreach(ChainPack pack in game.chainPacks)
			{
				chains.AddRange (pack.chains);
			}
			p.ucid = EditorGUILayout.Popup (p.ucid, chains.Select (x => x.name).Distinct ().ToArray()); 
			p.usableChain =  chains[p.ucid];
		}
		GUILayout.EndVertical ();
		GUILayout.EndHorizontal ();
	}

	void CreateParam()
	{
		game.parameters.Add (new Param());
	}

    void DrowPacksWindow ()
	{
		
		GUILayout.BeginHorizontal ();
		DrawPacksList ();
		DrawChainsList ();
		GUILayout.EndHorizontal ();
	}

	void DrawPacksList()
	{
		packsScrollPosition = GUILayout.BeginScrollView (packsScrollPosition, false, true, GUILayout.Width(position.width/2+5), GUILayout.Height(position.height-35));
		GUILayout.BeginVertical();
		ChainPack deletingPack = null;

		GUI.color = Color.green;
		if(GUILayout.Button("new pack", GUILayout.Width(position.width/2-30), GUILayout.Height(15)))
		{
			currentPack = new ChainPack ();
			game.chainPacks.Insert(0, currentPack);
			GUI.FocusControl ("packName"+(0));
		}
		GUI.color = Color.white;
	
		int counter = 1;
		foreach(ChainPack pack in game.chainPacks)
		{

			GUIStyle style = QuestCreatorSkin.GetStyle("Background");
			if(currentPack == pack)
			{
				style = QuestCreatorSkin.box;
			}

			Rect r = EditorGUILayout.BeginHorizontal (style);
			GUI.SetNextControlName ("packName"+(counter-1));
			pack.name = GUILayout.TextField (pack.name, GUILayout.Width(position.width/2-60), GUILayout.Height(15));

			if(GUI.GetNameOfFocusedControl ()=="packName"+(counter-1))
			{
				currentPack = pack;
			}
		
			if(GUILayout.Button((Texture2D)Resources.Load("Icons/cancel") as Texture2D, GUILayout.Width(30), GUILayout.Height(30)))
			{
				deletingPack = pack;
			}

			if (Event.current != null && Event.current.isMouse)
			{
				if (r.Contains (Event.current.mousePosition)) 
				{
					currentPack = pack;
				}
			}


			GUILayout.EndHorizontal ();
		
			counter++;
		}
			
		if(deletingPack!=null)
		{
			if(deletingPack == currentPack)
			{
				if(game.chainPacks.IndexOf(deletingPack)==0){
					if (game.chainPacks.Count > 1) {
						currentPack = game.chainPacks [1];
					} else {
						currentPack = null;
					}
				}
				else
				{
					currentPack = game.chainPacks [0];
				}
			}

			game.chainPacks.Remove (deletingPack);

		}

		GUILayout.EndVertical ();
		GUILayout.EndScrollView ();
	}

	void DrawChainsList()
	{
		if(currentPack == null)
		{
			return;
		}

		chainsScrollPosition = GUILayout.BeginScrollView (chainsScrollPosition, false, true, GUILayout.Width(position.width/2-5), GUILayout.Height(position.height-20));
		GUILayout.BeginVertical();

		Chain deletingChain = null;

		GUI.color = Color.green;
		if(GUILayout.Button("new chain", GUILayout.Width(position.width/2-30), GUILayout.Height(15)))
		{
			currentPack.chains.Insert(0, new Chain());
		}
		GUI.color = Color.white;

         

		foreach(Chain chain in currentPack.chains)
		{
			GUILayout.BeginHorizontal ();
			GUILayout.BeginVertical ();
			chain.name = GUILayout.TextField (chain.name, GUILayout.Width(position.width/2-60), GUILayout.Height(15));
			if(GUILayout.Button("edit", GUILayout.Width(50), GUILayout.Height(15)))
			{
				currentChain = chain;
				chainEditorMode = EditorMode.chains;
			}
			GUILayout.EndVertical ();
			if(GUILayout.Button((Texture2D)Resources.Load("Icons/cancel") as Texture2D, GUILayout.Width(30), GUILayout.Height(30)))
			{
				deletingChain = chain;
			}

			GUILayout.EndHorizontal ();

		}

		if(deletingChain!=null)
		{
			currentPack.chains.Remove (deletingChain);
	    }

		GUILayout.EndVertical ();
		GUILayout.EndScrollView ();
	}


    void DrowChainsWindow ()
	{
        if (currentChain == null)
        {
            return;
        }
        BeginWindows ();
        foreach (State state in currentChain.states)
		{
			DrawStateBox (state);
		}

        if (inspectedState != null)
        {
            DrawPathWindow();
        }

        EndWindows ();


        DrawAditional();


        Event evt = Event.current;

		if (evt.button == 1 && evt.type == EventType.MouseDown)
		{

			lastMousePosition = evt.mousePosition;


				GenericMenu menu = new GenericMenu();
				menu.AddItem(new GUIContent("Add state"), false, CreateState);
				menu.ShowAsContext();

		}
		
	}

	void DrawAditional (){

        if (makingPath)
		{
			Handles.BeginGUI();
			Handles.color = Color.white;
            //Handles.DrawLine(Vector3.zero, Vector3.one);
			Handles.DrawAAPolyLine(5, 2, new Vector3[] { makingPathRect.center, Event.current.mousePosition});
			Handles.EndGUI();

           
		}
   

        if (Event.current.button == 0 && Event.current.type == EventType.MouseUp)
        {
            makingPath = false;
            Repaint();
        }

        foreach (State state in currentChain.states)
        {
            int i = 0;
            foreach (Path path in state.pathes)
            {
                if (path.aimState != null)
                {
                    Handles.BeginGUI();
                    Handles.color = Color.red;
                    Handles.DrawAAPolyLine(5, 2, new Vector3[] { state.position.position+new Vector2(state.position.width*9/10, 55+i*23), path.aimState.position.center-new Vector2(path.aimState.position.width/2, 0) });
                    Handles.EndGUI();
                }
                i++;
            }
        }
    }
	void DrawStateBox(State state)
	{

        //GUI.Box(_boxPos, state.description);
        string ss = "";
        if (state.description!="")
        {
            ss = state.description.Split(new string[] { "\n" }, System.StringSplitOptions.RemoveEmptyEntries)[0];
            ss = ss.Substring(0, Mathf.Min(20, ss.Length));
        }
        Rect header = new Rect(state.position.position, new Vector3(state.position.width, 20));

        if (header.Contains(Event.current.mousePosition) && makingPath == true)
        {
            GUI.backgroundColor = Color.yellow;
            pathAimState = state;
        }
			
		state.position = new Rect (Mathf.Clamp(state.position.x, 5 , position.width-5-state.position.width), Mathf.Clamp(state.position.y, 35 , position.height-5-state.position.height), state.position.width, state.position.height);

        state.position = GUILayout.Window(currentChain.states.IndexOf(state), state.position, DoStateWindow, ss, GUILayout.Width(150), GUILayout.Height(100));

        GUI.backgroundColor = Color.white;
    }

	void DoStateWindow(int windowID)
	{
            if (Event.current.button == 0 && Event.current.type == EventType.MouseUp)
            {
                if (windowID == currentChain.states.IndexOf(pathAimState) && makingPath)
                {
                    startPath.aimState = pathAimState;
                    makingPath = false;
                    Repaint();
                }
            }
 

        GUILayout.BeginHorizontal ();
		GUILayout.BeginVertical (GUILayout.Width(130));
		currentChain.states [windowID].description = GUILayout.TextArea (currentChain.states [windowID].description, GUILayout.Height(45));
		currentChain.states [windowID].image = (Sprite)EditorGUILayout.ObjectField (currentChain.states [windowID].image, typeof(Sprite), false);
		GUILayout.EndVertical ();



		GUILayout.BeginVertical (GUILayout.Width(20));
		if (GUILayout.Button ((Texture2D)Resources.Load ("Icons/add") as Texture2D, GUILayout.Width (20), GUILayout.Height (20))) 
		{
			currentChain.states [windowID].pathes.Add (new Path ());
		}

        /*
		Rect buttonRect = GUILayoutUtility.GetLastRect (); 
		if (buttonRect.Contains(Event.current.mousePosition)) 
		{
			makingPathRect = buttonRect;
            makingPath = true;
		}*/

        int i = 0;
		foreach(Path path in currentChain.states [windowID].pathes)
		{
            if (GUILayout.Button("", GUILayout.Width(20), GUILayout.Height(20)))
            {
                if (inspectedState == currentChain.states[windowID] && inspectedPath == i)
                {
                    inspectedPath = -1;
                    inspectedState = null;
                }
                else
                {
                    inspectedState = currentChain.states[windowID];
                    inspectedPath = i;
                }
            }; 
            if(GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition) && Event.current.button == 0 && makingPath == false )
            {
                   makingPathRect = new Rect(currentChain.states[windowID].position.position+ GUILayoutUtility.GetLastRect().position, GUILayoutUtility.GetLastRect().size);
                   makingPath = true;
                   startPath = currentChain.states[windowID].pathes[i];
            };
            i++;

        }

       
		GUILayout.EndVertical ();
		GUILayout.EndHorizontal ();
		GUI.DragWindow ();
	}

    private void DrawPathWindow()
    {
        if (inspectedState.pathes.Count > inspectedPath)
        {
            string ss = "";
            if (inspectedState.pathes[inspectedPath].text != "")
            {
                ss = inspectedState.pathes[inspectedPath].text.Split(new string[] { "\n" }, System.StringSplitOptions.RemoveEmptyEntries)[0];
                ss = ss.Substring(0, Mathf.Min(20, ss.Length));
            }
            GUILayout.Window(42, new Rect(inspectedState.position.x + inspectedState.position.width, inspectedState.position.y + 43 + inspectedPath * 23, 100, 50), DoPathWindow, ss, GUILayout.Width(100), GUILayout.Height(50));
        }
    }

    private void DoPathWindow(int id)
    {
        GUILayout.BeginVertical(GUILayout.Width(130));
        inspectedState.pathes[inspectedPath].text = GUILayout.TextArea(inspectedState.pathes[inspectedPath].text, GUILayout.Height(30));
        GUILayout.BeginHorizontal();
        inspectedState.pathes[inspectedPath].auto = GUILayout.Toggle(inspectedState.pathes[inspectedPath].auto, "auto", GUILayout.Width(60));

        GUI.color = Color.yellow;
        if (GUILayout.Button((Texture2D)Resources.Load("Icons/add") as Texture2D, GUILayout.Width(20), GUILayout.Height(20)))
        {
            inspectedState.pathes[inspectedPath].conditions.Add(new Condition());
        }
        GUI.color = Color.green;
        if (GUILayout.Button((Texture2D)Resources.Load("Icons/add") as Texture2D, GUILayout.Width(20), GUILayout.Height(20)))
        {
            inspectedState.pathes[inspectedPath].changes.Add(new ParamChanges());
        }
        GUI.color = Color.white;
        GUILayout.EndHorizontal();
        foreach (Condition condition in inspectedState.pathes[inspectedPath].conditions)
        {
			GUILayout.BeginHorizontal ();
            condition.conditionString = GUILayout.TextArea(condition.conditionString, GUILayout.Height(15));
			GUI.color = Color.green;
			if(GUILayout.Button("", GUILayout.Width(15), GUILayout.Height(15)))
			{
				condition.parameters.Add (new Param());
			}
			GUILayout.EndHorizontal ();
			Param removingParam=null;
			int i = 0;
			foreach(Param p in condition.parameters)
			{
				GUILayout.BeginHorizontal ();
				GUILayout.Space (10);
				GUILayout.Label (p+""+i+"  ");


				GUI.color = Color.red;
				if(GUILayout.Button("", GUILayout.Width(15), GUILayout.Height(15)))
				{
					removingParam = p;
				}
				GUI.color = Color.white;
				GUILayout.EndHorizontal ();
				i++;
			}
			if(removingParam!=null)
			{
				condition.parameters.Remove (removingParam);
			}
			GUI.color = Color.white;

        }
        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(1) });
        foreach (ParamChanges paramChanger in inspectedState.pathes[inspectedPath].changes)
        {
			//paramChanger.aimParamId = EditorGUILayout.Popup (paramChanger.aimParamId, game.parameters.Select(p=>p.name).Distinct().ToArray());
			//paramChanger.aimParam = game.parameters [paramChanger.aimParamId];
        }

        GUILayout.EndVertical();
    }

    void CreateState()
	{
		State newState = new State();
		newState.position = new Rect (lastMousePosition.x - newState.position.width/2, lastMousePosition.y - newState.position.height/2, newState.position.width, newState.position.height);
		currentChain.states.Add (newState);
	}
}
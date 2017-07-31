using UnityEngine;
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

    public PathGame game;
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

	 	
		PathGame g =  (PathGame)EditorGUILayout.ObjectField(game, typeof(PathGame), false);

		if(g!=game)
		{
			if (game)
			{
				AssetDatabase.SaveAssets ();
			}
			game = g;
		}

		if (game) {
			chainEditorMode = (EditorMode)Tabs.DrawTabs (new string[] { "packs", "chains", "parameters" }, (int)chainEditorMode);
			switch (chainEditorMode) {
			case EditorMode.chains:
				DrowChainsWindow ();
				break;
			case EditorMode.packs:
				DrowPacksWindow ();
				break;
			case EditorMode.parameters:
				DrowParamsWindow ();
				break;
			}
		}
        GUILayout.EndVertical();
       
		if(game)
		{
			game.SetDirty ();
		}
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

		ParamsScrollPosition = GUILayout.BeginScrollView(ParamsScrollPosition, false, true, GUIStyle.none, GUI.skin.verticalScrollbar,GUILayout.Width(position.width-5), GUILayout.Height(position.height-35));
		GUILayout.BeginVertical();
		int i = 0;
		Vector2 rectSize = new Vector2 ((position.width-40)/3, (position.width-40)/3);

        foreach (Param param in game.parameters)
        {
			if(i%3==0)
			{
				GUILayout.BeginHorizontal ();
			}
			Rect r = new Rect (new Rect (i % 3 * (rectSize.x + 5), Mathf.FloorToInt (i / 3) * (rectSize.y + 5 / 1.5f), rectSize.x, rectSize.y));
			EditorGUI.DrawRect (r, Color.gray/2);

			DoParamWindow (game.parameters[i], rectSize.x+5, r);
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

	void DoParamWindow(Param p, float w, Rect r)
	{
		p.scrollPosition = GUILayout.BeginScrollView (p.scrollPosition, false, false, GUIStyle.none, GUI.skin.label, GUILayout.Width(r.width+5), GUILayout.Height(r.height));
		GUILayout.BeginHorizontal (GUILayout.Width(w));
		GUILayout.BeginVertical (GUILayout.Width(w-5), GUILayout.Height(w/1.5f));
		GUILayout.Space (5);
		GUILayout.BeginHorizontal ();
		p.name = GUILayout.TextField (p.name, GUILayout.Width(185));
		p.tags = GUILayout.TextField (p.tags, GUILayout.Width(150));
		GUI.color = Color.red;
		if(GUILayout.Button("", GUILayout.Width(15), GUILayout.Height(15)))
		{
			deletingParam = p;
		}
		GUI.color = Color.white;
		GUILayout.EndHorizontal();
		p.showing = !GUILayout.Toggle (!p.showing, "hidden");
		List<Chain> chains = new List<Chain> ();
		foreach (ChainPack pack in game.chainPacks) {
			chains.AddRange (pack.chains);
		}

		if (p.showing) {
			p.description = GUILayout.TextArea (p.description, GUILayout.Height (45));
			p.image = (Sprite)EditorGUILayout.ObjectField (p.image, typeof(Sprite), false);
		

	
			p.activating = GUILayout.Toggle (p.activating, "activating");


			if (p.activating) {
				if (p.usableChain == null || !chains.Contains (p.usableChain)) {
					if (chains.Count > 0) {
						p.usableChain = chains [0]; 
					} else {
						p.activating = false;
					}
				}
			}


			if (p.activating) {
				p.usableChain = chains [EditorGUILayout.Popup (chains.IndexOf (p.usableChain), chains.Select (x => x.name).ToArray ())]; 
				p.manualActivationWithConditions = GUILayout.Toggle (p.manualActivationWithConditions, "with condition");
			}

			if (p.activating && p.manualActivationWithConditions) 
			{
				EditorGUILayout.BeginHorizontal ();
				p.manualUsingCondition.conditionString = EditorGUILayout.TextField (p.manualUsingCondition.conditionString); 
				GUI.color = Color.yellow;
				if (GUILayout.Button ((Texture2D)Resources.Load ("Icons/add") as Texture2D, GUILayout.Width (20), GUILayout.Height (20))) {
					if (game.parameters.Count > 0) {
						p.manualUsingCondition.parameters.Add (game.parameters [0]);
					}
				}

				GUI.color = Color.white;
				EditorGUILayout.EndHorizontal ();

				Param removingParam = null;

				for (int i = 0; i < p.manualUsingCondition.parameters.Count; i++) {
					EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.LabelField ("[p" + i + "]", GUILayout.Width (35));

					if (!game.parameters.Contains (p.manualUsingCondition.parameters [i])) {
						if (game.parameters.Count > 0) {
							p.manualUsingCondition.parameters [i] = game.parameters [0];
						} else {
							removingParam = p.manualUsingCondition.parameters [i];
							continue;
						}
					}
					p.manualUsingCondition.parameters [i] = game.parameters [EditorGUILayout.Popup (game.parameters.IndexOf (p.manualUsingCondition.parameters [i]), game.parameters.Select (x => x.name).ToArray ())]; 


					GUI.color = Color.red;
					if (GUILayout.Button ("", GUILayout.Height (15), GUILayout.Width (15))) {
						removingParam = p.manualUsingCondition.parameters [i];
					}
					GUI.color = Color.white;
					EditorGUILayout.EndHorizontal ();
				}

				if (removingParam != null) {
					p.manualUsingCondition.parameters.Remove (removingParam);
				}
			}
		}
		GUI.color = Color.green;
		if (GUILayout.Button ("add auto action")) {
			if(chains.Count>0)
			{
				p.autoActivatedChains.Add (new Condition(), chains[0]);
			}
		};

		Condition removingCondition = null;

		for(int i = 0; i< p.autoActivatedChains.Count; i++)
		{
			
			if(!chains.Contains(p.autoActivatedChains.ElementAt(i).Value))
			{
				if (chains.Count > 0) {
					Condition pair = p.autoActivatedChains.ElementAt (i).Key;
					p.autoActivatedChains[pair] = chains [0];
				} else 
				{
					removingCondition = p.autoActivatedChains.ElementAt(i).Key;
					continue;
				}
			}
			GUILayout.BeginHorizontal ();
			Condition c = p.autoActivatedChains.ElementAt (i).Key;
			GUI.color = Color.white;
			p.autoActivatedChains[c] = chains [EditorGUILayout.Popup (chains.IndexOf (p.autoActivatedChains[c]), chains.Select (x => x.name).ToArray ())]; 
			GUI.color = Color.red;
			if(GUILayout.Button("", GUILayout.Width(15), GUILayout.Height(15)))
			{
				removingCondition = p.autoActivatedChains.ElementAt(i).Key;
			}
			GUILayout.EndHorizontal ();


			GUILayout.BeginHorizontal ();
			Param removingParam = null;
			GUI.color = Color.white;
			c.conditionString = GUILayout.TextField (c.conditionString);
			GUI.color = Color.yellow;
			if (GUILayout.Button((Texture2D)Resources.Load("Icons/add") as Texture2D, GUILayout.Width(20), GUILayout.Height(20)))
			{
				if(game.parameters.Count>0)
				{
					c.parameters.Add(game.parameters[0]);
				}
			}


			GUI.color = Color.white;
			EditorGUILayout.EndHorizontal ();

			for(int j = 0;j<c.parameters.Count;j++)
			{
				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.LabelField ("[p"+j+"]", GUILayout.Width(35));

				if(!game.parameters.Contains(c.parameters[j]))
				{
					if(game.parameters.Count>0){
						c.parameters [j] = game.parameters [0];
					}
					else{
						removingParam = c.parameters[j];
						continue;
					}
				}
				c.parameters[j] = game.parameters[EditorGUILayout.Popup (game.parameters.IndexOf(c.parameters[j]), game.parameters.Select (x => x.name).ToArray())]; 


				GUI.color = Color.red;
				if(GUILayout.Button("", GUILayout.Height(15), GUILayout.Width(15)))
				{
					removingParam = c.parameters[i];
				}
				GUI.color = Color.white;
				EditorGUILayout.EndHorizontal ();
			}

			if(removingParam!=null)
			{
				c.parameters.Remove (removingParam);
			}
		}
		if(removingCondition!=null)
		{
			p.autoActivatedChains.Remove (removingCondition);
		}
		GUI.color = Color.white;
		GUILayout.EndVertical ();
		GUILayout.EndHorizontal ();
		GUILayout.EndScrollView ();
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
		packsScrollPosition = GUILayout.BeginScrollView (packsScrollPosition, false, true, GUIStyle.none, GUI.skin.label, GUILayout.Width(position.width/2+5), GUILayout.Height(position.height-35));
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

		chainsScrollPosition = GUILayout.BeginScrollView (chainsScrollPosition, false, true, GUIStyle.none, GUI.skin.label,GUILayout.Width(position.width/2-5), GUILayout.Height(position.height-20));
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
			
		state.position = new Rect (Mathf.Clamp(state.position.x, 5 , position.width-5-state.position.width), Mathf.Clamp(state.position.y, 55 , position.height-5-state.position.height), state.position.width, state.position.height);

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
		if (inspectedState.pathes.Count > inspectedPath && inspectedPath>=0)
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
		GUILayout.BeginHorizontal ();
        inspectedState.pathes[inspectedPath].text = GUILayout.TextArea(inspectedState.pathes[inspectedPath].text, GUILayout.Height(30));
		GUI.color = Color.red;
		if(GUILayout.Button("",GUILayout.Width(20),GUILayout.Height(20)))
		{
			inspectedState.pathes.RemoveAt (inspectedPath);
			inspectedPath = -1;
			return;
		}
		GUI.color = Color.white;
		GUILayout.EndHorizontal ();
        inspectedState.pathes[inspectedPath].auto = GUILayout.Toggle(inspectedState.pathes[inspectedPath].auto, "auto", GUILayout.Width(60));


		if (!inspectedState.pathes [inspectedPath].auto) 
		{
			DrawCondition (inspectedState.pathes [inspectedPath]);
		}

		GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(1) });

		DrawChanges (inspectedState.pathes[inspectedPath]);

        GUILayout.EndVertical();
    }

    void CreateState()
	{
		State newState = new State();
		newState.position = new Rect (lastMousePosition.x - newState.position.width/2, lastMousePosition.y - newState.position.height/2, newState.position.width, newState.position.height);
		currentChain.states.Add (newState);
	}

	private void OnDestroy()
	{
		if (game)
		{
			AssetDatabase.SaveAssets ();
		}
	}

	private void DrawCondition(Path path)
	{
		EditorGUILayout.BeginHorizontal ();
		path.condition.conditionString = EditorGUILayout.TextField (path.condition.conditionString); 
		GUI.color = Color.yellow;
		if (GUILayout.Button((Texture2D)Resources.Load("Icons/add") as Texture2D, GUILayout.Width(20), GUILayout.Height(20)))
		{
			if(game.parameters.Count>0)
			{
				path.condition.parameters.Add(game.parameters[0]);
			}
		}

		GUI.color = Color.white;
		EditorGUILayout.EndHorizontal ();

		Param removingParam = null;

		for(int i = 0;i<path.condition.parameters.Count;i++)
		{
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField ("[p"+i+"]", GUILayout.Width(35));

			if(!game.parameters.Contains(path.condition.parameters[i]))
			{
				if(game.parameters.Count>0){
					path.condition.parameters [i] = game.parameters [0];
				}
				else{
					removingParam = path.condition.parameters[i];
					continue;
				}
			}
			path.condition.parameters[i] = game.parameters[EditorGUILayout.Popup (game.parameters.IndexOf(path.condition.parameters[i]), game.parameters.Select (x => x.name).ToArray())]; 


			GUI.color = Color.red;
			if(GUILayout.Button("", GUILayout.Height(15), GUILayout.Width(15)))
			{
				removingParam = path.condition.parameters[i];
			}
			GUI.color = Color.white;
			EditorGUILayout.EndHorizontal ();
		}

		if(removingParam!=null)
		{
			path.condition.parameters.Remove (removingParam);
		}

	}

	private void DrawChanges(Path path)
	{
		GUILayout.BeginHorizontal ();
		GUILayout.FlexibleSpace ();
		GUI.color = Color.green;
		if (GUILayout.Button((Texture2D)Resources.Load("Icons/add") as Texture2D, GUILayout.Width(20), GUILayout.Height(20)))
		{
			if(game.parameters.Count>0)
			{
				inspectedState.pathes[inspectedPath].changes.Add(new ParamChanges(game.parameters[0]));
			}
		}
		GUILayout.EndHorizontal ();
		GUI.color = Color.white;

		ParamChanges removingChanger = null;
		for (int i = 0; i< path.changes.Count; i++) {
			EditorGUILayout.BeginHorizontal ();

			if(!game.parameters.Contains(path.changes[i].aimParam))
			{
				if (game.parameters.Count > 0) {
					path.changes [i].aimParam = game.parameters [0];	
				} else {
					removingChanger = path.changes [i];
					continue;
				}
			}
			path.changes [i].aimParam = game.parameters [EditorGUILayout.Popup (game.parameters.IndexOf (path.changes[i].aimParam), game.parameters.Select (x => x.name).ToArray (), GUILayout.Width(50))]; 

			GUILayout.Label (" = ");

			path.changes [i].changeString = GUILayout.TextField (path.changes [i].changeString, GUILayout.Width(150));
			GUI.color = Color.red;
			if (GUILayout.Button ("", GUILayout.Height (15), GUILayout.Width (15))) {
				removingChanger = path.changes[i];
			}
			GUI.color = Color.yellow;
			if (GUILayout.Button ("", GUILayout.Height (15), GUILayout.Width (15))) {
				if (game.parameters.Count > 0) {
					path.changes[i].parameters.Add (game.parameters [0]);
				}
			}
			GUI.color = Color.white;

			Param removingParam = null;
			EditorGUILayout.EndHorizontal ();

			for (int j = 0; j < path.changes[i].parameters.Count; j++) {
				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.LabelField ("[p" + j + "]", GUILayout.Width (35));

				if (!game.parameters.Contains (path.changes[i].parameters [j])) {
					if (game.parameters.Count > 0) {
						path.changes[i].parameters [j] = game.parameters [0];
					} else {
						removingParam = path.changes[i].parameters [j];
						continue;
					}
				}
				path.changes[i].parameters [j] = game.parameters [EditorGUILayout.Popup (game.parameters.IndexOf (path.changes[i].parameters [j]), game.parameters.Select (x => x.name).ToArray ())]; 


				GUI.color = Color.red;
				if (GUILayout.Button ("", GUILayout.Height (15), GUILayout.Width (15))) {
					removingParam = path.changes[i].parameters [j];
				}
				GUI.color = Color.white;
				EditorGUILayout.EndHorizontal ();
			}

			if (removingParam != null) {
				path.changes[i].parameters.Remove (removingParam);
			}

			GUI.color = Color.white;

		}
		if(removingChanger!=null)
		{
			path.changes.Remove (removingChanger);
		}
	}
}
﻿using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditorInternal;

public class QuestWindow : EditorWindow
{
    private delegate void StateDel(State state);

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

    public static PathGame game;
	Vector2 lastMousePosition;
	Chain currentChain;
	ChainPack currentPack;
	ReorderableList packList;
	int selectedChain = 0;
	Vector2 packsScrollPosition = Vector2.zero;
	Vector2 chainsScrollPosition = Vector2.zero;
	EditorMode chainEditorMode = EditorMode.packs;
	Rect makingPathRect = new Rect(Vector2.one*0.12345f, Vector2.one*0.12345f);
    bool makingPath = false;
    State pathAimState;
    Path startPath;
    private State menuState;
    private State inspectedState;
    private int inspectedPath;
	private Param deletingParam;
    private Texture2D backgroundTexture;
    private StateLink menuStateLink;

    private Texture2D BackgroundTexture
    {
        get
        {
            if (backgroundTexture == null)
            {
                backgroundTexture = (Texture2D)Resources.Load("Icons/background") as Texture2D;
                BackgroundTexture.wrapMode = TextureWrapMode.Repeat;
            }
            return backgroundTexture;
        }
    }

	static void Init()
	{
		QuestWindow window = (QuestWindow)EditorWindow.GetWindow(typeof(QuestWindow), false,  "Quest creator", true);
        window.minSize = new Vector2(600, 400);
		window.Show();
	}

    public static void Init(PathGame editedGame = null)
    {
        game = editedGame;
        GUIDManager.SetInspectedGame(game);
        Init();
    }

    void OnGUI()
	{
        if (Event.current.type == EventType.ValidateCommand)
        {
            switch (Event.current.commandName)
            {
                case "UndoRedoPerformed":
                    Repaint();
                    break;
            }

        }

        switch (chainEditorMode) {
			case EditorMode.chains:
                if (game)
                {
                    DrowChainsWindow();
                }
				break;
			case EditorMode.packs:
				DrowPacksWindow ();
				break;
			case EditorMode.parameters:
                if (game)
                {
                    DrowParamsWindow();
                }
				break;
			}

        EditorMode newChainMode = (EditorMode)Tabs.DrawTabs(new Rect(0, 0, position.width, 30), new string[] { "packs", "chains", "parameters" }, (int)chainEditorMode);
        if (newChainMode == EditorMode.chains && newChainMode!=chainEditorMode)
        {
            RecalculateWindowsPositions();
        }

        chainEditorMode = newChainMode;

        if (game)
		{
			game.SetDirty ();
		}
       
    }

    private void DrowParamsWindow()
    {
        Undo.RecordObject(game, "params");
        Event evt = Event.current;

		if (evt.button == 1 && evt.type == EventType.MouseDown)
		{
			GenericMenu menu = new GenericMenu();
			menu.AddItem(new GUIContent("Add param"), false, CreateParam);
			menu.ShowAsContext();

		}

        GUILayout.BeginVertical();
        GUILayout.Space(35);

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

			DoParamWindow (game.parameters[i], new Rect(r.position.x, r.position.y, r.width+3, r.height+3));
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
        GUILayout.EndVertical();
        Undo.FlushUndoRecordObjects();
    }

	void DoParamWindow(Param p, Rect r)
	{
        p.scrollPosition = GUILayout.BeginScrollView (p.scrollPosition, false, false, GUIStyle.none, GUI.skin.label, GUILayout.Width(r.width), GUILayout.Height(r.height));
		GUILayout.BeginHorizontal (GUILayout.Width(r.width-5));
		GUILayout.BeginVertical (GUILayout.Width(r.width-5), GUILayout.Height((r.width-10)/1.5f));
        GUILayout.Space (5);
		GUILayout.BeginHorizontal ();
		p.name = EditorGUILayout.TextArea (p.name, GUILayout.Width(2*(r.width-10)/3-15));
		p.tags = EditorGUILayout.TextArea (p.tags, GUILayout.Width((r.width-10) / 3 - 15));
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
			p.description = EditorGUILayout.TextArea (p.description, GUILayout.Height (45), GUILayout.Width(r.width-10));
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
				

					List<State> states = new List<State> ();
					states.AddRange (p.usableChain.states);
					if (p.usableState == null || !states.Contains (p.usableState)) 
					{
						p.usableState = states[0];
					}
                p.manualActivationWithState = GUILayout.Toggle(p.manualActivationWithState, "with state");
                p.withChange = GUILayout.Toggle(p.withChange, "with change");
                if (p.manualActivationWithState)
                {
                    p.usableChain = chains[EditorGUILayout.Popup(chains.IndexOf(p.usableChain), chains.Select(x => x.name).ToArray())];
                    p.usableState = states[EditorGUILayout.Popup(states.IndexOf(p.usableState), states.Select(x => x.description).ToArray())];
                }
					

                if (p.withChange)
				{
					if(GUILayout.Button("add change"))
					{
						p.manualUsingChange.Add (new ParamChanges(p));
					}

					ParamChanges deletingPch = null;
					foreach(ParamChanges pch in p.manualUsingChange){
						GUILayout.BeginHorizontal ();
                    	DrawChanges(pch);
						GUI.color = Color.red;
						if(GUILayout.Button("", GUILayout.Width(15), GUILayout.Height(15)))
						{
							deletingPch = pch;
						}
						GUI.color = Color.white;
						GUILayout.EndHorizontal ();
					}
					if(deletingPch!=null)
					{
						p.manualUsingChange.Remove (deletingPch);
					}
                }
                
            }

			if (p.activating && (p.manualActivationWithState || p.withChange)) 
			{
				EditorGUILayout.BeginHorizontal ();
                GUI.backgroundColor = Color.white;

                try
                {
                    ExpressionSolver.CalculateBool(p.manualUsingCondition.conditionString, p.manualUsingCondition.Parameters);
                }
                catch
                {
                    GUI.color = Color.red;
                }

                p.manualUsingCondition.conditionString = EditorGUILayout.TextArea (p.manualUsingCondition.conditionString, GUILayout.Width(r.width - 35)); 
				GUI.color = Color.yellow;
				if (GUILayout.Button ((Texture2D)Resources.Load ("Icons/add") as Texture2D, GUILayout.Width (20), GUILayout.Height (20))) {
					if (game.parameters.Count > 0) {
						p.manualUsingCondition.AddParam (game.parameters [0]);
					}
				}

				GUI.color = Color.white;
				EditorGUILayout.EndHorizontal ();

				Param removingParam = null;

				for (int i = 0; i < p.manualUsingCondition.Parameters.Count; i++) {
					EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.LabelField ("[p" + i + "]", GUILayout.Width ((r.width-10)/3));

					if (!game.parameters.Contains (p.manualUsingCondition.Parameters [i])) {
						if (game.parameters.Count > 0) {
							p.manualUsingCondition.setParam(i,game.parameters [0]);
						} else {
							removingParam = p.manualUsingCondition.Parameters [i];
							continue;
						}
					}

					p.manualUsingCondition.setParam(i, game.parameters [EditorGUILayout.Popup (game.parameters.IndexOf (p.manualUsingCondition.Parameters [i]), game.parameters.Select (x => x.name).ToArray ())]); 


					GUI.color = Color.red;
					if (GUILayout.Button ("", GUILayout.Height (15), GUILayout.Width (15))) {
						removingParam = p.manualUsingCondition.Parameters [i];
					}
					GUI.color = Color.white;
					EditorGUILayout.EndHorizontal ();
				}

				if (removingParam != null) {
					p.manualUsingCondition.RemoveParam (removingParam);
				}
			}
		}

        GUILayout.BeginHorizontal();
		GUI.color = Color.green;
		if (GUILayout.Button ("auto state")) {
			if(chains.Count>0)
			{
				p.AddAutoActivatedChain (new Condition(), chains[0]);
			}
		};
        GUI.color = Color.cyan;
        if (GUILayout.Button("auto change"))
        {
            p.autoActivatedChangesGUIDS.Add(new ConditionChange(new Condition()));
        }
        GUI.color = Color.white;
        GUILayout.EndHorizontal();

        ///
		int removingCondition = -1;

		for(int i = 0; i< p.autoActivatedChains.Count; i++)
		{
			if(p.autoActivatedChains.ElementAt(i).Value == null)
			{
				if(GUIDManager.GetChainByGuid(p.autoActivatedChainsGUIDS[i].chainGuid) == null)
				{
						List<Chain> chains2 = new List<Chain>();
						foreach(ChainPack cp in game.chainPacks)
						{
							chains2.AddRange(cp.chains);
						}
					if (chains2.Count > 0) {
						p.SetAutoActivatedChain (i, chains2 [0]);
						p.SetAutoActivatedState (i, chains2 [0].StartState);
					} else {
						Repaint ();
						return;
					}
				}
				else
				{
					p.SetAutoActivatedState (i, GUIDManager.GetChainByGuid(p.autoActivatedChainsGUIDS[i].chainGuid).StartState);	
				}
			}

			if(!chains.Contains(GUIDManager.GetChainByStateGuid(p.autoActivatedChains.ElementAt(i).Value.stateGUID)))
			{
				if (chains.Count > 0) {
					Condition pair = p.autoActivatedChains.ElementAt (i).Key;
						p.autoActivatedChains[pair] = chains [0].StartState;
				} else 
				{
					removingCondition = i;
					continue;
				}
			}
			GUILayout.BeginHorizontal ();
			Condition c = p.autoActivatedChains.ElementAt (i).Key;
			GUI.color = Color.white;

			int v = EditorGUILayout.Popup(chains.IndexOf(GUIDManager.GetChainByGuid(p.autoActivatedChainsGUIDS[i].chainGuid)), chains.Select(x => x.name).ToArray());
			p.SetAutoActivatedChain(i, chains[v]); 

			if(p.autoActivatedChainsGUIDS[i].chainGuid!=chains[v].ChainGuid)
			{
				p.SetAutoActivatedState (i, chains[v].states[0]);
			}

			int v2 = EditorGUILayout.Popup(GUIDManager.GetChainByGuid(p.autoActivatedChainsGUIDS[i].chainGuid).states.IndexOf(GUIDManager.GetStateByGuid(p.autoActivatedChainsGUIDS[i].stateGuid)), GUIDManager.GetChainByStateGuid(p.autoActivatedChainsGUIDS[i].stateGuid).states.Select(x => x.description).ToArray());


			if(v2>=GUIDManager.GetChainByGuid(p.autoActivatedChainsGUIDS[i].chainGuid).states.Count || v2<0)
			{
				v2 = 0;
			}
				
			p.SetAutoActivatedState (i, chains[v].states[v2]);

			GUI.color = Color.red;
			if(GUILayout.Button("", GUILayout.Width(15), GUILayout.Height(15)))
			{
				removingCondition = i;
			}
			GUILayout.EndHorizontal ();


			GUILayout.BeginHorizontal ();
			Param removingParam = null;
			GUI.color = Color.white;
            try
            {
                ExpressionSolver.CalculateBool(c.conditionString, c.Parameters);
            }
            catch
            {
                GUI.color = Color.red;
            }
            c.conditionString = EditorGUILayout.TextArea (c.conditionString, GUILayout.Width(r.width-35));
			GUI.color = Color.yellow;
			if (GUILayout.Button((Texture2D)Resources.Load("Icons/add") as Texture2D, GUILayout.Width(20), GUILayout.Height(20)))
			{
				if(game.parameters.Count>0)
				{
					c.AddParam(game.parameters[0]);
				}
			}


			GUI.color = Color.white;
			EditorGUILayout.EndHorizontal ();

			for(int j = 0;j<c.Parameters.Count;j++)
			{
				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.LabelField ("[p"+j+"]", GUILayout.Width((r.width-10)/3));

				if(!game.parameters.Contains(c.Parameters[j]))
				{
					if(game.parameters.Count>0){
						c.Parameters [j] = game.parameters [0];
					}
					else{
						removingParam = c.Parameters[j];
						continue;
					}
				}

                int val = EditorGUILayout.Popup(game.parameters.IndexOf(c.Parameters[j]), game.parameters.Select(x => x.name).ToArray());
                c.setParam(j, game.parameters[val]);
				GUI.color = Color.red;
				if(GUILayout.Button("", GUILayout.Height(15), GUILayout.Width(15)))
				{
					removingParam = c.Parameters[i];
				}
				GUI.color = Color.white;
				EditorGUILayout.EndHorizontal ();
			}

			if(removingParam!=null)
			{
				c.RemoveParam (removingParam);
			}
		}
		if(removingCondition>=0)
		{
			p.RemoveAutoActivatedChain  (removingCondition);
		}
        ///
        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(1) });

        ConditionChange removingConditionChange = null;
        foreach (ConditionChange conditionChange in p.autoActivatedChangesGUIDS)
        {
            GUILayout.BeginHorizontal();
            DrawCondition(conditionChange.condition);
            GUI.color = Color.green;
            if (GUILayout.Button((Texture2D)Resources.Load("Icons/cancel") as Texture2D, GUILayout.Width(20), GUILayout.Height(20)))
            {
                conditionChange.changes.Add(new ParamChanges(p));
            }
            GUI.color = Color.red;
            if (GUILayout.Button("", GUILayout.Width(20), GUILayout.Height(20)))
            {
                removingConditionChange = conditionChange;
            }
            GUI.color = Color.white;
            GUILayout.EndHorizontal();
            ParamChanges removingChanger = null;

            foreach (ParamChanges c in conditionChange.changes)
            {
                GUILayout.BeginHorizontal();
                DrawChanges(c);
                GUI.color = Color.red;
                if (GUILayout.Button("", GUILayout.Width(15), GUILayout.Height(15)))
                {
                    removingChanger = c;
                }
                GUILayout.EndHorizontal();
            }
            if (removingChanger!=null)
            {
                conditionChange.changes.Remove(removingChanger);
            }
        }

        if (removingConditionChange!=null)
        {
            p.autoActivatedChangesGUIDS.Remove(removingConditionChange);
        }
        GUI.color = Color.white;
		GUILayout.EndVertical ();
		GUILayout.EndHorizontal ();
		GUILayout.EndScrollView ();
	}

	void CreateParam()
	{
		game.parameters.Add (new Param(GUIDManager.GetItemGUID()));
	}

    void DrowPacksWindow ()
	{  
        GUILayout.BeginVertical();
        GUILayout.Space(30);

            GUILayout.BeginHorizontal();
        Undo.RecordObject(game, "packs and chains");
   
        DrawPacksList();
        DrawChainsList();
        Undo.FlushUndoRecordObjects();
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();
	}

	void DrawPacksList()
	{
        packsScrollPosition = GUILayout.BeginScrollView (packsScrollPosition, false, true, GUIStyle.none, GUI.skin.label, GUILayout.Width(position.width/2+5), GUILayout.Height(position.height-35));
		GUILayout.BeginVertical();
		ChainPack deletingPack = null;

		GUI.color = Color.green;

        if (GUILayout.Button("new pack", GUILayout.Width(position.width/2-30), GUILayout.Height(15)))
		{
			currentPack = new ChainPack (GUIDManager.getChainPackGuid ());
			game.chainPacks.Insert(0, currentPack);
			GUI.FocusControl ("packName"+(0));
			Repaint ();
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
			pack.name = EditorGUILayout.TextArea (pack.name, GUILayout.Width(position.width/4-30), GUILayout.Height(15));
            pack.tags = EditorGUILayout.TextArea(pack.tags, GUILayout.Width(position.width / 4 - 30), GUILayout.Height(15));
            if (GUI.GetNameOfFocusedControl ()=="packName"+(counter-1))
			{
				currentPack = pack;
			}
		
			if(GUILayout.Button((Texture2D)Resources.Load("Icons/cancel") as Texture2D, GUILayout.Width(30), GUILayout.Height(30)))
			{
				deletingPack = pack;
			}

			if (Event.current.isMouse)
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
            inspectedPath = -1;
            inspectedState = null;

            if (deletingPack == currentPack)
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
			currentPack.chains.Insert(0, new Chain(GUIDManager.GetChainGUID(), GUIDManager.GetStateGUID()));
			Repaint ();
		}
		GUI.color = Color.white;

         

		foreach(Chain chain in currentPack.chains)
		{
			GUILayout.BeginHorizontal ();
			GUILayout.BeginVertical ();
			GUILayout.BeginHorizontal ();
			chain.name = EditorGUILayout.TextArea (chain.name, GUILayout.Height(15));
			chain.returnAfterEnd = GUILayout.Toggle (chain.returnAfterEnd, "return");
			GUILayout.EndHorizontal ();

			if(GUILayout.Button("edit", GUILayout.Width(50), GUILayout.Height(15)))
			{
				currentChain = chain;
                inspectedPath = -1;
                inspectedState = null;
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
            inspectedPath = -1;
            inspectedState = null;
            currentPack.chains.Remove (deletingChain);
	    }

		GUILayout.EndVertical ();
		GUILayout.EndScrollView ();
	}


    void DrowChainsWindow ()
	{
        Undo.RecordObject(game, "chains");
        if (currentChain == null)
        {
            inspectedPath = -1;
            inspectedState = null;
            foreach (ChainPack pack in game.chainPacks)
            {
                foreach (Chain c in pack.chains)
                {
                    currentChain = c;        
                }
            }
            if (currentChain == null)
            {
                return;
            }
        }

        Rect fieldRect = new Rect(0, 30, position.width, position.height);
        GUI.DrawTextureWithTexCoords(fieldRect, BackgroundTexture, new Rect(0, 0, fieldRect.width / BackgroundTexture.width, fieldRect.height / BackgroundTexture.height));


        if (Event.current.type == EventType.MouseDown && Event.current.button == 2)
        {
            lastMousePosition = Event.current.mousePosition;
        }

        if (Event.current.type == EventType.MouseDrag && Event.current.button == 2)
        {
            Vector2 delta = Event.current.mousePosition - lastMousePosition;
            foreach (State s in currentChain.states)
            {
                s.position = new Rect(s.position.position+delta, s.position.size);
            }
            foreach (StateLink sl in currentChain.statesLinks)
            {
                sl.position = new Rect(sl.position.position + delta, sl.position.size);
            }
            lastMousePosition = Event.current.mousePosition;
            Repaint();
        }

        DrawAditional();

        BeginWindows ();

        if (makingPath && !makingPathRect.Contains(Event.current.mousePosition))
        {
            startPath.aimState = new State(-1);
        }

     

        foreach (State state in currentChain.states)
		{
			DrawStateBox (state);
		}


        foreach (StateLink stateLink in currentChain.StateLinks)
        {
            DrawStateLinkBox(stateLink);
        }
        

        if (inspectedState != null)
        {
            DrawPathWindow();
        }

        EndWindows ();


        Event evt = Event.current;

		if (evt.button == 1 && evt.type == EventType.MouseDown)
		{

			lastMousePosition = evt.mousePosition;


				GenericMenu menu = new GenericMenu();
				menu.AddItem(new GUIContent("Add state"), false, CreateNewState);
                menu.AddItem(new GUIContent("Add state link"), false, CreateNewStateLink);
                menu.ShowAsContext();
		}

        if (evt.button == 0 && evt.type == EventType.MouseUp)
        {
            makingPath = false;
        }

        Undo.FlushUndoRecordObjects();
    }

    

    private void RecalculateWindowsPositions()
    {
        if (currentChain!=null)
        {
            Vector2 delta;

            delta = position.center - currentChain.StartState.position.position;
            
            foreach (State s in currentChain.states)
            {
                s.position = new Rect(s.position.position + delta, s.position.size);
            }

            foreach (StateLink sl in currentChain.statesLinks)
            {
                sl.position = new Rect(sl.position.position + delta, sl.position.size);
            }
        }
    }

    void DrawAditional (){

        if (makingPath)
		{
			Handles.BeginGUI();
			Handles.color = Color.white;
            DrawNodeCurve(makingPathRect, new Rect(Event.current.mousePosition, Vector2.zero), Color.white);

            Handles.EndGUI();
            Repaint();
		}

        foreach (State state in currentChain.states)
        {
            int i = 0;
            foreach (Path path in state.pathes)
            {
                if (path.aimState != null && path.aimStateGuid!=-1)
                {
                    Handles.BeginGUI();
                    Handles.color = Color.red;

                    Rect end = path.aimState.position;

                    foreach (StateLink sl in currentChain.statesLinks)
                    {
                        if (sl.stateGUID == path.aimState.stateGUID)
                        {
                            end = sl.position;
                        }
                    }
                    DrawNodeCurve( new Rect(state.position.position + new Vector2(state.position.width + 15f, 7.5f + i * 19), Vector2.zero), end, Color.gray);
                    Handles.EndGUI();
                }
                i++;
            }
        }
    }

    void DrawStateLinkBox(StateLink stateLink)
    {
        Rect header = new Rect(stateLink.position.position, new Vector3(stateLink.position.width, 20));

        
        if (header.Contains(Event.current.mousePosition) && makingPath == true)
        {
            GUI.backgroundColor = Color.yellow;
            if (Event.current.button == 0 && Event.current.type == EventType.MouseUp)
            {
                if (GUIDManager.GetStateByGuid(stateLink.stateGUID)!=null)
                {
                    startPath.aimState = GUIDManager.GetStateByGuid(stateLink.stateGUID);
                }
                makingPath = false;
                Repaint();
            }
        }

        string title = "";
        if (GUIDManager.GetStateByGuid(stateLink.stateGUID)!=null)
        {
            title = GUIDManager.GetStateByGuid(stateLink.stateGUID).description;
        }
        stateLink.position = GUILayout.Window(currentChain.statesLinks.IndexOf(stateLink)+90000, stateLink.position, DoStateLinkWindow, title, GUILayout.Width(100), GUILayout.Height(60));   
        GUI.backgroundColor = Color.white;
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

        if (currentChain.StartState == state)
        {
            GUI.backgroundColor = Color.green;
        }

        if (header.Contains(Event.current.mousePosition) && makingPath == true)
        {
            GUI.backgroundColor = Color.yellow;
            if (Event.current.button == 0 && Event.current.type == EventType.MouseUp)
            {
                startPath.aimState = state;
                makingPath = false;
                Repaint();
            }
        }
				
        state.position = GUILayout.Window(currentChain.states.IndexOf(state), state.position, DoStateWindow, ss, GUILayout.Width(180), GUILayout.Height(120));

        int i = 0;

        Event c = Event.current;

        foreach (Path path in state.pathes)
        {
            Rect r = new Rect(state.position.x + state.position.width, state.position.y + 16 * i, 15, 15);
            GUI.backgroundColor = Color.white;
            if (inspectedState == state && inspectedPath == state.pathes.IndexOf(path))
            {
                GUI.backgroundColor = Color.gray;
            }
            GUI.Box(r, new GUIContent((Texture2D)Resources.Load("Icons/play-button") as Texture2D));

            if (r.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown)
            {
                makingPath = true;
                makingPathRect = r;
                startPath = path;
            }

            if (r.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseUp)
            {
                if (makingPath)
                {
                    if (inspectedState != state || inspectedPath != state.pathes.IndexOf(path))
                    {
                        inspectedPath = state.pathes.IndexOf(path);
                        inspectedState = state;
                        Repaint();
                    }
                    else
                    {
                        inspectedPath = -1;
                        Repaint();
                    }
                }
                makingPath = false;
            }
            i++;
        }

        GUI.backgroundColor = Color.white;
    }

	void DoStateWindow(int windowID)
	{
        Event evt = Event.current;

        if (evt.button == 1 && evt.type == EventType.MouseDown)
        {     
            menuState = currentChain.states[windowID];
            lastMousePosition = evt.mousePosition;
            GenericMenu menu = new GenericMenu();
            Undo.RecordObject(game, "chainsed");
            menu.AddItem(new GUIContent("Remove state"), false, RemoveState);
            menu.AddItem(new GUIContent("Add path"), false, AddPath);
            menu.AddItem(new GUIContent("Make start"), false, MakeStart);
            Undo.FlushUndoRecordObjects();
            menu.ShowAsContext();
        }

        GUILayout.BeginHorizontal ();
		GUILayout.BeginVertical ();
		currentChain.states [windowID].description = EditorGUILayout.TextArea (currentChain.states [windowID].description, GUILayout.Height(75), GUILayout.Width(170));
		currentChain.states [windowID].image = (Sprite)EditorGUILayout.ObjectField (currentChain.states [windowID].image, typeof(Sprite), false);
		GUILayout.EndVertical ();
		GUILayout.EndHorizontal ();
        GUI.DragWindow();
	}

    void DoStateLinkWindow(int windowID)
    {
        
        Event evt = Event.current;

        if (evt.button == 1 && evt.type == EventType.MouseDown)
        {
            menuStateLink = currentChain.statesLinks[windowID-90000];
            lastMousePosition = evt.mousePosition;
            GenericMenu menu = new GenericMenu();
            Undo.RecordObject(game, "remove link");
            menu.AddItem(new GUIContent("Remove state link"), false, RemoveStateLink);
            Undo.FlushUndoRecordObjects();
            menu.ShowAsContext();
        }
        

        GUILayout.BeginVertical();
        List<Chain> avaliableChains = new List<Chain>();
        foreach (ChainPack cp in game.chainPacks)
        {
            avaliableChains.AddRange(cp.chains);
        }

        avaliableChains.Remove(currentChain);

        if (avaliableChains.Count>0)
        {
            if (currentChain.statesLinks.Count>(windowID - 90000))
            {
                currentChain.statesLinks[windowID - 90000].chainGUID = avaliableChains[0].ChainGuid;
            }

            currentChain.statesLinks[windowID-90000].chainGUID = avaliableChains[EditorGUILayout.Popup(avaliableChains.IndexOf(GUIDManager.GetChainByGuid(currentChain.statesLinks[windowID-90000].chainGUID)), avaliableChains.Select(x => x.name).ToArray())].ChainGuid;

            List<State> avaliableStates = new List<State>();
            foreach (State  s in GUIDManager.GetChainByGuid(currentChain.statesLinks[windowID - 90000].chainGUID).states)
            {
                avaliableStates.Add(s);
            }

            if (GUIDManager.GetStateByGuid(currentChain.statesLinks[windowID-90000].stateGUID) == null || !avaliableStates.Contains(GUIDManager.GetStateByGuid(currentChain.statesLinks[windowID-90000].stateGUID)))
            {
                currentChain.statesLinks[windowID-90000].stateGUID = avaliableStates[0].stateGUID;
            }

            currentChain.statesLinks[windowID-90000].stateGUID = avaliableStates[EditorGUILayout.Popup(avaliableStates.IndexOf(GUIDManager.GetStateByGuid(currentChain.statesLinks[windowID - 90000].stateGUID)), avaliableStates.Select(x => x.description).ToArray())].stateGUID;
        }
        GUILayout.EndVertical();
        GUI.DragWindow();
    }

    private void RemoveStateLink()
    {
        foreach (State s in currentChain.states)
        {
            List<Path> removigPathes = new List<Path>();
            int i = 0;
            foreach (Path p in s.pathes)
            {
                if (p.aimState == GUIDManager.GetStateByGuid(menuStateLink.stateGUID))
                {
                    removigPathes.Add(s.pathes[i]);
                    inspectedPath = -1;
                }
                i++;
            }

            foreach (Path p in s.pathes.FindAll((p) => removigPathes.Contains(p)))
            {
                p.aimState = new State(-1);
            }
        }

        //inspectedState = null;
        currentChain.statesLinks.Remove(menuStateLink);
        Repaint();
        Debug.Log(currentChain.statesLinks.Count);
    }

    private void MakeStart()
    {
        currentChain.StartState = menuState;
    }

    private void RemoveState()
    {
        if (menuState == currentChain.StartState)
        {
            if (currentChain.states.Count == 1)
            {
                currentChain.StartState = CreateState();
            }
            else
            {
                foreach (State s in currentChain.states)
                {
                    if (s!=menuState)
                    {
                        currentChain.StartState = s;
                        break;
                    }
                }
            }
        }

        foreach (State s in currentChain.states)
        {
            List<Path> removigPathes = new List<Path>();
            int i = 0;
            foreach (Path p in s.pathes)
            {
                if (p.aimState == menuState)
                {
                    removigPathes.Add(s.pathes[i]);
                    inspectedPath = -1;
                }
                i++;
            }
            foreach(Path p in s.pathes.FindAll((p) => removigPathes.Contains(p)))
            {
                p.aimState = new State(-1);
            }
        }

        inspectedState = null;
        currentChain.states.Remove(menuState);
    }

    private void AddPath()
    {
        menuState.pathes.Add(new Path());
        inspectedPath = menuState.pathes.Count-1;
        inspectedState = menuState;
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
            GUILayout.Window(42, new Rect(inspectedState.position.x, inspectedState.position.y+ inspectedState.position.height, 100, 50), DoPathWindow, ss, GUILayout.Width(165), GUILayout.Height(50));
        }
    }

    private void DoPathWindow(int id)
    {
        GUILayout.BeginVertical(GUILayout.Width(171));
		GUILayout.BeginHorizontal ();
        inspectedState.pathes[inspectedPath].text = EditorGUILayout.TextArea(inspectedState.pathes[inspectedPath].text, GUILayout.Height(30), GUILayout.Width(171-25));
		GUI.color = Color.red;
		if(GUILayout.Button("",GUILayout.Width(20),GUILayout.Height(20)))
		{
			inspectedState.pathes.RemoveAt (inspectedPath);
			inspectedPath = -1;
			return;
		}
		GUI.color = Color.white;
		GUILayout.EndHorizontal ();
        inspectedState.pathes[inspectedPath].pathSprite = (Sprite)EditorGUILayout.ObjectField(inspectedState.pathes[inspectedPath].pathSprite, typeof(Sprite), false);
        inspectedState.pathes[inspectedPath].auto = GUILayout.Toggle(inspectedState.pathes[inspectedPath].auto, "auto", GUILayout.Width(60));


        inspectedState.pathes[inspectedPath].waitInput = GUILayout.Toggle(inspectedState.pathes[inspectedPath].waitInput, "wait input", GUILayout.Width(80));
        DrawCondition (inspectedState.pathes [inspectedPath]);
	
 
            
   
		if(inspectedState.pathes[inspectedPath].condition.Parameters.Count>0)
		{
			GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(1) });
		}
		DrawChanges (inspectedState.pathes[inspectedPath]);
		if(inspectedState.pathes[inspectedPath].changes.Count>0)
		{
			GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(1) });
		}
		DrawPileChangers (inspectedState.pathes[inspectedPath]);
        GUI.color = Color.white;
        GUILayout.EndVertical();
    }

    private void CreateNewState()
    {
        CreateState();
    }

    private void CreateNewStateLink()
    {
        CreateStateLink();
    }

    StateLink CreateStateLink()
    {
        List<Chain> chains = new List<Chain>();
        foreach (ChainPack cp in game.chainPacks)
        {
            chains.AddRange(cp.chains);
        }

        chains.Remove(currentChain);

        if (chains.Count>0)
        {
            StateLink link = new StateLink(chains[0]);
            link.position = new Rect(lastMousePosition.x - link.position.width / 2, lastMousePosition.y - link.position.height / 2, link.position.width, link.position.height);
            currentChain.statesLinks.Add(link);
            return link;
        }
        return null;
    }

    State CreateState()
	{
		State newState = new State(GUIDManager.GetStateGUID());
		newState.position = new Rect (lastMousePosition.x - newState.position.width/2, lastMousePosition.y - newState.position.height/2, newState.position.width, newState.position.height);
		currentChain.states.Add (newState);
        return newState;
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

        GUI.backgroundColor = Color.white;
        try
        {
            ExpressionSolver.CalculateBool(path.condition.conditionString, path.condition.Parameters);
        }
        catch
        {
            GUI.color = Color.red;
        }

		path.condition.conditionString = EditorGUILayout.TextArea (path.condition.conditionString, GUILayout.Width(171 - 25));
         
		GUI.color = Color.yellow;
		if (GUILayout.Button((Texture2D)Resources.Load("Icons/add") as Texture2D, GUILayout.Width(20), GUILayout.Height(20)))
		{
			if(game.parameters.Count>0)
			{
				path.condition.AddParam(game.parameters[0]);
			}
		}

		GUI.color = Color.white;
		EditorGUILayout.EndHorizontal ();

		Param removingParam = null;

		for(int i = 0;i<path.condition.Parameters.Count;i++)
		{
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField ("[p"+i+"]", GUILayout.Width(35));

			if(!game.parameters.Contains(path.condition.Parameters[i]))
			{
				if(game.parameters.Count>0){
					path.condition.Parameters [i] = game.parameters [0];
				}
				else{
					removingParam = path.condition.Parameters[i];
					continue;
				}
			}
			path.condition.setParam(i, game.parameters[EditorGUILayout.Popup (game.parameters.IndexOf(path.condition.Parameters[i]), game.parameters.Select (x => x.name).ToArray())]); 


			GUI.color = Color.red;
			if(GUILayout.Button("", GUILayout.Height(15), GUILayout.Width(15)))
			{
				removingParam = path.condition.Parameters[i];
			}
			GUI.color = Color.white;
			EditorGUILayout.EndHorizontal ();
		}

		if(removingParam!=null)
		{
			path.condition.RemoveParam (removingParam);
		}

	}

    private void DrawCondition(Condition condition)
    {
        GUILayout.BeginVertical();
        EditorGUILayout.BeginHorizontal();

        GUI.backgroundColor = Color.white;
        try
        {
            ExpressionSolver.CalculateBool(condition.conditionString, condition.Parameters);
        }
        catch
        {
            GUI.color = Color.red;
        }

        condition.conditionString = EditorGUILayout.TextArea(condition.conditionString, GUILayout.Width((position.width - 40) / 3 - 80));

        GUI.color = Color.yellow;
        if (GUILayout.Button((Texture2D)Resources.Load("Icons/add") as Texture2D, GUILayout.Width(20), GUILayout.Height(20)))
        {
            if (game.parameters.Count > 0)
            {
                condition.AddParam(game.parameters[0]);
            }
        }

        GUI.color = Color.white;
        EditorGUILayout.EndHorizontal();

        Param removingParam = null;

        for (int i = 0; i < condition.Parameters.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("[p" + i + "]", GUILayout.Width(35));

            if (!game.parameters.Contains(condition.Parameters[i]))
            {
                if (game.parameters.Count > 0)
                {
                    condition.Parameters[i] = game.parameters[0];
                }
                else
                {
                    removingParam = condition.Parameters[i];
                    Repaint();
                    continue;
                }
            }
            condition.setParam(i, game.parameters[EditorGUILayout.Popup(game.parameters.IndexOf(condition.Parameters[i]), game.parameters.Select(x => x.name).ToArray())]);


            GUI.color = Color.red;
            if (GUILayout.Button("", GUILayout.Height(15), GUILayout.Width(15)))
            {
                removingParam = condition.Parameters[i];
            }
            GUI.color = Color.white;
            EditorGUILayout.EndHorizontal();
        }

        if (removingParam != null)
        {
            condition.RemoveParam(removingParam);
        }
        GUILayout.EndVertical();
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
			path.changes [i].aimParam = game.parameters [EditorGUILayout.Popup (game.parameters.IndexOf (path.changes[i].aimParam), game.parameters.Select (x => x.name).ToArray ())]; 

			GUILayout.Label ("=");

            GUI.backgroundColor = Color.white;
            try
            {
                ExpressionSolver.CalculateFloat(path.changes[i].changeString, path.changes[i].parameters);
            }
            catch
            {
                GUI.color = Color.red;
            }

            path.changes [i].changeString = EditorGUILayout.TextArea(path.changes [i].changeString, GUILayout.Width(58));
			GUI.color = Color.red;
			if (GUILayout.Button ("", GUILayout.Height (15), GUILayout.Width (15))) {
				removingChanger = path.changes[i];
			}
			GUI.color = Color.yellow;
			if (GUILayout.Button ("", GUILayout.Height (15), GUILayout.Width (15))) {
				if (game.parameters.Count > 0) {
					path.changes[i].AddParam (game.parameters [0]);
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
						path.changes[i].setParam(game.parameters [0], j);
					} else {
						removingParam = path.changes[i].parameters [j];
						continue;
					}
				}

				int v = EditorGUILayout.Popup (game.parameters.IndexOf (path.changes[i].parameters [j]), game.parameters.Select (x => x.name).ToArray ());


                path.changes[i].setParam(game.parameters[v], j);
                

                GUI.color = Color.red;
				if (GUILayout.Button ("", GUILayout.Height (15), GUILayout.Width (15))) {
					removingParam = path.changes[i].parameters [j];
				}
				GUI.color = Color.white;
				EditorGUILayout.EndHorizontal ();
			}

			if (removingParam != null) {
				path.changes[i].RemoveParam (removingParam);
			}

			GUI.color = Color.white;

		}
		if(removingChanger!=null)
		{
			path.changes.Remove (removingChanger);
		}
	}

	private void DrawPileChangers (Path path)
	{
		GUI.color = new Color (0.8f, 0.5f, 0.8f);
		GUILayout.BeginHorizontal ();
		GUILayout.FlexibleSpace ();
		if (GUILayout.Button ((Texture2D)Resources.Load("Icons/add") as Texture2D, GUILayout.Width(20), GUILayout.Height(20))) 
		{
			path.pileChangers.Add (new PileChanger(game.chainPacks[0]));
		}
		GUILayout.EndHorizontal ();
		PileChanger removingChanger = null;
		GUI.color = Color.white;
		foreach(PileChanger pch in path.pileChangers)
		{
			GUILayout.BeginHorizontal ();
			pch.chainPileGUID = game.chainPacks[EditorGUILayout.Popup (game.chainPacks.IndexOf(GUIDManager.getChainPackByGuid(pch.chainPileGUID)), game.chainPacks.Select(x => x.name).ToArray())].ChainPackGUID;
			GUI.color = Color.red;
			if(GUILayout.Button("", GUILayout.Height(15), GUILayout.Width(15)))
			{
				removingChanger = pch;
			}
			GUI.color = Color.white;
			GUILayout.EndHorizontal ();
			pch.changeType = (PileChanger.ChangeType)EditorGUILayout.Popup ((int)pch.changeType, Enum.GetNames(typeof(PileChanger.ChangeType)));
			pch.aim = (PileChanger.PileChangeAim)EditorGUILayout.Popup ((int)pch.aim, Enum.GetNames(typeof(PileChanger.PileChangeAim)));
			pch.withRepeat = GUILayout.Toggle (pch.withRepeat, "dublicates");
			GUILayout.Space (2);
		}
			
		if(removingChanger!=null)
		{
			Debug.Log (")))))");
			path.pileChangers.Remove (removingChanger);
			Repaint ();
		}
		GUI.color = Color.white;
	}

    private void DrawChanges(ParamChanges change)
    {
        GUILayout.BeginVertical();
        GUI.color = Color.white;

            EditorGUILayout.BeginHorizontal();

            if (!game.parameters.Contains(change.aimParam))
            {
                if (game.parameters.Count > 0)
                {
                    change.aimParam = game.parameters[0];
                }
                else
                {
                    //removingChanger = path.changes[i];
                    EditorGUILayout.EndHorizontal();
                    return;
                }
            }
            change.aimParam = game.parameters[EditorGUILayout.Popup(game.parameters.IndexOf(change.aimParam), game.parameters.Select(x => x.name).ToArray())];

            GUILayout.Label("=");

            GUI.backgroundColor = Color.white;
            try
            {
                ExpressionSolver.CalculateFloat(change.changeString, change.parameters);
            }
            catch
            {
                GUI.color = Color.red;
            }

            change.changeString = EditorGUILayout.TextArea(change.changeString, GUILayout.Width(58));
            GUI.color = Color.yellow;
            if (GUILayout.Button("", GUILayout.Height(15), GUILayout.Width(15)))
            {
                if (game.parameters.Count > 0)
                {
                    change.AddParam(game.parameters[0]);
                }
            }
            GUI.color = Color.white;

            Param removingParam = null;
            EditorGUILayout.EndHorizontal();

            if (change.parameters == null)
            {
                change = new ParamChanges(change.aimParam);
            }

        for (int j = 0; j < change.parameters.Count; j++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("[p" + j + "]", GUILayout.Width(35));

                if (!game.parameters.Contains(change.parameters[j]))
                {
                    if (game.parameters.Count > 0)
                    {
                        change.setParam(game.parameters[0], j);
                    }
                    else
                    {
                        removingParam = change.parameters[j];
                        continue;
                    }
                }

                int v = EditorGUILayout.Popup(game.parameters.IndexOf(change.parameters[j]), game.parameters.Select(x => x.name).ToArray());
                change.setParam(game.parameters[v], j);
                GUI.color = Color.red;
                if (GUILayout.Button("", GUILayout.Height(15), GUILayout.Width(15)))
                {
                    removingParam = change.parameters[j];
                }
                GUI.color = Color.white;
                EditorGUILayout.EndHorizontal();
            }

            if (removingParam != null)
            {
                change.RemoveParam(removingParam);
            }
            GUI.color = Color.white;
        GUILayout.EndVertical();
    }

    void DrawNodeCurve(Rect start, Rect end, Color c)
    {
        Vector3 startPos = new Vector3(start.x + start.width, start.y + start.height / 2, 0);
        Vector3 endPos = new Vector3(end.x, end.y + end.height / 2, 0);
        Vector3 startTan = startPos + Vector3.right * 150;
        Vector3 endTan = endPos + Vector3.left * 150;
        Color shadowCol = new Color(0, 0, 0, 0.06f);
        for (int i = 0; i < 2; i++) // Draw a shadow
        Handles.DrawBezier(startPos, endPos, startTan, endTan, shadowCol, null, (i + 1) * 7);
        Handles.DrawBezier(startPos, endPos, startTan, endTan, c, null, 3);
    }
}
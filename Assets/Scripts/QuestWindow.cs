using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditorInternal;


public class MyWindow : EditorWindow
{
	private GUISkin QuestCreatorSkin
	{
		get
		{
			return Resources.Load ("Skins/QuestEditorSkin") as GUISkin;
		}
	}

	Vector2 lastMousePosition;
	List<ChainPack> chainPacks = new List<ChainPack>();
	Chain currentChain;
	ChainPack currentPack;
	ReorderableList packList;
	string[] toolbarStrings = new string[] {"Packs", "Chains"};
	int toolbarInt = 0, selectedChain = 0;
	Vector2 packsScrollPosition = Vector2.zero;
	Vector2 chainsScrollPosition = Vector2.zero;
	bool chainEditorMode = false;
	Rect makingPathRect = new Rect(Vector2.one*0.12345f, Vector2.one*0.12345f);
    bool makingPath = false;
    State pathAimState;
    Path startPath;

	[MenuItem("Window/Quest creator")]
	static void Init()
	{
		MyWindow window = (MyWindow)EditorWindow.GetWindow(typeof(MyWindow), false,  "Quest creator", true); 
		window.Show();
	}

	void OnGUI()
	{
        GUILayout.BeginVertical ();
		if (!chainEditorMode) {
			DrowPacksWindow ();
		} else {
			DrowChainsWindow ();
		} 
        GUILayout.EndVertical();
       
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
		packsScrollPosition = GUILayout.BeginScrollView (packsScrollPosition, false, true, GUILayout.Width(position.width/2+5), GUILayout.Height(position.height-5));
		GUILayout.BeginVertical();
		ChainPack deletingPack = null;

		GUI.color = Color.green;
		if(GUILayout.Button("new pack", GUILayout.Width(position.width/2-30), GUILayout.Height(15)))
		{
			currentPack = new ChainPack ();
			chainPacks.Insert(0, currentPack);
			GUI.FocusControl ("packName"+(0));
		}
		GUI.color = Color.white;
	
		int counter = 1;
		foreach(ChainPack pack in chainPacks)
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
				if(chainPacks.IndexOf(deletingPack)==0){
					if (chainPacks.Count > 1) {
						currentPack = chainPacks [1];
					} else {
						currentPack = null;
					}
				}
				else
				{
					currentPack = chainPacks [0];
				}
			}

			chainPacks.Remove (deletingPack);

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

		chainsScrollPosition = GUILayout.BeginScrollView (chainsScrollPosition, false, true, GUILayout.Width(position.width/2-5), GUILayout.Height(position.height-5));
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
				chainEditorMode = true;
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

        GUILayout.BeginVertical ();
		if (GUILayout.Button ((Texture2D)Resources.Load("Icons/cancel") as Texture2D, GUILayout.Width(30), GUILayout.Height(30))) 
		{
			chainEditorMode = false;
		}
        GUILayout.EndVertical();

       

        BeginWindows ();
        foreach (State state in currentChain.states)
		{
			DrawStateBox (state);
		}
        EndWindows ();


        DrawAditional();


        Event evt = Event.current;

		if (evt.button == 1)
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
		string ss = state.description.Split(new string[] {"\n"}, System.StringSplitOptions.RemoveEmptyEntries)[0];
		ss = ss.Substring (0, Mathf.Min (20, ss.Length));

        if (state.position.Contains(Event.current.mousePosition) && makingPath == true)
        {
            GUI.backgroundColor = Color.yellow;
            pathAimState = state;
        }
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
            GUILayout.Button("", GUILayout.Width(20), GUILayout.Height(20)); 
            if(GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition) && Event.current.button == 0 && makingPath == false)
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

	void CreateState()
	{
		State newState = new State();
		newState.position = new Rect (lastMousePosition.x - newState.position.width/2, lastMousePosition.y - newState.position.height/2, newState.position.width, newState.position.height);
		currentChain.states.Add (newState);
	}
}
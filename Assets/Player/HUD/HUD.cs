﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RTS;

public class HUD : MonoBehaviour
{

		public GUISkin resourceSkin, ordersSkin, selectBoxSkin, mouseCursorSkin;
		private const int ORDERS_BAR_WIDTH = 150, RESOURCE_BAR_HEIGHT = 40;
		private Player player;
		private const int SELECTION_NAME_HEIGHT = 30;
		
		//For resourceBar
		private Dictionary< ResourceType, int > resourceValues, resourceLimits;
		private const int ICON_WIDTH = 32, ICON_HEIGHT = 32, TEXT_WIDTH = 128, TEXT_HEIGHT = 32;
		public Texture2D[] resources;
		private Dictionary< ResourceType, Texture2D > resourceImages;
		//cursors

		public Texture2D activeCursor;
		public Texture2D selectCursor, leftCursor, rightCursor, upCursor, downCursor;
		public Texture2D[] moveCursors, attackCursors, harvestCursors; //size declared in unity main window (current size 2)
		private CursorState activeCursorState;
		private int currentFrame = 0;


	//order bar
	//incase of more orders than can fit on screen implement a slider
	private WorldObject lastSelection;
	private float sliderValue; 

		void Start ()
		{
				player = transform.root.GetComponent< Player > ();
				ResourceManager.StoreSelectBoxItems (selectBoxSkin);
				SetCursorState (CursorState.Select);

				resourceValues = new Dictionary< ResourceType, int > ();
				resourceLimits = new Dictionary< ResourceType, int > ();
		//reourcebar
		resourceImages = new Dictionary< ResourceType, Texture2D >();
		for(int i = 0; i < resources.Length; i++) {
			switch(resources[i].name) {
			case "Money":
				resourceImages.Add(ResourceType.Money, resources[i]);
				resourceValues.Add(ResourceType.Money, 0);
				resourceLimits.Add(ResourceType.Money, 0);
				break;
			case "Power":
				resourceImages.Add(ResourceType.Power, resources[i]);
				resourceValues.Add(ResourceType.Power, 0);
				resourceLimits.Add(ResourceType.Power, 0);
				break;
			default: break;
			}
		}

		}
	
		void OnGUI ()
		{
				if (player && player.human) {
						DrawOrdersBar ();
						DrawResourceBar ();
						DrawMouseCursor ();
				}
		}

		public bool MouseInBounds ()
		{
				//Screen coordinates start in the lower-left corner of the screen
				//not the top-left of the screen like the drawing coordinates do
				Vector3 mousePos = Input.mousePosition;
				bool insideWidth = mousePos.x >= 0 && mousePos.x <= Screen.width - ORDERS_BAR_WIDTH;
				bool insideHeight = mousePos.y >= 0 && mousePos.y <= Screen.height - RESOURCE_BAR_HEIGHT;
				return insideWidth && insideHeight;
		}
		//area of screen not taken by hud
		public Rect GetPlayingArea ()
		{
				return new Rect (0, RESOURCE_BAR_HEIGHT, Screen.width - ORDERS_BAR_WIDTH, Screen.height - RESOURCE_BAR_HEIGHT);
		}

		//sets the values of resources to display
		public void SetResourceValues (Dictionary< ResourceType, int > resourceValues, Dictionary< ResourceType, int > resourceLimits)
		{
				this.resourceValues = resourceValues;
				this.resourceLimits = resourceLimits;
		}



		/*** Private Worker Methods ***/


	#region Cursors
		private void DrawMouseCursor ()
		{
				bool mouseOverHud = !MouseInBounds () && activeCursorState != CursorState.PanRight && activeCursorState != CursorState.PanUp; //Checks if the mouse cursor is in the HUD

				if (mouseOverHud) {
						Screen.showCursor = true;
				} else {
						Screen.showCursor = false;
						GUI.skin = mouseCursorSkin;
						GUI.BeginGroup (new Rect (0, 0, Screen.width, Screen.height));
						UpdateCursorAnimation ();
						Rect cursorPosition = GetCursorDrawPosition ();
						GUI.Label (cursorPosition, activeCursor);
						GUI.EndGroup ();
				}
		}

		private void UpdateCursorAnimation ()
		{
				//sequence animation for cursor (based on more than one image for the cursor)
				//change once per second, loops through array of images
				if (activeCursorState == CursorState.Move) {
						currentFrame = (int)Time.time % moveCursors.Length;
						activeCursor = moveCursors [currentFrame];
				} else if (activeCursorState == CursorState.Attack) {
						currentFrame = (int)Time.time % attackCursors.Length;
						activeCursor = attackCursors [currentFrame];
				} else if (activeCursorState == CursorState.Harvest) {
						currentFrame = (int)Time.time % harvestCursors.Length;
						activeCursor = harvestCursors [currentFrame];
				}
		}

		private Rect GetCursorDrawPosition ()
		{
				//set base position for custom cursor image
				float leftPos = Input.mousePosition.x;
				float topPos = Screen.height - Input.mousePosition.y; //screen draw coordinates are inverted
				//adjust position base on the type of cursor being shown
				if (activeCursorState == CursorState.PanRight)
						leftPos = Screen.width - activeCursor.width;
				else if (activeCursorState == CursorState.PanDown)
						topPos = Screen.height - activeCursor.height;
				else if (activeCursorState == CursorState.Move || activeCursorState == CursorState.Select || activeCursorState == CursorState.Harvest) {
						topPos -= activeCursor.height / 2;
						leftPos -= activeCursor.width / 2;
				}
				return new Rect (leftPos, topPos, activeCursor.width, activeCursor.height);
		}

		public void SetCursorState (CursorState newState)
		{
				activeCursorState = newState;
				switch (newState) {
				case CursorState.Select:
						activeCursor = selectCursor;
						break;
				case CursorState.Attack:
						currentFrame = (int)Time.time % attackCursors.Length;
						activeCursor = attackCursors [currentFrame];
						break;
				case CursorState.Harvest:
						currentFrame = (int)Time.time % harvestCursors.Length;
						activeCursor = harvestCursors [currentFrame];
						break;
				case CursorState.Move:
						currentFrame = (int)Time.time % moveCursors.Length;
						activeCursor = moveCursors [currentFrame];
						break;
				case CursorState.PanLeft:
						activeCursor = leftCursor;
						break;
				case CursorState.PanRight:
						activeCursor = rightCursor;
						break;
				case CursorState.PanUp:
						activeCursor = upCursor;
						break;
				case CursorState.PanDown:
						activeCursor = downCursor;
						break;
				default:
						break;
				}
		}

	#endregion

		private void DrawOrdersBar ()
		{
				GUI.skin = ordersSkin;
				GUI.BeginGroup (new Rect (Screen.width - ORDERS_BAR_WIDTH, RESOURCE_BAR_HEIGHT, ORDERS_BAR_WIDTH, Screen.height - RESOURCE_BAR_HEIGHT));
				GUI.Box (new Rect (0, 0, ORDERS_BAR_WIDTH, Screen.height - RESOURCE_BAR_HEIGHT), "");
				string selectionName = "";
				if (player.SelectedObject) {
						selectionName = player.SelectedObject.objectName;
				}
				if (!selectionName.Equals ("")) {
						GUI.Label (new Rect (0, 10, ORDERS_BAR_WIDTH, SELECTION_NAME_HEIGHT), selectionName);
				}

		if(player.SelectedObject.IsOwnedBy(player)) {
			//reset slider value if the selected object has changed
			if(lastSelection && lastSelection != player.SelectedObject) sliderValue = 0.0f;
			DrawActions(player.SelectedObject.GetActions());
			//store the current selection
			lastSelection = player.SelectedObject;
		}


				GUI.EndGroup ();
		}



	private void DrawActions(string[] actions) {
		GUIStyle buttons = new GUIStyle();
		buttons.hover.background = buttonHover;
		buttons.active.background = buttonClick;
		GUI.skin.button = buttons;
		int numActions = actions.Length;
		//define the area to draw the actions inside
		GUI.BeginGroup(new Rect(0, 0, ORDERS_BAR_WIDTH, buildAreaHeight));
		//draw scroll bar for the list of actions if need be
		if(numActions <= MaxNumRows(buildAreaHeight)) DrawSlider(buildAreaHeight, numActions / 2.0f);
		//display possible actions as buttons and handle the button click for each
		for(int i = 0; i < numActions; i++) {
			int column = i % 2;
			int row = i / 2;
			Rect pos = GetButtonPos(row, column);
			Texture2D action = ResourceManager.GetBuildImage(actions[i]);
			if(action) {
				//create the button and handle the click of that button
				if(GUI.Button(pos, action)) {
					if(player.SelectedObject) player.SelectedObject.PerformAction(actions[i]);
				}
			}
		}
		GUI.EndGroup();
	}





		private void DrawResourceBar ()
		{
				GUI.skin = resourceSkin;
				GUI.BeginGroup (new Rect (0, 0, Screen.width, RESOURCE_BAR_HEIGHT));
				GUI.Box (new Rect (0, 0, Screen.width, RESOURCE_BAR_HEIGHT), "");

				int topPos = 4, iconLeft = 4, textLeft = 20;
				DrawResourceIcon (ResourceType.Money, iconLeft, textLeft, topPos);
				iconLeft += TEXT_WIDTH;
				textLeft += TEXT_WIDTH;
				DrawResourceIcon (ResourceType.Power, iconLeft, textLeft, topPos);

				GUI.EndGroup ();
		}
	//to change where diplayed, change values in DrawResourceBar
	private void DrawResourceIcon(ResourceType type, int iconLeft, int textLeft, int topPos) {
		Texture2D icon = resourceImages[type];
		string text = resourceValues[type].ToString() + "/" + resourceLimits[type].ToString();
		GUI.DrawTexture(new Rect(iconLeft, topPos, ICON_WIDTH, ICON_HEIGHT), icon);
		GUI.Label (new Rect(textLeft, topPos, TEXT_WIDTH, TEXT_HEIGHT), text);

	}






}










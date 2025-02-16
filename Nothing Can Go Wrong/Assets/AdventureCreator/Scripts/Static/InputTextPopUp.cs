/*
*
*	Adventure Creator
*	by Chris Burton, 2013-2024
*	
*	"InputTextPopUp.cs"
* 
*	This script provides a small popup-window that lets the user enter some text.
* 
*/

#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEngine;

namespace AC
{
		
	public class InputTextPopUp : PopupWindowContent
	{

		#region Variables

		private string inputText, originalText;
		private Rect rect;
		private System.Action<string> onSet;

		#endregion


		#region Constructors

		public InputTextPopUp (string _inputText, Rect _rect, System.Action<string> _onSet)
		{
			inputText = originalText = _inputText;
			rect = _rect;
			onSet = _onSet;
		}

		#endregion


		#region PublicFunctions

		public override Vector2 GetWindowSize ()
		{
			editorWindow.position = rect;
			return new Vector2 (rect.width, rect.height);
		}


		public override void OnGUI (Rect rect)
		{
			var e = Event.current;
        	if (e.type == EventType.KeyDown)
        	{
				switch (e.keyCode)
				{
					// Escape pressed
					case KeyCode.Escape:
						inputText = originalText;
						editorWindow.Close ();
						e.Use();
						break;
	
					// Enter pressed
					case KeyCode.Return:
					case KeyCode.KeypadEnter:
						editorWindow.Close ();
						e.Use();
						break;

					default:
						break;
				}
			}

			GUI.SetNextControlName ("inText");
			inputText = EditorGUILayout.TextField ("", inputText);
			GUI.FocusControl ("inText");
		}


		public override void OnClose ()
		{
			onSet?.Invoke (inputText);
		}

		#endregion

	}

}

#endif
#if UNITY_2019_4_OR_NEWER && UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System.Linq;
using UnityEditor;

namespace AC
{

	public class EventsEditorData
	{

		private Vector2 scrollPos;
		private EventTypeReference[] eventTypeReferences;
		private int selectedReference;

		private int selectedIndex;
		private int sideEvent = -1;
		private bool showEventsList = true;
		private bool showSelectedEvent = true;


		public void ShowGUI (List<EventBase> events, ActionListSource source, UnityEngine.Object sourceObject)
		{
			ShowEventsGUI (events, source, sourceObject);

			if (GUI.changed)
			{
				UnityVersionHandler.CustomSetDirty (sourceObject);
			}
		}


		private void ShowEventsGUI (List<EventBase> events, ActionListSource source, UnityEngine.Object sourceObject)
		{
			if (eventTypeReferences == null || eventTypeReferences.Length == 0)
			{
				GenerateTypeReferencesArray ();
			}

			showEventsList = CustomGUILayout.ToggleHeader (showEventsList, "Events");
			CustomGUILayout.BeginVertical ();
			if (showEventsList)
			{
				scrollPos = EditorGUILayout.BeginScrollView (scrollPos);
				for (int i = 0; i < events.Count; i++)
				{
					if (events[i] == null) continue;

					EditorGUILayout.BeginHorizontal ();

					if (GUILayout.Toggle (selectedIndex == i, events[i].ID + ": " + events[i].Label, "Button"))
					{
						if (selectedIndex != i)
						{
							DeactivateAllEvents ();
							ActivateEvent (i);
						}
					}

					if (GUILayout.Button (string.Empty, CustomStyles.IconCog))
					{
						SideMenu (i, events, sourceObject);
					}

					EditorGUILayout.EndHorizontal ();
				}

				EditorGUILayout.EndScrollView ();

				EditorGUILayout.Space ();
				EditorGUILayout.Space ();

				string[] typeLabels = new string[eventTypeReferences.Length];
				for (int i = 0; i < typeLabels.Length; i++) typeLabels[i] = eventTypeReferences[i].MenuName;
				EditorGUILayout.BeginHorizontal ();
				selectedReference = EditorGUILayout.Popup ("New event:", selectedReference, typeLabels);
				if (CustomGUILayout.ClickedCreateButton ())
				{
					Undo.RecordObject (sourceObject, "Create new event");

					EventBase newEvent = (EventBase) Activator.CreateInstance (eventTypeReferences[selectedReference].Type);
					newEvent.AssignID (GetUniqueID (events));
					newEvent.AssignVariant (eventTypeReferences[selectedReference].Variant);
					events.Add (newEvent);

					if (events.Count > 1)
					{
						DeactivateAllEvents ();
						ActivateEvent (events.Count-1);
					}
				}

				if (events.Count > 1)
				{
					if (GUILayout.Button (string.Empty, CustomStyles.IconCog))
					{
						GlobalSideMenu (events, sourceObject);
					}
				}
				EditorGUILayout.EndHorizontal ();
			}

			CustomGUILayout.EndVertical ();
			EditorGUILayout.Space ();

			if (selectedIndex >= 0 && selectedIndex < events.Count)
			{
				if (events[selectedIndex] == null) return;

				showSelectedEvent = CustomGUILayout.ToggleHeader (showSelectedEvent, "Event #" + events[selectedIndex].ID + ": " + events[selectedIndex].Label);
				if (showSelectedEvent)
				{
					CustomGUILayout.BeginVertical ();
					bool isAssetFile = sourceObject is ScriptableObject;
					events[selectedIndex].ShowGUI (isAssetFile, isAssetFile ? ActionListSource.AssetFile : source);
					CustomGUILayout.EndVertical ();
				}
			}
		}


		private void SideMenu (int index, List<EventBase> events, UnityEngine.Object sourceObject)
		{
			GenericMenu menu = new GenericMenu ();
			sideEvent = index;

			if (events.Count > 0)
			{
				menu.AddItem (new GUIContent ("Delete"), false, Callback, new GenericMenuData ("Delete", events, sourceObject));
			}
			if (sideEvent > 0 || sideEvent < events.Count - 1)
			{
				menu.AddSeparator (string.Empty);
			}
			if (sideEvent > 0)
			{
				menu.AddItem (new GUIContent ("Re-arrange/Move to top"), false, Callback, new GenericMenuData ("Move to top", events, sourceObject));
				menu.AddItem (new GUIContent ("Re-arrange/Move up"), false, Callback, new GenericMenuData ("Move up", events, sourceObject));
			}
			if (sideEvent < events.Count - 1)
			{
				menu.AddItem (new GUIContent ("Re-arrange/Move down"), false, Callback, new GenericMenuData ("Move down", events, sourceObject));
				menu.AddItem (new GUIContent ("Re-arrange/Move to bottom"), false, Callback, new GenericMenuData ("Move to bottom", events, sourceObject));
			}

			menu.ShowAsContext ();
		}


		private class GenericMenuData
		{

			public readonly string command;
			public readonly List<EventBase> events;
			public readonly UnityEngine.Object sourceObject;


			public GenericMenuData (string _command, List<EventBase> _events, UnityEngine.Object _sourceObject)
			{
				command = _command;
				events = _events;
				sourceObject = _sourceObject;
			}
		}


		private void Callback (object obj)
		{
			if (sideEvent >= 0)
			{
				GenericMenuData data = (GenericMenuData) obj;
				EventBase tempEvent = data.events[sideEvent];

				switch (data.command)
				{
					case "Delete":
						Undo.RecordObject (data.sourceObject, "Delete Event");
						if (sideEvent == selectedIndex)
						{
							DeactivateAllEvents ();
						}
						data.events.RemoveAt (sideEvent);
						break;

					case "Move up":
						Undo.RecordObject (data.sourceObject, "Move Event up");
						data.events.RemoveAt (sideEvent);
						data.events.Insert (sideEvent - 1, tempEvent);
						break;

					case "Move down":
						Undo.RecordObject (data.sourceObject, "Move Event down");
						data.events.RemoveAt (sideEvent);
						data.events.Insert (sideEvent + 1, tempEvent);
						break;

					case "Move to top":
						Undo.RecordObject (data.sourceObject, "Move Event to top");
						data.events.RemoveAt (sideEvent);
						data.events.Insert (0, tempEvent);
						break;

					case "Move to bottom":
						Undo.RecordObject (data.sourceObject, "Move Event to bottom");
						data.events.Add (tempEvent);
						data.events.RemoveAt (sideEvent);
						break;
				}

				UnityVersionHandler.CustomSetDirty (data.sourceObject);
				AssetDatabase.SaveAssets ();
			}

			sideEvent = -1;
		}


		private void GlobalSideMenu (List<EventBase> events, UnityEngine.Object sourceObject)
		{
			GenericMenu menu = new GenericMenu ();
			menu.AddItem (new GUIContent ("Delete all"), false, GlobalCallback, new GenericMenuData ("Delete all", events, sourceObject));
			menu.ShowAsContext ();
		}


		private void GlobalCallback (object obj)
		{
			GenericMenuData data = (GenericMenuData) obj;

			switch (data.command)
			{
				case "Delete all":
					Undo.RecordObject (data.sourceObject, "Delete all Events");
					DeactivateAllEvents ();
					data.events.Clear ();
					break;

				default:
					break;
			}

			UnityVersionHandler.CustomSetDirty (data.sourceObject);
			AssetDatabase.SaveAssets ();
		}


		private void GenerateTypeReferencesArray ()
		{
			Assembly assembly = typeof (EventBase).Assembly;
			var types = assembly.GetTypes ().Where (t => t.BaseType == typeof (EventBase));

			List<EventTypeReference> eventTypeReferencesList = new List<EventTypeReference> ();
			foreach (Type type in types)
			{
				EventBase eventBase = (EventBase) Activator.CreateInstance (type);

				for (int i = 0; i < eventBase.EditorNames.Length; i++)
				{
					EventTypeReference reference = new EventTypeReference (eventBase.EditorNames[i], eventBase.GetType (), i);
					eventTypeReferencesList.Add (reference);
				}
			}

			eventTypeReferencesList.Sort (delegate (EventTypeReference a, EventTypeReference b) { return a.MenuName.CompareTo (b.MenuName); });
			eventTypeReferences = eventTypeReferencesList.ToArray ();
		}


		private void DeactivateAllEvents ()
		{
			selectedIndex = -1;
		}


		private void ActivateEvent (int index)
		{
			selectedIndex = index;
			EditorGUIUtility.editingTextField = false;
		}


		private int GetUniqueID (List<EventBase> events)
		{
			List<int> idArray = new List<int> ();
			foreach (EventBase _event in events)
			{
				if (_event == null) continue;
				idArray.Add (_event.ID);
			}
			idArray.Sort ();

			int newID = 0;

			foreach (int _id in idArray)
			{
				if (newID == _id)
					newID++;
			}
			return newID;
		}


		private class EventTypeReference
		{

			public readonly string MenuName;
			public readonly Type Type;
			public readonly int Variant;

			public EventTypeReference (string menuName, Type type, int variant)
			{
				MenuName = menuName;
				Type = type;
				Variant = variant;
			}

		}

	}

}

#endif
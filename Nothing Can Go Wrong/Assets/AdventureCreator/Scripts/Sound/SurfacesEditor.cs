#if UNITY_EDITOR

using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

namespace AC
{

	public class SurfacesEditor : EditorWindow
	{

		private SettingsManager settingsManager;
		private Vector2 scrollPos;
		private Surface selectedSurface;
		private int sideSurface = -1;

		private bool showSurfacesList = true;
		private bool showSelectedSurface = true;


		public static void Init ()
		{
			SurfacesEditor window = (SurfacesEditor) GetWindow (typeof (SurfacesEditor));
			window.titleContent.text = "Surfaces";
			window.position = new Rect (300, 200, 450, 490);
			window.minSize = new Vector2 (300, 180);
		}


		private void OnGUI ()
		{
			settingsManager = KickStarter.settingsManager;
			if (settingsManager == null)
			{
				EditorGUILayout.HelpBox ("A Settings Manager must be assigned before this window can display correctly.", MessageType.Warning);
				return;
			}

			EditorGUILayout.LabelField ("Surfaces", CustomStyles.managerHeader);

			ShowSurfacesGUI ();

			UnityVersionHandler.CustomSetDirty (settingsManager);
		}


		private void ShowSurfacesGUI ()
		{
			showSurfacesList = CustomGUILayout.ToggleHeader (showSurfacesList, "Surfaces");
			CustomGUILayout.BeginVertical ();
			if (showSurfacesList)
			{
				scrollPos = EditorGUILayout.BeginScrollView (scrollPos);
				foreach (Surface surface in settingsManager.surfaces)
				{
					EditorGUILayout.BeginHorizontal ();

					if (GUILayout.Toggle (selectedSurface == surface, surface.ID + ": " + surface.label, "Button"))
					{
						if (selectedSurface != surface)
						{
							DeactivateAllSurfaces ();
							ActivateSurface (surface);
						}
					}

					if (GUILayout.Button (string.Empty, CustomStyles.IconCog))
					{
						SideMenu (surface);
					}

					EditorGUILayout.EndHorizontal ();
				}
				EditorGUILayout.EndScrollView ();

				EditorGUILayout.Space ();
				EditorGUILayout.BeginHorizontal ();
				if (GUILayout.Button ("Create new Surface"))
				{
					Undo.RecordObject (this, "Create new Surface");

					if (settingsManager.surfaces.Count > 0)
					{
						List<int> idArray = new List<int> ();
						foreach (Surface surface in settingsManager.surfaces)
						{
							idArray.Add (surface.ID);
						}
						idArray.Sort ();

						Surface newSurface = new Surface (idArray.ToArray ());
						settingsManager.surfaces.Add (newSurface);

						DeactivateAllSurfaces ();
						ActivateSurface (newSurface);
					}
					else
					{
						Surface newSurface = new Surface (0);
						settingsManager.surfaces.Add (newSurface);
					}
				}

				if (settingsManager.surfaces.Count > 1)
				{
					if (GUILayout.Button (string.Empty, CustomStyles.IconCog))
					{
						GlobalSideMenu ();
					}
				}
				EditorGUILayout.EndHorizontal ();
			}

			CustomGUILayout.EndVertical ();
			EditorGUILayout.Space ();

			if (selectedSurface != null && settingsManager.surfaces.Contains (selectedSurface))
			{
				showSelectedSurface = CustomGUILayout.ToggleHeader (showSelectedSurface, "Surface #" + selectedSurface.ID + ": " + selectedSurface.label);
				if (showSelectedSurface)
				{
					CustomGUILayout.BeginVertical ();
					selectedSurface.ShowGUI ();
					CustomGUILayout.EndVertical ();
				}
			}
		}


		private void SideMenu (Surface surface)
		{
			GenericMenu menu = new GenericMenu ();
			sideSurface = settingsManager.surfaces.IndexOf (surface);

			menu.AddItem (new GUIContent ("Insert after"), false, Callback, "Insert after");
			if (settingsManager.surfaces.Count > 0)
			{
				menu.AddItem (new GUIContent ("Delete"), false, Callback, "Delete");
			}
			if (sideSurface > 0 || sideSurface < settingsManager.surfaces.Count - 1)
			{
				menu.AddSeparator (string.Empty);
			}
			if (sideSurface > 0)
			{
				menu.AddItem (new GUIContent ("Re-arrange/Move to top"), false, Callback, "Move to top");
				menu.AddItem (new GUIContent ("Re-arrange/Move up"), false, Callback, "Move up");
			}
			if (sideSurface < settingsManager.surfaces.Count - 1)
			{
				menu.AddItem (new GUIContent ("Re-arrange/Move down"), false, Callback, "Move down");
				menu.AddItem (new GUIContent ("Re-arrange/Move to bottom"), false, Callback, "Move to bottom");
			}

			menu.ShowAsContext ();
		}


		private void Callback (object obj)
		{
			if (sideSurface >= 0)
			{
				Surface tempSurface = settingsManager.surfaces[sideSurface];

				switch (obj.ToString ())
				{
					case "Insert after":
						Undo.RecordObject (settingsManager, "Insert Surface");
						settingsManager.surfaces.Insert (sideSurface + 1, new Surface (GetIDList ().ToArray ()));
						break;

					case "Delete":
						Undo.RecordObject (settingsManager, "Delete Surface");
						if (tempSurface == selectedSurface)
						{
							DeactivateAllSurfaces ();
						}
						settingsManager.surfaces.RemoveAt (sideSurface);
						break;

					case "Move up":
						Undo.RecordObject (settingsManager, "Move Surface up");
						settingsManager.surfaces.RemoveAt (sideSurface);
						settingsManager.surfaces.Insert (sideSurface - 1, tempSurface);
						break;

					case "Move down":
						Undo.RecordObject (settingsManager, "Move Surface down");
						settingsManager.surfaces.RemoveAt (sideSurface);
						settingsManager.surfaces.Insert (sideSurface + 1, tempSurface);
						break;

					case "Move to top":
						Undo.RecordObject (settingsManager, "Move Surface to top");
						settingsManager.surfaces.RemoveAt (sideSurface);
						settingsManager.surfaces.Insert (0, tempSurface);
						break;

					case "Move to bottom":
						Undo.RecordObject (settingsManager, "Move Surface to bottom");
						settingsManager.surfaces.Add (tempSurface);
						settingsManager.surfaces.RemoveAt (sideSurface);
						break;
				}
			}

			EditorUtility.SetDirty (settingsManager);
			AssetDatabase.SaveAssets ();

			sideSurface = -1;
		}


		private void GlobalSideMenu ()
		{
			GenericMenu menu = new GenericMenu ();
			menu.AddItem (new GUIContent ("Delete all"), false, GlobalCallback, "Delete all");
			menu.ShowAsContext ();
		}


		private void GlobalCallback (object obj)
		{
			switch (obj.ToString ())
			{
				case "Delete all":
					Undo.RecordObject (settingsManager, "Delete all Surfaces");
					selectedSurface = null;
					settingsManager.surfaces.Clear ();
					break;

				default:
					break;
			}

			EditorUtility.SetDirty (settingsManager);
			AssetDatabase.SaveAssets ();
		}


		private void DeactivateAllSurfaces ()
		{
			selectedSurface = null;
		}


		private List<int> GetIDList ()
		{
			List<int> idList = new List<int> ();
			foreach (Surface surface in settingsManager.surfaces)
			{
				idList.Add (surface.ID);
			}

			idList.Sort ();

			return idList;
		}


		private void ActivateSurface (Surface surface)
		{
			selectedSurface = surface;
			EditorGUIUtility.editingTextField = false;
		}

	}

}

#endif
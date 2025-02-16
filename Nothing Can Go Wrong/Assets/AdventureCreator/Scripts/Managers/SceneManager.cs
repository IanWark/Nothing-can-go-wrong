/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2024
 *	
 *	"SceneManager.cs"
 * 
 *	This script handles the "Scene" tab of the main wizard.
 *	It is used to create the prefabs needed to run the game,
 *	as well as provide easy-access to game logic.
 * 
 */
#if UNITY_2018_3_OR_NEWER
#define NEW_PREFABS
#endif

using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
using System;
using UnityEditor;
#endif


namespace AC
{

	/**
	 * Handles the "Scene" tab of the Game Editor window.
	 * It doesn't store any data from the scene itself, only references objects that do.
	 * It provides a list of Adventure Creator prefabs that can be created and managed.
	 */
	[System.Serializable]
	public class SceneManager : ScriptableObject
	{
		
		#if UNITY_EDITOR

		[SerializeField] private UnityEvent onOrganiseScene = null;

		private readonly string[] cameraPerspective_list = { "2D", "2.5D", "3D" };
		private bool tempOverrideCameraPerspective;
		private CameraPerspective tempCameraPerspective;
		private MovingTurning tempMovingTurning = MovingTurning.Unity2D;
		
		private int selectedSceneObject;
		
		private List<ScenePrefabCollection> scenePrefabCollections = new List<ScenePrefabCollection> ();
		[SerializeField] private List<SceneManagerPrefabData> sceneManagerPrefabDatas = new List<SceneManagerPrefabData> ();
		
		private string newFolderName = "";
		private string newPrefabName;
		private Renderer positionHotspotOverMesh;
		private static GUILayoutOption buttonWidth = GUILayout.MaxWidth (120f);

		private bool showStructure = true;
		private bool showSettings = true;
		private bool showCutscenes = true;
		private bool showVisibility = true;
		private bool showPrefabs = true;
		private bool showAttributes = true;
		
		/*private ScenePrefab hoverScenePrefab;
		private Rect hoverSceneRect;
		private const int PreviewBoxWidth = 300;
		private const int PreviewBoxHeight = 250;*/

		private List<GameObject> existingPrefabs = new List<GameObject> ();
		
		private Texture2D hotspotIcon, triggerIcon, collisionIcon, markerIcon, playerStartIcon, navMeshIcon;


		private static string assetFolder
		{
			get
			{
				return Resource.MainFolderPath + "/Prefabs/";
			}
		}

		/** Shows the GUI. */
		public void ShowGUI (Rect windowRect, System.Action<ActionList> showALEditor, System.Action<ActionListAsset> showALAEditor)
		{
			ShowSettingsGUI (windowRect, showALEditor, showALAEditor);

			/*if (hoverScenePrefab != null && ACEditorPrefs.DrawPrefabPreviews)
			{
				DrawPreviewGUI (hoverScenePrefab, windowRect, new Vector2 (hoverSceneRect.center.x, hoverSceneRect.y));
			}*/

			if (GUI.changed)
			{
				UnityVersionHandler.CustomSetDirty (KickStarter.sceneSettings);
				UnityVersionHandler.CustomSetDirty (KickStarter.playerMovement);

				if (KickStarter.sceneSettings != null && KickStarter.sceneSettings.defaultPlayerStart != null)
				{
					UnityVersionHandler.CustomSetDirty (KickStarter.sceneSettings.defaultPlayerStart);
				}
			}
		}


		/*private void DrawPreviewGUI (ScenePrefab prefab, Rect windowRect, Vector2 position)
		{
			try
			{
				position.x -= (PreviewBoxWidth * 0.5f);
				position.y -= (PreviewBoxHeight + 10);

				if (position.x < 0f) position.x = 0f;
				if (position.x > windowRect.width - PreviewBoxWidth - 12) position.x = windowRect.width - PreviewBoxWidth - 12;

				GUILayout.BeginArea (new Rect (position.x, position.y, PreviewBoxWidth, PreviewBoxHeight), Resource.NodeSkin.customStyles[EditorGUIUtility.isProSkin ? 43 : 44]);

				GUI.Label (new Rect (10, 10, 280, 29), prefab.label, CustomStyles.PreviewHeader);
				GUI.DrawTexture (new Rect (10, 40, 280, 200), Resources.Load ("PreviewTestTrigger") as Texture2D, ScaleMode.StretchToFill);
				GUI.Label (new Rect (10, 170, 280, 70), prefab.helpText, CustomStyles.PreviewDescription);

				GUILayout.EndArea ();
			}
			catch {}
		}*/


		private void ShowSettingsGUI (Rect windowRect, System.Action<ActionList> showALEditor, System.Action<ActionListAsset> showALAEditor)
		{
			string sceneName = MultiSceneChecker.EditActiveScene ();
			if (!string.IsNullOrEmpty (sceneName) && UnityEngine.SceneManagement.SceneManager.sceneCount > 1)
			{
				EditorGUILayout.LabelField ("Editing scene: '" + sceneName + "'", EditorStyles.boldLabel);
				EditorGUILayout.Space ();
			}

			SceneStructureGUI ();

			if (KickStarter.settingsManager == null)
			{
				EditorGUILayout.HelpBox ("No Settings Manager defined - cannot display full Editor without it.", MessageType.Warning);
				return;
			}
			
			if (KickStarter.sceneSettings == null)
			{
				return;
			}

			EditorGUILayout.Space ();

			SceneSettingsGUI ();

			EditorGUILayout.Space ();

			CutscenesGUI (showALEditor, showALAEditor);

			EditorGUILayout.Space ();

			SceneAttributesGUI ();

			EditorGUILayout.Space ();

			VisibilityGUI (windowRect);

			EditorGUILayout.Space ();

			ListPrefabs (windowRect);
		}


		private void SceneStructureGUI ()
		{
			showStructure = CustomGUILayout.ToggleHeader (showStructure, "Basic structure");
			if (showStructure)
			{
				CustomGUILayout.BeginVertical ();
				if (KickStarter.sceneSettings == null && KickStarter.settingsManager != null && KickStarter.settingsManager.movementMethod != MovementMethod.FirstPerson)
				{
					EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.LabelField ("Override the default camera perspective?");
					tempOverrideCameraPerspective = EditorGUILayout.Toggle (tempOverrideCameraPerspective);
					EditorGUILayout.EndHorizontal ();
					if (tempOverrideCameraPerspective)
					{
						int cameraPerspective_int = (int) tempCameraPerspective;
						cameraPerspective_int = EditorGUILayout.Popup ("Camera perspective:", cameraPerspective_int, cameraPerspective_list);
						tempCameraPerspective = (CameraPerspective) cameraPerspective_int;

						if (tempCameraPerspective == CameraPerspective.TwoD)
						{
							tempMovingTurning = (MovingTurning) EditorGUILayout.EnumPopup ("Moving and turning:", tempMovingTurning);
							if (tempMovingTurning == MovingTurning.TopDown)
							{
								EditorGUILayout.HelpBox ("This mode is now deprecated - use Unity 2D mode instead.", MessageType.Warning);
							}
						}
					}
				}

				if (KickStarter.sceneSettings != null)
				{
					KickStarter.sceneSettings.ShowCameraOverrideLabel ();
				}

				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label ("Organise scene:");
				if (GUILayout.Button ("With folders", EditorStyles.miniButtonLeft))
				{
					InitialiseObjects ();
				}
				if (GUILayout.Button ("Without folders", EditorStyles.miniButtonRight))
				{
					InitialiseObjects (false);
				}
				EditorGUILayout.EndHorizontal ();

				if (KickStarter.sceneSettings == null)
				{
					CustomGUILayout.EndVertical ();
					return;
				}

				EditorGUILayout.BeginHorizontal ();
				newFolderName = EditorGUILayout.TextField (newFolderName);
		
				if (GUILayout.Button ("Create new folder", buttonWidth))
				{
					if (newFolderName != "")
					{
						GameObject newFolder = new GameObject();
						
						if (!newFolderName.StartsWith ("_"))
							newFolder.name = "_" + newFolderName;
						else
							newFolder.name = newFolderName;
						
						Undo.RegisterCreatedObjectUndo (newFolder, "Create folder " + newFolder.name);
						
						if (Selection.activeGameObject)
						{
							newFolder.transform.parent = Selection.activeGameObject.transform;
						}
						
						Selection.activeObject = newFolder;
					}
				}
				EditorGUILayout.EndHorizontal ();
				CustomGUILayout.EndVertical ();
			}
		}


		private PlayerStart AutoCreateDefaultPlayerStart ()
		{
			PlayerStart newPlayerStart = (SceneSettings.IsUnity2D ())
										? AddPrefab ("Navigation", "PlayerStart2D", true, false, true).GetComponent <PlayerStart>()
										: AddPrefab ("Navigation", "PlayerStart", true, false, true).GetComponent <PlayerStart>();

			newPlayerStart.gameObject.name = "Default PlayerStart";
			EditorGUIUtility.PingObject (newPlayerStart.gameObject);
			return newPlayerStart;
		}


		private _Camera AutoCreateDefaultCamera ()
		{
			_Camera newCamera;

			switch (SceneSettings.CameraPerspective)
			{
				case CameraPerspective.ThreeD:
				default:
					newCamera = AddPrefab ("Camera", "GameCamera", true, false, true).GetComponent <GameCamera>();
					break;

				case CameraPerspective.TwoPointFiveD:
					newCamera = AddPrefab ("Camera", "GameCamera2.5D", true, false, true).GetComponent <GameCamera25D>();
					break;

				case CameraPerspective.TwoD:
					newCamera = AddPrefab ("Camera", "GameCamera2D", true, false, true).GetComponent <GameCamera2D>();
					break;
			}

			newCamera.gameObject.name = "NavCam 1";
			EditorGUIUtility.PingObject (KickStarter.sceneSettings.defaultPlayerStart.cameraOnStart);
			return newCamera;
		}


		private SortingMap AutoCreateDefaultSortingMap ()
		{
			SortingMap newSortingMap = AddPrefab ("Navigation", "SortingMap", true, false, true).GetComponent <SortingMap>();
			newSortingMap.gameObject.name = "Default SortingMap";
			EditorGUIUtility.PingObject (newSortingMap.gameObject);
			return newSortingMap;
		}


		private TintMap AutoCreateDefaultTintMap ()
		{
			TintMap newTintMap = AddPrefab ("Camera", "TintMap", true, false, true).GetComponent <TintMap>();
			PutInFolder (newTintMap.gameObject, "_SetGeometry");
			newTintMap.gameObject.name = "Default TintMap";
			EditorGUIUtility.PingObject (newTintMap.gameObject);
			return newTintMap;
		}


		private Sound AutoCreateDefaultSound ()
		{
			Sound newSound = AddPrefab ("Logic", "Sound", true, false, true).GetComponent <Sound>();
			newSound.gameObject.name = "Default Sound";
			newSound.playWhilePaused = true;
			EditorGUIUtility.PingObject (newSound.gameObject);
			return newSound;
		}


		private void SceneSettingsGUI ()
		{
			showSettings = CustomGUILayout.ToggleHeader (showSettings, "Scene settings");
			if (showSettings)
			{
				CustomGUILayout.BeginVertical ();
				KickStarter.sceneSettings.navigationMethod = (AC_NavigationMethod) CustomGUILayout.EnumPopup ("Pathfinding method:", KickStarter.sceneSettings.navigationMethod, "AC.KickStarter.sceneSettings.navigationMethod", "The scene's navigation method");
				if (KickStarter.sceneSettings.navigationMethod == AC_NavigationMethod.Custom)
				{
					KickStarter.sceneSettings.customNavigationClass = CustomGUILayout.DelayedTextField ("Script name:", KickStarter.sceneSettings.customNavigationClass, "AC.KickStarter.sceneSettings.customNavigationClass", "The class name of the NavigationEngine ScriptableObject that is used to handle pathfinding");
				}
				KickStarter.navigationManager.ResetEngine ();
				if (KickStarter.navigationManager.navigationEngine != null)
				{
					KickStarter.navigationManager.navigationEngine.SceneSettingsGUI ();
				}
				
				KickStarter.sceneSettings.defaultPlayerStart = CustomGUILayout.AutoCreateField<PlayerStart> ("Default PlayerStart:", KickStarter.sceneSettings.defaultPlayerStart, AutoCreateDefaultPlayerStart, "AC.KickStarter.sceneSettings.defaultPlayerStart", "The scene's default PlayerStart");

				if (KickStarter.sceneSettings.defaultPlayerStart)
				{
					KickStarter.sceneSettings.defaultPlayerStart.cameraOnStart = CustomGUILayout.AutoCreateField <_Camera> ("Default Camera:", KickStarter.sceneSettings.defaultPlayerStart.cameraOnStart, AutoCreateDefaultCamera, "AC.KickStarter.sceneSettings.defaultPlayerStart.cameraOnStart", "The Camera that should be made active when the Player starts the scene from this point");
					if (KickStarter.sceneSettings.defaultPlayerStart.cameraOnStart)
					{
						if (KickStarter.settingsManager != null && KickStarter.settingsManager.movementMethod == MovementMethod.FirstPerson)
						{
							EditorGUILayout.HelpBox ("The scene's default camera will be overridden during gameplay by the Player's first person camera.", MessageType.Info);
						}
					}
				}

				KickStarter.sceneSettings.sortingMap = CustomGUILayout.AutoCreateField <SortingMap> ("Default Sorting map:", KickStarter.sceneSettings.sortingMap, AutoCreateDefaultSortingMap, "AC.KickStarter.sceneSettings.sortingMap", "The scene's default SortingMap");
				if (SceneSettings.IsUnity2D ())
				{
					KickStarter.sceneSettings.tintMap = CustomGUILayout.AutoCreateField <TintMap> ("Default Tint map:", KickStarter.sceneSettings.tintMap, AutoCreateDefaultTintMap, "AC.KickStarter.sceneSettings.tintMap", "The scene's default TintMap");
				}
				KickStarter.sceneSettings.defaultSound = CustomGUILayout.AutoCreateField <Sound> ("Default Sound:", KickStarter.sceneSettings.defaultSound, AutoCreateDefaultSound, "AC.KickStarter.sceneSettings.defaultSound", "The scene's default Sound. This is used to play Menu audio");

				if (SceneSettings.IsTopDown () || SceneSettings.IsUnity2D ())
				{
					KickStarter.sceneSettings.overrideVerticalReductionFactor = EditorGUILayout.BeginToggleGroup (new GUIContent ("Override vertical movement factor?", "If True, then the 'Vertical movement factor' in the Settings Manager will be overridden with a scene-specific value"), KickStarter.sceneSettings.overrideVerticalReductionFactor);
					KickStarter.sceneSettings.verticalReductionFactor = EditorGUILayout.Slider (new GUIContent ("Vertical movement factor:", "How much slower vertical movement is compared to horizontal movement"), KickStarter.sceneSettings.verticalReductionFactor, 0.1f, 1f);
					EditorGUILayout.EndToggleGroup ();
				}
				CustomGUILayout.EndVertical ();
			}
		}


		private Cutscene AutoCreateCutsceneOnStart ()
		{
			Cutscene newCutscene = AddPrefab ("Logic", "Cutscene", true, false, true).GetComponent <Cutscene>();
			newCutscene.gameObject.name = "OnStart";
			EditorGUIUtility.PingObject (newCutscene.gameObject);
			return newCutscene;
		}


		private Cutscene AutoCreateCutsceneOnLoad ()
		{
			Cutscene newCutscene = AddPrefab ("Logic", "Cutscene", true, false, true).GetComponent <Cutscene>();
			newCutscene.gameObject.name = "OnLoad";
			EditorGUIUtility.PingObject (newCutscene.gameObject);
			return newCutscene;
		}


		private void CutscenesGUI (System.Action<ActionList> showALEditor, System.Action<ActionListAsset> showALAEditor)
		{
			showCutscenes = CustomGUILayout.ToggleHeader (showCutscenes, "Scene cutscenes");
			if (showCutscenes)
			{
				CustomGUILayout.BeginVertical ();
				KickStarter.sceneSettings.actionListSource = (ActionListSource) CustomGUILayout.EnumPopup ("ActionList source:", KickStarter.sceneSettings.actionListSource, "AC.KickStarter.sceneSettings.actionListSource", "The source of Actions used for the scene's main cutscenes");

				EditorGUILayout.BeginHorizontal ();
				if (KickStarter.sceneSettings.actionListSource == ActionListSource.InScene)
				{
					KickStarter.sceneSettings.cutsceneOnStart = CustomGUILayout.AutoCreateField <Cutscene> ("On start:", KickStarter.sceneSettings.cutsceneOnStart, AutoCreateCutsceneOnStart, "AC.KickStarter.sceneSettings.cutsceneOnStart", "The Cutscene to run whenever the game beings from this scene, or when this scene is visited during gameplay");
					if (KickStarter.sceneSettings.cutsceneOnStart && GUILayout.Button (string.Empty, CustomStyles.IconNodes))
					{
						showALEditor.Invoke (KickStarter.sceneSettings.cutsceneOnStart);
					}
				}
				else if (KickStarter.sceneSettings.actionListSource == ActionListSource.AssetFile)
				{
					KickStarter.sceneSettings.actionListAssetOnStart = (ActionListAsset) CustomGUILayout.ObjectField <ActionListAsset> ("On start:", KickStarter.sceneSettings.actionListAssetOnStart, true, "AC.KickStarter.sceneSettings.actionListAssetOnStart", "The ActionList asset to run whenever the game beings from this scene, or when this scene is visited during gameplay");

					if (KickStarter.sceneSettings.actionListAssetOnStart == null && CustomGUILayout.ClickedCreateButton ())
					{
						KickStarter.sceneSettings.actionListAssetOnStart = ActionListAssetMenu.CreateAsset (KickStarter.sceneSettings.gameObject.scene.name + "_OnStart");
					}
					if (KickStarter.sceneSettings.actionListAssetOnStart && GUILayout.Button (string.Empty, CustomStyles.IconNodes))
					{
						showALAEditor.Invoke (KickStarter.sceneSettings.actionListAssetOnStart);
					}
				}
				EditorGUILayout.EndHorizontal ();
				EditorGUILayout.BeginHorizontal ();
				if (KickStarter.sceneSettings.actionListSource == ActionListSource.InScene)
				{
					KickStarter.sceneSettings.cutsceneOnLoad = CustomGUILayout.AutoCreateField <Cutscene> ("On load:", KickStarter.sceneSettings.cutsceneOnLoad, AutoCreateCutsceneOnLoad, "AC.KickStarter.sceneSettings.cutsceneOnLoad", "The Cutscene to run whenever this scene is loaded after restoring a saved game file");
					if (KickStarter.sceneSettings.cutsceneOnLoad && GUILayout.Button (string.Empty, CustomStyles.IconNodes))
					{
						showALEditor.Invoke (KickStarter.sceneSettings.cutsceneOnLoad);
					}
				}
				else if (KickStarter.sceneSettings.actionListSource == ActionListSource.AssetFile)
				{
					KickStarter.sceneSettings.actionListAssetOnLoad = (ActionListAsset) CustomGUILayout.ObjectField <ActionListAsset> ("On load:", KickStarter.sceneSettings.actionListAssetOnLoad, true, "AC.KickStarter.sceneSettings.actionListAssetOnLoad", "The ActionList asset to run whenever this scene is loaded after restoring a saved game file");

					if (KickStarter.sceneSettings.actionListAssetOnLoad == null && CustomGUILayout.ClickedCreateButton ())
					{
						KickStarter.sceneSettings.actionListAssetOnLoad = ActionListAssetMenu.CreateAsset (KickStarter.sceneSettings.gameObject.scene.name + "_OnLoad");
					}
					if (KickStarter.sceneSettings.actionListAssetOnLoad && GUILayout.Button (string.Empty, CustomStyles.IconNodes))
					{
						showALAEditor.Invoke (KickStarter.sceneSettings.actionListAssetOnLoad);
					}
				}
				EditorGUILayout.EndHorizontal ();
				CustomGUILayout.EndVertical ();
			}
		}

		
		private void VisibilityGUI (Rect windowRect)
		{
			if (KickStarter.sceneSettings == null) return;

			showVisibility = CustomGUILayout.ToggleHeader (showVisibility, "Visibility");
			if (showVisibility)
			{
				CustomGUILayout.BeginVertical ();
				GUILayoutOption[] options = new GUILayoutOption[2] { GUILayout.Width (((windowRect.width - 10f) / 3f) - 11f), GUILayout.Height (20f) };
				EditorGUI.BeginChangeCheck ();

				EditorGUILayout.BeginHorizontal ();
				if (hotspotIcon == null) hotspotIcon = (Texture2D) AssetDatabase.LoadAssetAtPath (Resource.MainFolderPath + "/Graphics/PrefabIcons/Hotspot.png", typeof (Texture2D));
				KickStarter.sceneSettings.visibilityHotspots = GUILayout.Toggle (KickStarter.sceneSettings.visibilityHotspots, (hotspotIcon) ? new GUIContent (" Hotspots", hotspotIcon) : new GUIContent ("Hotspots"), options);

				if (triggerIcon == null) triggerIcon = (Texture2D) AssetDatabase.LoadAssetAtPath (Resource.MainFolderPath + "/Graphics/PrefabIcons/AC_Trigger.png", typeof (Texture2D));
				KickStarter.sceneSettings.visibilityTriggers = GUILayout.Toggle (KickStarter.sceneSettings.visibilityTriggers, (triggerIcon) ? new GUIContent (" Triggers", triggerIcon) : new GUIContent ("Triggers"), options);

				if (collisionIcon == null) collisionIcon = (Texture2D) AssetDatabase.LoadAssetAtPath (Resource.MainFolderPath + "/Graphics/PrefabIcons/_Collision.png", typeof (Texture2D));
				KickStarter.sceneSettings.visibilityCollision = GUILayout.Toggle (KickStarter.sceneSettings.visibilityCollision, (collisionIcon) ? new GUIContent (" Collision", collisionIcon) : new GUIContent ("Collision"), options);
				EditorGUILayout.EndHorizontal ();

				EditorGUILayout.BeginHorizontal ();
				if (markerIcon == null) markerIcon = (Texture2D) AssetDatabase.LoadAssetAtPath (Resource.MainFolderPath + "/Graphics/PrefabIcons/Marker.png", typeof (Texture2D));
				KickStarter.sceneSettings.visibilityMarkers = GUILayout.Toggle (KickStarter.sceneSettings.visibilityMarkers, (markerIcon) ? new GUIContent (" Markers", markerIcon) : new GUIContent ("Markers"), options);

				if (playerStartIcon == null) playerStartIcon = (Texture2D) AssetDatabase.LoadAssetAtPath (Resource.MainFolderPath + "/Graphics/PrefabIcons/PlayerStart.png", typeof (Texture2D));
				KickStarter.sceneSettings.visibilityPlayerStarts = GUILayout.Toggle (KickStarter.sceneSettings.visibilityPlayerStarts, (playerStartIcon) ? new GUIContent (" PlayerStarts", playerStartIcon) : new GUIContent ("PlayerStarts"), options);

				if (navMeshIcon == null) navMeshIcon = (Texture2D) AssetDatabase.LoadAssetAtPath (Resource.MainFolderPath + "/Graphics/PrefabIcons/NavigationMesh.png", typeof (Texture2D));
				KickStarter.sceneSettings.visibilityNavMesh = GUILayout.Toggle (KickStarter.sceneSettings.visibilityNavMesh, (navMeshIcon) ? new GUIContent (" NavMesh", navMeshIcon) : new GUIContent ("NavMesh"), options);
				EditorGUILayout.EndHorizontal ();

				if (EditorGUI.EndChangeCheck ())
				{		
					SceneView.RepaintAll ();
					Marker[] markers = UnityVersionHandler.FindObjectsOfType<Marker>();
					foreach (Marker marker in markers)
					{
						marker.DrawGizmos ();
					}
				}
				CustomGUILayout.EndVertical ();
			}
		}


		private void SceneAttributesGUI ()
		{
			if (KickStarter.settingsManager == null)
			{
				EditorGUILayout.HelpBox ("A Settings Manager is required to view and edit scene attributes.", MessageType.Info);
				return;
			}

			if (KickStarter.sceneSettings == null)
			{
				return;
			}

			showAttributes = CustomGUILayout.ToggleHeader (showAttributes, "Scene attributes");
			if (showAttributes)
			{
				CustomGUILayout.BeginVertical ();
				if (KickStarter.settingsManager == null)
				{
					EditorGUILayout.HelpBox ("A Settings Manager must be assigned in order to make use of scene attributes.", MessageType.Warning);
				}
				else
				{
					InvVar[] sceneAttributes = KickStarter.settingsManager.sceneAttributes.ToArray ();
					if (KickStarter.settingsManager.sceneAttributes.Count > 0)
					{
						RebuildProperties (sceneAttributes);

						// UI for setting property values
						if (KickStarter.sceneSettings.attributes.Count > 0)
						{
							foreach (InvVar attribute in KickStarter.sceneSettings.attributes)
							{
								string apiPrefix = "AC.KickStarter.sceneSettings.GetAttribute (" + attribute.id + ")";
								attribute.ShowGUI (apiPrefix, true);
							}

						}
						else
						{
							EditorGUILayout.HelpBox ("No properties have been defined that this inventory item can use.", MessageType.Info);
						}
					}

					if (GUILayout.Button ("Manage attributes"))
					{
						SceneAttributesWindow.Init ();
					}
				}
				CustomGUILayout.EndVertical ();
			}
		}


		private void RebuildProperties (InvVar[] invVars)
		{
			// Which properties are available?
			List<int> availableVarIDs = new List<int>();
			foreach (InvVar invVar in invVars)
			{
				availableVarIDs.Add (invVar.id);
			}
			
			// Create new properties / transfer existing values
			List<InvVar> newInvVars = new List<InvVar>();
			foreach (InvVar invVar in invVars)
			{
				if (availableVarIDs.Contains (invVar.id))
				{
					InvVar newInvVar = new InvVar (invVar);
					InvVar oldInvVar = KickStarter.sceneSettings.GetAttribute (invVar.id);
					if (oldInvVar != null)
					{
						newInvVar.TransferValues (oldInvVar);
					}
					newInvVars.Add (newInvVar);
				}
			}
			
			KickStarter.sceneSettings.attributes = newInvVars;
		}


		/**
		 * <summary>Makes the current scene AC-ready, by setting up the MainCamera, instantiating the GameEngine prefab, and optionally creating "folder" objects.</summary>
		 * <param name = "createFolders">If True, then empty GameObjects that acts as folders, to aid organisation, will be created</param>
		 */
		public void InitialiseObjects (bool createFolders = true)
		{
			bool reallyDoOverrideCameraPerspective = (KickStarter.sceneSettings == null && tempOverrideCameraPerspective) ? true : false;

			if (createFolders)
			{
				CreateFolder ("_Cameras");
				CreateFolder ("_Cutscenes");
				CreateFolder ("_DialogueOptions");
				CreateFolder ("_Interactions");
				CreateFolder ("_Lights");
				CreateFolder ("_Logic");
				CreateFolder ("_Moveables");
				CreateFolder ("_Navigation");
				CreateFolder ("_NPCs");
				CreateFolder ("_Sounds");
				CreateFolder ("_SetGeometry");
				CreateFolder ("_UI");
				
				// Create subfolders
				CreateSubFolder ("_Cameras", "_GameCameras");
				
				CreateSubFolder ("_Logic", "_ArrowPrompts");
				CreateSubFolder ("_Logic", "_Conversations");
				CreateSubFolder ("_Logic", "_Containers");
				CreateSubFolder ("_Logic", "_Hotspots");
				CreateSubFolder ("_Logic", "_Triggers");
				CreateSubFolder ("_Logic", "_Variables");
				
				CreateSubFolder ("_Moveables", "_Tracks");
				
				CreateSubFolder ("_Navigation", "_CollisionCubes");
				CreateSubFolder ("_Navigation", "_CollisionCylinders");
				CreateSubFolder ("_Navigation", "_Markers");
				CreateSubFolder ("_Navigation", "_NavMesh");
				CreateSubFolder ("_Navigation", "_Paths");
				CreateSubFolder ("_Navigation", "_PlayerStarts");
				CreateSubFolder ("_Navigation", "_SortingMaps");
			}

			// Tag the SampleScene's MainCamera as such so that it can be replaced
			GameObject defaultCamera = GameObject.Find ("Main Camera");
			if (defaultCamera != null && UnityVersionHandler.ObjectIsInActiveScene (defaultCamera) && defaultCamera.GetComponent <MainCamera>() == null && defaultCamera.tag == Tags.untagged)
			{
				defaultCamera.tag = Tags.mainCamera;
			}
						
			// Delete default main camera
			GameObject[] mainCameras = GameObject.FindGameObjectsWithTag (Tags.mainCamera);
			foreach (GameObject oldMainCam in mainCameras)
			{
				if (UnityVersionHandler.ObjectIsInActiveScene (oldMainCam) && oldMainCam.GetComponent <MainCamera>() == null)
				{
					if (oldMainCam.GetComponent <Camera>())
					{
						string camName = oldMainCam.name;

						bool replaceCamera = true;

						if (camName != "Main Camera" || oldMainCam.transform.parent != null)
						{
							replaceCamera = EditorUtility.DisplayDialog ("MainCamera detected", "AC has detected the scene object '" + camName + "', which is tagged as 'MainCamera'." +
						                                             "\n\n" +
						                                             "AC requires that the scene's MainCamera also has the AC.MainCamera script attached.  Should it convert '" + camName + "', or untag it and create a separate MainCamera for AC?" +
						                                             "\n\n" +
						                                             "(Note: If '" + camName + "' is part of a Player prefab, it is recommended to simply untag it.)", "Convert it", "Untag it");
						}

						if (replaceCamera)
						{
							oldMainCam.AddComponent <MainCamera>();
							
							string camPrefabfileName = assetFolder + "Automatic" + Path.DirectorySeparatorChar.ToString () + "MainCamera.prefab";
							GameObject camPrefab = (GameObject) AssetDatabase.LoadAssetAtPath (camPrefabfileName, typeof (GameObject));
							if (camPrefab)
							{
								Texture2D prefabFadeTexture = camPrefab.GetComponent <MainCamera>().GetFadeTexture ();
								oldMainCam.GetComponent <MainCamera>().SetDefaultFadeTexture (prefabFadeTexture);
							}
							ACDebug.Log ("'" + oldMainCam.name + "' has been converted to an Adventure Creator MainCamera.");
						}
						else
						{
							ACDebug.Log ("Untagged MainCamera '" + oldMainCam.name + "'.", oldMainCam);
							oldMainCam.tag = Tags.untagged;
							oldMainCam.GetComponent <Camera>().enabled = false;
							if (camName == "Main Camera") oldMainCam.gameObject.name += " (Untagged)";
						}
					}
					else
					{
						ACDebug.Log ("Untagged MainCamera '" + oldMainCam.name + "', as it had no Camera component.", oldMainCam);
						oldMainCam.tag = Tags.untagged;
					}
				}
			}

			bool foundMainCamera = false;
			foreach (GameObject oldMainCam in mainCameras)
			{
				if (UnityVersionHandler.ObjectIsInActiveScene (oldMainCam) && oldMainCam.GetComponent <MainCamera>() != null)
				{
					foundMainCamera = true;
				}
			}
			
			// Create Game engine
			GameObject gameEngineOb = AddPrefab ("Automatic", "GameEngine", false, false, false);
			if (gameEngineOb == null && !UnityVersionHandler.ObjectIsInActiveScene ("GameEngine"))
			{
				gameEngineOb = new GameObject ("GameEngine");
				gameEngineOb.AddComponent<MenuSystem> ();
				gameEngineOb.AddComponent<Dialog> ();
				gameEngineOb.AddComponent<PlayerInput> ();
				gameEngineOb.AddComponent<PlayerInteraction> ();
				gameEngineOb.AddComponent<PlayerMovement> ();
				gameEngineOb.AddComponent<PlayerCursor> ();
				gameEngineOb.AddComponent<PlayerQTE> ();
				gameEngineOb.AddComponent<SceneSettings> ();
				gameEngineOb.AddComponent<NavigationManager> ();
				gameEngineOb.AddComponent<ActionListManager> ();
				gameEngineOb.AddComponent<LocalVariables> ();
				gameEngineOb.AddComponent<MenuPreview> ();
				gameEngineOb.AddComponent<EventManager> ();
				gameEngineOb.AddComponent<KickStarter> ();
			}

			// Camera perspective override
			if (reallyDoOverrideCameraPerspective)
			{
				KickStarter.sceneSettings.SetOverrideCameraPerspective (tempCameraPerspective, tempMovingTurning);
			}

			// Create main camera if none exists
			if (!foundMainCamera)
			{
				GameObject mainCamOb = AddPrefab ("Automatic", "MainCamera", false, false, false);
				#if NEW_PREFABS
				try
				{
					PrefabUtility.UnpackPrefabInstance (mainCamOb, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
				}
				catch {}
				#else
				PrefabUtility.DisconnectPrefabInstance (mainCamOb);
				#endif
				PutInFolder (mainCamOb, "_Cameras");
				if (SceneSettings.IsUnity2D ())
				{
					KickStarter.CameraMain.orthographic = true;
				}
			}
			
			// Create Background Camera (if 2.5D)
			if (SceneSettings.CameraPerspective == CameraPerspective.TwoPointFiveD)
			{
				CreateSubFolder ("_SetGeometry", "_BackgroundImages");
				GameObject newOb = AddPrefab ("Automatic", "BackgroundCamera", false, false, false);
				PutInFolder (newOb, "_Cameras");
			}
			
			// Assign Player Start
			if (KickStarter.sceneSettings && KickStarter.sceneSettings.defaultPlayerStart == null)
			{
				string playerStartPrefab = "PlayerStart";
				if (SceneSettings.IsUnity2D ())
				{
					playerStartPrefab += "2D";
				}

				GameObject playerStartOb = AddPrefab ("Navigation", playerStartPrefab, true, false, true);
				if (playerStartOb)
				{
					PlayerStart playerStart = playerStartOb.GetComponent<PlayerStart> ();
					KickStarter.sceneSettings.defaultPlayerStart = playerStart;
				}
			}
			
			// Pathfinding method
			if (SceneSettings.IsUnity2D ())
			{
				KickStarter.sceneSettings.navigationMethod = AC_NavigationMethod.PolygonCollider;
				KickStarter.navigationManager.ResetEngine ();
			}

			tempOverrideCameraPerspective = false;

			if (onOrganiseScene != null)
			{
				onOrganiseScene.Invoke ();
			}
		}
		

		private static void RenameObject (GameObject ob, string resourceName)
		{
			int slash = resourceName.IndexOf ("/");
			string newName;

			if (slash > 0)
			{
				newName = resourceName.Remove (0, slash + 1);
			}
			else
			{
				newName = resourceName;
			}

			ob.name = newName;
		}


		/**
		 * <summary>Adds an Adventure Creator prefab to the scene.</summary>
		 * <param name = "folderName">The name of the subfolder that the prefab lives in, within /Assets/AdventureCreator/Prefabs</param>
		 * <param name = "prefabName">The name of the prefab filename, without the '.asset' extension</param>
		 * <param name = "canCreateMultiple">If True, then multiple instances of the prefab can exuist within the scene</param>
		 * <param name = "selectAfter">If True, the created GameObject will be selected in the Hierarchy window</param>
		 * <param name = "putInFolder">If True, then the Scene Manager will attempt to place the created GameObject in an appropriate "folder" object.</param>
		 * <returns>The created prefab GameObject</param>
		 */
		public static GameObject AddPrefab (string folderName, string prefabName, bool canCreateMultiple, bool selectAfter, bool putInFolder)
		{
			if (canCreateMultiple || !UnityVersionHandler.ObjectIsInActiveScene (prefabName))
			{
				string fileName = assetFolder + folderName + Path.DirectorySeparatorChar.ToString () + prefabName + ".prefab";
				
				GameObject newOb = (GameObject) PrefabUtility.InstantiatePrefab (AssetDatabase.LoadAssetAtPath (fileName, typeof (GameObject)));
				if (newOb == null)
				{
					ACDebug.LogWarning ("Error creating new object '" + prefabName + "' from filepath '" + fileName + "'.");
					return null;
				}
				newOb.name = "Temp";
				
				if (folderName != "" && putInFolder)
				{
					if (!PutInFolder (newOb, "_" + prefabName + "s"))
					{
						string newName = "_" + prefabName;
						
						if (newName.Contains ("2D"))
						{
							newName = newName.Substring (0, newName.IndexOf ("2D"));
							
							if (!PutInFolder (newOb, newName + "s"))
							{
								PutInFolder (newOb, newName);
							}
							else
							{
								PutInFolder (newOb, newName);
							}
						}
						else if (newName.Contains ("2.5D"))
						{
							newName = newName.Substring (0, newName.IndexOf ("2.5D"));
							
							if (!PutInFolder (newOb, newName + "s"))
							{
								PutInFolder (newOb, newName);
							}
							else
							{
								PutInFolder (newOb, newName);
							}
						}
						else if (newName.Contains ("Animated"))
						{
							newName = newName.Substring (0, newName.IndexOf ("Animated"));
							
							if (!PutInFolder (newOb, newName + "s"))
							{
								PutInFolder (newOb, newName);
							}
							else
							{
								PutInFolder (newOb, newName);
							}
						}
						else if (newName.Contains ("ThirdPerson"))
						{
							newName = newName.Substring (0, newName.IndexOf ("ThirdPerson"));
							
							if (!PutInFolder (newOb, newName + "s"))
							{
								PutInFolder (newOb, newName);
							}
							else
							{
								PutInFolder (newOb, newName);
							}
						}
						else
						{
							PutInFolder (newOb, newName);
						}
					}
				}
				
				if (newOb.GetComponent <GameCamera2D>())
				{
					newOb.GetComponent <GameCamera2D>().SetCorrectRotation ();
				}

				RenameObject (newOb, prefabName);

				RegisterNewObject (newOb);

				// Select the object
				if (selectAfter)
				{
					Selection.activeObject = newOb;
				}
				
				return newOb;
			}
			
			return null;
		}
		
		
		private static bool PutInFolder (GameObject ob, string folderName)
		{
			return UnityVersionHandler.PutInFolder (ob, folderName);
		}
		
		
		private void CreateFolder (string folderName)
		{
			if (string.IsNullOrEmpty (folderName)) return;

			if (!UnityVersionHandler.ObjectIsInActiveScene (folderName))
			{
				GameObject newFolder = new GameObject();
				newFolder.name = folderName;
				Undo.RegisterCreatedObjectUndo (newFolder, "Created " + newFolder.name);
			}
		}
		
		
		private void CreateSubFolder (string baseFolderName, string subFolderName)
		{
			if (string.IsNullOrEmpty (baseFolderName) || string.IsNullOrEmpty (subFolderName)) return;

			CreateFolder (baseFolderName);
			
			if (!UnityVersionHandler.ObjectIsInActiveScene (subFolderName))
			{
				GameObject newFolder = new GameObject ();
				newFolder.name = subFolderName;
				Undo.RegisterCreatedObjectUndo (newFolder, "Created " + newFolder.name);

				if (!PutInFolder (newFolder, baseFolderName))
				{
					ACDebug.Log ("Folder " + baseFolderName + " does not exist!");
				}
			}
		}
		

		public void RegneratePrefabCache ()
		{
			DeclareScenePrefabs ();
		}


		private void ListPrefabs (Rect windowRect)
		{
			if (scenePrefabCollections.Count == 0 || GUI.changed)
			{
				RegneratePrefabCache ();
			}

			showPrefabs = CustomGUILayout.ToggleHeader (showPrefabs, "Scene prefabs");
			if (showPrefabs)
			{
				CustomGUILayout.BeginVertical ();

				#if NEW_PREFABS
				GameObject prefabRoot = UnityVersionHandler.GetPrefabStageRoot ();
				if (prefabRoot)
				{
					EditorGUILayout.HelpBox ("Currently editing prefab " + prefabRoot.name + " - new objects added here will be added to this prefab.", MessageType.Info);
				}
				#endif
				ListAllPrefabs (windowRect);

				CustomGUILayout.EndVertical ();
			}
		}
		
		
		private void ListAllPrefabs (Rect windowRect)
		{
			foreach (ScenePrefabCollection scenePrefabCollection in scenePrefabCollections)
			{
				bool isEven = false;

				if (scenePrefabCollections.IndexOf (scenePrefabCollection) > 0) EditorGUILayout.Space ();
				EditorGUILayout.LabelField (scenePrefabCollection.CategoryName,  CustomStyles.PrefabHeader);
				EditorGUILayout.Space ();
				
				EditorGUILayout.BeginHorizontal ();
				
				float buttonWidth = windowRect.width / 2f - 10f;

				foreach (ScenePrefab prefab in scenePrefabCollection.ScenePrefabs)
				{
					isEven = !isEven;

					if (prefab.icon && buttonWidth > 65f)
					{
						if (GUILayout.Button (new GUIContent (" " + prefab.label, prefab.icon, prefab.helpText), CustomStyles.PrefabButton, GUILayout.MaxWidth (buttonWidth)))
						{
							GUI.skin = null;
							ClickPrefabButton (prefab, Event.current.button);
						}
					}
					else if (prefab.icon)
					{
						if (GUILayout.Button (new GUIContent (prefab.icon, prefab.helpText), CustomStyles.PrefabButton, GUILayout.Width (buttonWidth)))
						{
							GUI.skin = null;
							ClickPrefabButton (prefab, Event.current.button);
						}
					}
					else
					{
						if (GUILayout.Button (new GUIContent (" " + prefab.label, prefab.helpText), CustomStyles.PrefabButton))
						{
							GUI.skin = null;
							ClickPrefabButton (prefab, Event.current.button);
						}
					}

					/*if (Event.current.type == EventType.Repaint && GUILayoutUtility.GetLastRect ().Contains (Event.current.mousePosition))
					{
						hoverScenePrefab = prefab;
						hoverSceneRect = GUILayoutUtility.GetLastRect ();
					}*/
					
					if (!isEven)
					{
						EditorGUILayout.EndHorizontal ();
						EditorGUILayout.BeginHorizontal ();
					}
					else
					{
					//	GUILayout.FlexibleSpace ();
					}
				}
				
				EditorGUILayout.EndHorizontal ();
				//GUILayout.EndVertical ();
			}
		}

		
		private static void RegisterNewObject (GameObject newOb, bool canAddToPrefab = false)
		{
			if (newOb == null) return;

			#if NEW_PREFABS
			PrefabUtility.UnpackPrefabInstance (newOb, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
			#endif
			Undo.RegisterCreatedObjectUndo (newOb, "Created " + newOb.name);

			if (canAddToPrefab)
			{
				GameObject prefabRoot = UnityVersionHandler.GetPrefabStageRoot ();
				if (prefabRoot)
				{
					newOb.transform.SetParent (prefabRoot.transform);
				}
			}
		}
		
		
		private void ClickPrefabButton (ScenePrefab _prefab, int mouseButton)
		{
			if (mouseButton > 1) return;

			if (mouseButton == 1)
			{
				existingPrefabs.Clear ();
				if (Type.GetType ("AC." + _prefab.componentName) != null)
				{
					var objects = UnityVersionHandler.FindObjectsOfType (Type.GetType ("AC." + _prefab.componentName)) as MonoBehaviour [];
					foreach (MonoBehaviour _object in objects)
					{
						if (UnityVersionHandler.ObjectIsInActiveScene (_object.gameObject) && !existingPrefabs.Contains (_object.gameObject))
						{
							existingPrefabs.Add (_object.gameObject);
						}
					}
				}
				else if (!string.IsNullOrEmpty (_prefab.componentName))
				{
					var objects = UnityVersionHandler.FindObjectsOfType (Type.GetType (_prefab.componentName)) as MonoBehaviour [];
					foreach (MonoBehaviour _object in objects)
					{
						if (UnityVersionHandler.ObjectIsInActiveScene (_object.gameObject) && !existingPrefabs.Contains (_object.gameObject))
						{
							existingPrefabs.Add (_object.gameObject);
						}
					}
				}
				
				GenericMenu menu = new GenericMenu ();
				int i = 0;
				for (i = 0; i < existingPrefabs.Count; i++)
				{
					menu.AddItem (new GUIContent (existingPrefabs[i].name), false, ExistingPrefabsCallback, i);
				}
				menu.ShowAsContext ();
				
				return;
			}

			// Clicked twice, add new
			GameObject newOb = null;

			if (_prefab.onCreateBefore != null) _prefab.onCreateBefore ();
			
			if (_prefab.prefab)
			{
				newOb = (GameObject) PrefabUtility.InstantiatePrefab (_prefab.prefab);
				newOb.name = _prefab.prefab.name;

				if (_prefab.onCreateAfter != null) _prefab.onCreateAfter (newOb);

				#if NEW_PREFABS
				PrefabUtility.UnpackPrefabInstance (newOb, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
				#endif
				Undo.RegisterCreatedObjectUndo (newOb, "Created " + newOb.name);
				Selection.activeGameObject = newOb;

			}
			else
			{
				string fileName = assetFolder + _prefab.prefabPath + ".prefab";
				newOb = (GameObject) PrefabUtility.InstantiatePrefab (AssetDatabase.LoadAssetAtPath (fileName, typeof (GameObject)));

				if (_prefab.onCreateAfter != null) _prefab.onCreateAfter (newOb);

				RegisterNewObject (newOb, true);
			}
			
			if (newOb)
			{
				PutInFolder (newOb, _prefab.sceneFolder);
				EditorGUIUtility.PingObject (newOb);
			}
		}


		private void ExistingPrefabsCallback (object obj)
		{
			int clickedIndex = (int) obj;
			if (existingPrefabs.Count > 0 && clickedIndex < existingPrefabs.Count)
			{
				EditorGUIUtility.PingObject (existingPrefabs[clickedIndex]);
			}
		}
		
		
		private void DeclareScenePrefabs ()
		{
			scenePrefabCollections.Clear ();

			if (SceneSettings.CameraPerspective == CameraPerspective.ThreeD)
			{
				AddScenePrefab ("Camera", new ScenePrefab ("Standard", "Camera/GameCamera", "_GameCameras", "A camera that can move, rotate, and change its FOV according to its target's position.", "GameCamera"));
				//AddScenePrefab ("Camera", new ScenePrefab ("GameCamera Animated", "Camera/GameCameraAnimated", "_GameCameras", "Plays an Animation Clip when active, or syncs it with its target's position.", "GameCameraAnimated"));
				AddScenePrefab ("Camera", new ScenePrefab ("Third-person", "Camera/GameCameraThirdPerson", "_GameCameras", "A camera that orbits its target, for over-the-shoulder behaviour.", "GameCameraThirdPerson"));
				AddScenePrefab ("Camera", new ScenePrefab ("Array", "Camera/GameCameraArray", "_GameCameras", "A camera whose position is interpolated based on its proximity to an array of child cameras.", "GameCameraArray"));
				AddScenePrefab ("Camera", new ScenePrefab ("Stationary", "Camera/SimpleCamera", "_GameCameras", "A static camera that doesn't move unless via a custom script.", "_Camera"));
			}
			else
			{
				if (SceneSettings.CameraPerspective == CameraPerspective.TwoD)
				{
					AddScenePrefab ("Camera", new ScenePrefab ("Standard", "Camera/GameCamera2D", "_GameCameras", "A camera that can pan sideways according to its target's position and facing direction.", "GameCamera2D", string.Empty, null, OnCreateAfter_Camera2D));

					if (SceneSettings.IsUnity2D ())
					{
						AddScenePrefab ("Camera", new ScenePrefab ("Drag", "Camera/GameCamera2DDrag", "_GameCameras", "A camera that can be panned in 2D by dragging the mouse/touch.", "GameCamera2D"));
						//AddScenePrefab ("Camera", new ScenePrefab ("TintMap", "Camera/TintMap", "_SetGeometry", "A texture used to tint 2D sprites.", "TintMap"));
					}
				}
				else
				{
					AddScenePrefab ("Camera", new ScenePrefab ("2.5D Camera", "Camera/GameCamera2.5D", "_GameCameras", "A stationary camera that can display images in the background.", "GameCamera25D"));
					AddScenePrefab ("Camera", new ScenePrefab ("Background Image", "SetGeometry/BackgroundImage", "_BackgroundImages", "A container for a 2.5D camera's background image.", "BackgroundImage"));
					AddScenePrefab ("Camera", new ScenePrefab ("Scene sprite", "SetGeometry/SceneSprite", "_SetGeometry", "An in-scene sprite for 2.5D games.", "", "SceneSprite"));
				}
			}

			AddScenePrefab ("Characters", new ScenePrefab ("Player", string.Empty, string.Empty, "A character that can be controlled by the player.", string.Empty, "Player", OnCreateBefore_Player));
			AddScenePrefab ("Characters", new ScenePrefab ("NPC", string.Empty, string.Empty, "A non-player character that is controlled through Actions.", string.Empty, "NPC", OnCreateBefore_NPC));
			
			AddScenePrefab ("Logic", new ScenePrefab ("Arrow prompt", "Logic/ArrowPrompt", "_ArrowPrompts", "An on-screen directional prompt for the player.", "ArrowPrompt"));
			AddScenePrefab ("Logic", new ScenePrefab ("Conversation", "Logic/Conversation", "_Conversations", "Stores a list of dialogue options, that the player can choose from.", "Conversation"));
			AddScenePrefab ("Logic", new ScenePrefab ("Container", "Logic/Container", "_Containers", "Store a list of Inventory Items within the scene, for the player to retrieve and add to.", "Container"));
			AddScenePrefab ("Logic", new ScenePrefab ("Cutscene", "Logic/Cutscene", "_Cutscenes", "A sequence of Actions that can form a cinematic.", "Cutscene"));
			AddScenePrefab ("Logic", new ScenePrefab ("Dialogue Option", "Logic/DialogueOption", "_DialogueOptions", "An option available to the player when a Conversation is active.", "DialogueOption"));
			
			if (SceneSettings.IsUnity2D ())
			{
				AddScenePrefab ("Logic", new ScenePrefab ("Hotspot 2D", "Logic/Hotspot2D", "_Hotspots", "A portion of the scene that can be interacted with.", "Hotspot", string.Empty, OnCreateBefore_Hotspot, OnCreateAfter_Hotspot));
			}
			else
			{
				AddScenePrefab ("Logic", new ScenePrefab ("Hotspot", "Logic/Hotspot", "_Hotspots", "A portion of the scene that can be interacted with.", "Hotspot", string.Empty, OnCreateBefore_Hotspot, OnCreateAfter_Hotspot));
			}
			
			AddScenePrefab ("Logic", new ScenePrefab ("Interaction", "Logic/Interaction", "_Interactions", "A sequence of Actions that run when a Hotspot is activated.", "Interaction"));

			if (SceneSettings.IsUnity2D ())
			{
				AddScenePrefab ("Logic", new ScenePrefab ("InteractiveBoundary2D", "Logic/InteractiveBoundary2D", "_Hotspots", "An optional boundary that the player must be inside for a Hotspot to be interactive.", "InteractiveBoundary"));
			}
			else
			{
				AddScenePrefab ("Logic", new ScenePrefab ("Interactive Boundary", "Logic/InteractiveBoundary", "_Hotspots", "An optional boundary that the player must be inside for a Hotspot to be interactive.", "InteractiveBoundary"));
			}

			AddScenePrefab ("Logic", new ScenePrefab ("Sound", "Logic/Sound", "_Sounds", "An audio source that syncs with AC's sound levels.", "Sound"));
			
			if (SceneSettings.IsUnity2D ())
			{
				AddScenePrefab ("Logic", new ScenePrefab ("Trigger 2D", "Logic/Trigger2D", "_Triggers", "A portion of the scene that responds to objects entering it.", "AC_Trigger"));
			}
			else
			{
				AddScenePrefab ("Logic", new ScenePrefab ("Trigger", "Logic/Trigger", "_Triggers", "A portion of the scene that responds to objects entering it.", "AC_Trigger"));
			}

			AddScenePrefab ("Logic", new ScenePrefab ("Variables", "Logic/Variables", "_Variables", "A collection of Variables that can be used to handle logic processing.", "Variables"));
			
			AddScenePrefab ("Moveable", new ScenePrefab ("Draggable", "Moveable/Draggable", "_Moveables", "Can move along pre-defined Tracks, along planes, or be rotated about its centre.", "Moveable_Drag"));
			AddScenePrefab ("Moveable", new ScenePrefab ("PickUp", "Moveable/PickUp", "_Moveables", "Can be grabbed, rotated and thrown freely in 3D space.", "Moveable_PickUp"));
			AddScenePrefab ("Moveable", new ScenePrefab ("Straight Track", "Moveable/StraightTrack", "_Tracks", "Constrains a Drag object along a straight line, optionally adding rolling or screw effects.", "DragTrack_Straight"));
			AddScenePrefab ("Moveable", new ScenePrefab ("Curved Track", "Moveable/CurvedTrack", "_Tracks", "Constrains a Drag object along a circular line.", "DragTrack_Curved"));
			AddScenePrefab ("Moveable", new ScenePrefab ("Hinge Track", "Moveable/HingeTrack", "_Tracks", "Constrains a Drag object's position, only allowing it to rotate in a circular motion.", "DragTrack_Hinge"));
			
			AddScenePrefab ("Navigation", new ScenePrefab ("Sorting Map", "Navigation/SortingMap", "_SortingMaps", "Defines how sprites are scaled and sorted relative to one another.", "SortingMap"));
			
			if (SceneSettings.IsUnity2D ())
			{
				AddScenePrefab ("Navigation", new ScenePrefab ("Collision Cube 2D", "Navigation/CollisionCube2D", "_CollisionCubes", "Blocks Character movement, as well as cursor clicks if placed on the Default layer.", "_Collision"));
				AddScenePrefab ("Navigation", new ScenePrefab ("Marker 2D", "Navigation/Marker2D", "_Markers", "A point in the scene used by Characters and objects.", "Marker"));
				AddScenePrefab ("Navigation", new ScenePrefab ("Random Marker 2D", "Navigation/RandomMarker2D", "_Markers", "A random point in the scene used by Characters and objects.", "Marker"));
			}
			else
			{
				AddScenePrefab ("Navigation", new ScenePrefab ("Collision Cube", "Navigation/CollisionCube", "_CollisionCubes", "Blocks Character movement, as well as cursor clicks if placed on the Default layer.", "_Collision"));
			//	AddScenePrefab ("Navigation", new ScenePrefab ("Collision Cylinder", "Navigation/CollisionCylinder", "_CollisionCylinders", "Blocks Character movement, as well as cursor clicks if placed on the Default layer.", "_Collision", "_CollisionCylinder"));
				AddScenePrefab ("Navigation", new ScenePrefab ("Marker", "Navigation/Marker", "_Markers", "A point in the scene used by Characters and objects.", "Marker"));
				AddScenePrefab ("Navigation", new ScenePrefab ("Random Marker", "Navigation/RandomMarker", "_Markers", "A random point in the scene used by Characters and objects.", "Marker"));
			}
			
			if (KickStarter.sceneSettings)
			{
				AC_NavigationMethod engine = KickStarter.sceneSettings.navigationMethod;
				if (engine == AC_NavigationMethod.meshCollider)
				{
					AddScenePrefab ("Navigation", new ScenePrefab ("NavMesh", "Navigation/NavMesh", "_NavMesh", "A mesh that defines the area that Characters can move in.", "NavigationMesh"));
				}
				else if (engine == AC_NavigationMethod.PolygonCollider || engine == AC_NavigationMethod.AStar2D)
				{
					AddScenePrefab ("Navigation", new ScenePrefab ("NavMesh 2D", "Navigation/NavMesh2D", "_NavMesh", "A polygon that defines the area that Characters can move in.", "NavigationMesh"));
				}
			}
			
			AddScenePrefab ("Navigation", new ScenePrefab ("Path", "Navigation/Path", "_Paths", "A sequence of points that describe a Character's movement.", "Paths"));
			
			if (SceneSettings.IsUnity2D ())
			{
				AddScenePrefab ("Navigation", new ScenePrefab ("PlayerStart 2D", "Navigation/PlayerStart2D", "_PlayerStarts", "A point in the scene from which the Player begins.", "PlayerStart"));
			}
			else
			{
				AddScenePrefab ("Navigation", new ScenePrefab ("PlayerStart", "Navigation/PlayerStart", "_PlayerStarts", "A point in the scene from which the Player begins.", "PlayerStart"));
			}

			foreach (SceneManagerPrefabData sceneManagerPrefabData in sceneManagerPrefabDatas)
			{
				if (sceneManagerPrefabData == null || !sceneManagerPrefabData.IsValid ()) continue;
				AddScenePrefab (sceneManagerPrefabData.Category, new ScenePrefab (sceneManagerPrefabData));
			}
		}


		private void OnCreateBefore_NPC ()
		{
			if (Selection.activeGameObject != null && Selection.activeGameObject.GetComponent <Renderer> () && Selection.activeGameObject.GetComponent<Char> () == null)
			{
				CharacterWizardWindow.InitForNPC (Selection.activeGameObject);
			}
			else
			{
				CharacterWizardWindow.InitForNPC ();
			}
		}


		private void OnCreateBefore_Player ()
		{
			if (Selection.activeGameObject != null && Selection.activeGameObject.GetComponent <Renderer> () && Selection.activeGameObject.GetComponent<Char> () == null)
			{
				CharacterWizardWindow.InitForPlayer (Selection.activeGameObject);
			}
			else
			{
				CharacterWizardWindow.InitForPlayer ();
			}
		}


		private void OnCreateBefore_Hotspot ()
		{
			positionHotspotOverMesh = null;

			if (Selection.activeGameObject != null && Selection.activeGameObject.GetComponent <Renderer> ())
			{
				if (EditorUtility.DisplayDialog ("Auto-set Hotspot position?", "The currently-selected object, " + Selection.activeGameObject.name + ", has a " +  ((Selection.activeGameObject.GetComponent <SpriteRenderer>() != null) ? "sprite" : "mesh") + ".  Position the new Hotspot over it?", "Yes", "No"))
				{
					positionHotspotOverMesh = Selection.activeGameObject.GetComponent <Renderer>();
				}
			}
		}


		private void OnCreateAfter_Hotspot (GameObject gameObject)
		{
			if (positionHotspotOverMesh)
			{
				gameObject.transform.position = positionHotspotOverMesh.bounds.center;
				Vector3 scale = positionHotspotOverMesh.bounds.size;
				scale.x = Mathf.Max (scale.x, 0.01f);
				scale.y = Mathf.Max (scale.y, 0.01f);
				scale.z = Mathf.Max (scale.z, 0.01f);
				gameObject.transform.localScale = scale;

				positionHotspotOverMesh = null;
			}
		}


		private void OnCreateAfter_Camera2D (GameObject gameObject)
		{
			if (gameObject.GetComponent <GameCamera2D>())
			{
				gameObject.GetComponent <GameCamera2D>().SetCorrectRotation ();
			}
		}


		private void AddScenePrefab (string categoryName, ScenePrefab scenePrefab)
		{
			foreach (ScenePrefabCollection scenePrefabCollection in scenePrefabCollections)
			{
				if (scenePrefabCollection.CategoryName == categoryName)
				{
					scenePrefabCollection.ScenePrefabs.Add (scenePrefab);
					return;
				}
			}

			ScenePrefabCollection newScenePrefabCollection = new ScenePrefabCollection (categoryName);
			newScenePrefabCollection.ScenePrefabs.Add (scenePrefab);
			scenePrefabCollections.Add (newScenePrefabCollection);
		}


		public void AddPrefab (SceneManagerPrefabData newData)
		{
			if (newData == null || !newData.IsValid ()) return;

			for (int i = 0; i < sceneManagerPrefabDatas.Count; i++)
			{
				if (sceneManagerPrefabDatas[i].Label == newData.Label && sceneManagerPrefabDatas[i].Category == newData.Category)
				{
					sceneManagerPrefabDatas.RemoveAt (i);
					ACDebug.LogWarning ("Scene Manager prefab with matching Label and Category data already installed - replacing..");
					break;
				}
			}

			sceneManagerPrefabDatas.Add (newData);
			RegneratePrefabCache ();
			EditorUtility.SetDirty (this);
		}
		

		private class ScenePrefabCollection
		{

			public readonly string CategoryName;
			public readonly List<ScenePrefab> ScenePrefabs;


			public ScenePrefabCollection (string categoryName)
			{
				CategoryName = categoryName;
				ScenePrefabs = new List<ScenePrefab> ();
			}

		}


		private class ScenePrefab
		{

			public readonly string label;
			public readonly string prefabPath;
			public readonly string sceneFolder;
			public readonly string helpText;
			public readonly string componentName;
			public readonly Texture2D icon;
			public readonly GameObject prefab;
			public readonly System.Action onCreateBefore;
			public readonly System.Action<GameObject> onCreateAfter;


			public ScenePrefab (string _label, string _prefabPath, string _sceneFolder, string _helpText, string _componentName, string _graphicName = "", System.Action _onCreateBefore = null, System.Action<GameObject> _onCreateAfter = null)
			{
				label = _label;
				prefabPath = _prefabPath;
				sceneFolder = _sceneFolder;
				helpText = _helpText;
				componentName = _componentName;
				onCreateBefore = _onCreateBefore;
				onCreateAfter = _onCreateAfter;
				
				if (!string.IsNullOrEmpty (_graphicName))
				{
					icon = (Texture2D) AssetDatabase.LoadAssetAtPath (Resource.MainFolderPath + "/Graphics/PrefabIcons/" + _graphicName +  ".png", typeof (Texture2D));
				}
				else
				{
					icon = (Texture2D) AssetDatabase.LoadAssetAtPath (Resource.MainFolderPath + "/Graphics/PrefabIcons/" + _componentName +  ".png", typeof (Texture2D));
				}
			}


			public ScenePrefab (SceneManagerPrefabData _sceneManagerPrefabData)
			{
				label = _sceneManagerPrefabData.Label;
				icon = _sceneManagerPrefabData.Icon;
				helpText = _sceneManagerPrefabData.Description;
				icon = _sceneManagerPrefabData.Icon;
				prefab = _sceneManagerPrefabData.Prefab;
			}

		}

		#endif
		
	}

}
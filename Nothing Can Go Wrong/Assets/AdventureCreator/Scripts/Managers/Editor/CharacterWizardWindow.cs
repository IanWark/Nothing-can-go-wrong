#if UNITY_EDITOR

using UnityEngine;
using UnityEngine.AI;
using UnityEditor;
using UnityEditor.Animations;

namespace AC
{

	public class CharacterWizardWindow : EditorWindow
	{

		private GameObject baseObject;
		private CharType charType;
		private enum CharType { Player, NPC };

		private bool isSpriteBased;

		private enum AnimationEngine2D { SpritesUnity, SpritesUnityComplex };

		private string characterName;
		private bool add2DCollision = true;
		private bool autoAddAnimatorStates = false;
		private bool canChooseCharType;

		private enum MotionControl3D { CharacterController, Rigidbody, NavMeshAgent, CustomScript };

		private NGWData.Option<AnimationEngine2D> animationEngine2DOption = new NGWData.Option<AnimationEngine2D> ();
		private NGWData.Option<MotionControl3D> motionControl3DOption = new NGWData.Option<MotionControl3D> ();
		
		private enum PageType { CharType, BaseGraphic, Configuration, Complete };
		private PageType currentPage;

		private int assignPlayerPrefabID = -1;

		private Rect pageRect = new Rect (350, 335, 150, 25);

		private const int Padding = 80;
		private Vector2 scrollPosition;

		private const int BottomButtonWidth = 140;
		private const int BottomButtonHeight = 40;


		[MenuItem ("Adventure Creator/Editors/Character wizard", false, 4)]
		public static void Init ()
		{
			CharacterWizardWindow window = EditorWindow.GetWindowWithRect <CharacterWizardWindow> (DefaultWindowRect, true, "Character Wizard", true);
			window.titleContent.text = "Character wizard";
			window.position = DefaultWindowRect;

			window.RebuildOptions ();
			window.canChooseCharType = true;
			window.SetPage (PageType.CharType);
		}


		public static void InitForNPC (GameObject defaultObject = null)
		{
			CharacterWizardWindow window = EditorWindow.GetWindowWithRect <CharacterWizardWindow> (DefaultWindowRect, true, "NPC Wizard", true);
			window.titleContent.text = "NPC wizard";
			window.position = DefaultWindowRect;
			window.charType = CharType.NPC;
			window.baseObject = defaultObject;

			window.RebuildOptions ();
			window.SetPage (PageType.BaseGraphic);
		}


		public static void InitForPlayer (GameObject defaultObject = null, int _assignPlayerPrefabID = -1)
		{
			CharacterWizardWindow window = EditorWindow.GetWindowWithRect <CharacterWizardWindow> (DefaultWindowRect, true, "Player Wizard", true);
			window.titleContent.text = "Player wizard";
			window.position = DefaultWindowRect;
			window.charType = CharType.Player;
			window.assignPlayerPrefabID = _assignPlayerPrefabID;
			window.baseObject = defaultObject;

			window.RebuildOptions ();

			if (window.IsFirstPerson && window.baseObject == null)
			{
				window.SetPage (PageType.Configuration);
			}
			else
			{
				window.SetPage (PageType.BaseGraphic);
			}
		}


		private void RebuildOptions ()
		{
			animationEngine2DOption.RebuildOptionData
			(
				new NGWData.OptionData<AnimationEngine2D>[]
				{
					new NGWData.OptionData<AnimationEngine2D> () { label = "Sprites Unity", description = "Animations are played by naming convention, without the need for parameters or transitions. Simple to set up, but flexibility is limited.", icon = null, value = AnimationEngine2D.SpritesUnity},
					new NGWData.OptionData<AnimationEngine2D> () { label = "Sprites Unity Complex", description = "Animations are played using parameters and transitions. More complex to set up, but offers a lot of flexibility.", icon = null, value = AnimationEngine2D.SpritesUnityComplex},
				}
			);
			
			motionControl3DOption.RebuildOptionData
			(
				new NGWData.OptionData<MotionControl3D>[]
				{
					new NGWData.OptionData<MotionControl3D> () { label = "Character Controller", description = "The character moves with a Character Controller.", icon = null, value = MotionControl3D.CharacterController},
					new NGWData.OptionData<MotionControl3D> () { label = "Rigidbody", description = "The character moves with a Rigidbody and Capsule Collider pairing.", icon = null, value = MotionControl3D.Rigidbody},
					new NGWData.OptionData<MotionControl3D> () { label = "NavMesh Agent", description = "The character moves using a NavMesh Agent.", icon = null, value = MotionControl3D.NavMeshAgent},
					new NGWData.OptionData<MotionControl3D> () { label = "Custom script", description = "The character has no built-in motion, and instead is reliant on a custom script.", icon = null, value = MotionControl3D.CustomScript},
				}
			);
		}
		
		
		private void OnGUI ()
		{
			EditorGUILayout.Separator ();
			GUILayout.Space (10f);

			GUI.Label (new Rect (0, 30, position.width, 60), GetTitle (), CustomStyles.managerHeader);
			GUI.Box (new Rect (Padding, 80, position.width - Padding - Padding, 0), "", CustomStyles.Header);

			ShowPage ();
			
			if ((canChooseCharType && currentPage != PageType.CharType) || (!canChooseCharType && currentPage != PageType.BaseGraphic) && currentPage != PageType.Complete && !IsFirstPerson)
			{
				if (ClickedBottomButton ((position.width - BottomButtonWidth) * 0.5f - 150, "Back"))
				{
					SetPage ((PageType) ((int) currentPage - 1));
				}
			}

			if (currentPage != PageType.Complete)
			{
				if (currentPage == PageType.CharType) return;

				if (currentPage == PageType.BaseGraphic)
				{
					if (!IsFirstPerson)
					{
						if (baseObject == null || baseObject.GetComponent <AC.Char>())
						{
							GUI.enabled = false;
						}
					}
				}

				if (currentPage == PageType.Configuration)
				{
					if (ClickedBottomButton ((position.width - BottomButtonWidth) * 0.5f + 150, "Create"))
					{
						Finish ();
						Close ();
						//SetPage ((PageType) ((int) currentPage + 1));
					}
				}
				else
				{
					if (ClickedBottomButton ((position.width - BottomButtonWidth) * 0.5f + 150, "Configure"))
					{
						SetPage ((PageType) ((int) currentPage + 1));
					}
				}

				GUI.enabled = true;
			}
			else
			{
				if (GUILayout.Button ("Close", EditorStyles.miniButtonRight))
				{
					Close ();
				}
				GUI.enabled = true;
			}
		}


		private void SetPage (PageType pageType)
		{
			if (currentPage == PageType.BaseGraphic && pageType == PageType.Configuration)
			{
				if (string.IsNullOrEmpty (characterName))
				{
					characterName = baseObject ? baseObject.name : charType.ToString ();
				}

				if (IsFirstPerson)
				{
					isSpriteBased = false;
				}
				else if (baseObject.GetComponentInChildren <SpriteRenderer> () || SceneSettings.CameraPerspective == CameraPerspective.TwoD)
				{
					isSpriteBased = true;
				}
				else
				{
					isSpriteBased = false;
				}
			}

			currentPage = pageType;

		}
		
		
		private string GetTitle ()
		{
			switch (currentPage)
			{
				case PageType.CharType:
					return "Character type";

				case PageType.BaseGraphic:
					return "Base graphic";

				default:
					return currentPage.ToString ();
			}
		}


		private void ConfigMotion3D (AC.Char character)
		{
			character.animationEngine = AnimationEngine.Mecanim;
			character.turn2DCharactersIn3DSpace = true;

			switch (motionControl3DOption.Value)
			{
				case MotionControl3D.CharacterController:
					if (character.GetComponent<CharacterController> () == null)
					{
						CharacterController characterController = character.gameObject.AddComponent <CharacterController>();
						characterController.center = new Vector3 (0f, 1f, 0f);
						characterController.height = 2f;
					}
					break;
					
				case MotionControl3D.Rigidbody:
					if (character.GetComponent<Rigidbody> () == null)
					{
						Rigidbody rigidbody = character.gameObject.AddComponent<Rigidbody> ();
					}
					if (character.GetComponent<CapsuleCollider> () == null)
					{
						CapsuleCollider capsuleCollider = character.gameObject.AddComponent <CapsuleCollider>();
						capsuleCollider.center = new Vector3 (0f, 1f, 0f);
						capsuleCollider.height = 2f;
					}
					break;
					
				case MotionControl3D.NavMeshAgent:
					if (character.GetComponent<CharacterController> () == null)
					{
						CharacterController characterController = character.gameObject.AddComponent <CharacterController>();
						characterController.center = new Vector3 (0f, 1f, 0f);
						characterController.height = 2f;
					}
					if (character.GetComponent<NavMeshAgent> () == null)
					{
						NavMeshAgent navMeshAgent = character.gameObject.AddComponent <NavMeshAgent>();
						navMeshAgent.height = 2f;
					}
					break;
					
				case MotionControl3D.CustomScript:
					character.motionControl = MotionControl.Manual;
					break;
			}
		}


		private void ConfigMotion2D (AC.Char character)
		{
			if (add2DCollision)
			{
				if (character.GetComponent<Collider2D> () == null)
				{
					character.gameObject.AddComponent <CircleCollider2D>();
				}
				if (character.GetComponent<Rigidbody2D> () == null)
				{
					Rigidbody2D rigidbody = character.gameObject.AddComponent<Rigidbody2D> ();
					rigidbody.gravityScale = 0f;
				}
			}

			character.turn2DCharactersIn3DSpace = false;
			character.ignoreGravity = true;
			FollowSortingMap followSortingMap = character.gameObject.AddComponent <FollowSortingMap>();
			followSortingMap.followSortingMap = true;
		}
		
		
		private void Finish ()
		{
			if (baseObject == null) baseObject = new GameObject ("New " + charType);
			
			if (baseObject)
			{
				if (UnityVersionHandler.IsPrefabFile (baseObject))
				{
					string originalName = baseObject.name;
					baseObject = Instantiate (baseObject);
					baseObject.name = originalName;
				}
				baseObject.SetActive (true);
			}

			if (string.IsNullOrEmpty (characterName))
			{
				characterName = baseObject.name;
			}

			GameObject newCharacterOb = baseObject;
			GameObject newBaseObject = isSpriteBased ? new GameObject (characterName) : baseObject;

			newBaseObject.transform.position = Vector3.zero;
			newBaseObject.transform.eulerAngles = Vector3.zero;
			newBaseObject.layer = LayerMask.NameToLayer ("Ignore Raycast");

			AC.Char character;
			switch (charType)
			{
				case CharType.Player:
				default:
					character = newBaseObject.AddComponent<Player> ();
					break;
				
				case CharType.NPC:
					character = newBaseObject.AddComponent<NPC> ();
					break;
			}

			character.speechLabel = characterName;

			if (IsFirstPerson)
			{
				ConfigMotion3D (character);

				GameObject cameraObject = new GameObject ("First person camera");
				cameraObject.transform.parent = newBaseObject.transform;
				cameraObject.transform.position = new Vector3 (0f, 1.5f, 0f);
				Camera cam = cameraObject.AddComponent <Camera>();
				cam.enabled = false;
				cameraObject.AddComponent <FirstPersonCamera>();

				return;
			}

			if (Is2D)
			{
				ConfigMotion2D (character);
			}
			else
			{
				ConfigMotion3D (character);
			}

			if (isSpriteBased)
			{
				newCharacterOb.transform.parent = newBaseObject.transform;
				newCharacterOb.transform.position = Vector3.zero;
				newCharacterOb.transform.eulerAngles = Vector3.zero;

				character.animationEngine = (animationEngine2DOption.Value == AnimationEngine2D.SpritesUnity) ? AnimationEngine.SpritesUnity : AnimationEngine.SpritesUnityComplex;
				character._spriteDirectionData = new SpriteDirectionData (true, false);

				if (newCharacterOb.GetComponent<Animator> () == null)
				{
					newCharacterOb.AddComponent<Animator> ();
				}

				if (autoAddAnimatorStates)
				{
					AutoAddAnimatorStates (newCharacterOb.GetComponent<Animator> ());
				}
			}
			else
			{
				if (newBaseObject.GetComponent<Animator> () == null)
				{
					newBaseObject.AddComponent<Animator> ();
				}
			}

			if (newBaseObject.GetComponent <AudioSource>() == null)
			{
				AudioSource baseAudioSource = newBaseObject.AddComponent <AudioSource>();
				baseAudioSource.playOnAwake = false;
			}

			if (newBaseObject.GetComponent <Paths>() == null)
			{
				newBaseObject.AddComponent <Paths>();
			}

			if (charType == CharType.NPC)
			{
				if (Is2D)
				{
					if (newCharacterOb.GetComponent<BoxCollider2D> () == null)
					{
						BoxCollider2D boxCollider = newCharacterOb.AddComponent <BoxCollider2D>();
						boxCollider.offset = new Vector2 (0f, 1f);
						boxCollider.size = new Vector2 (1f, 2f);
						boxCollider.isTrigger = true;
					}
				}
				else
				{
					if (newCharacterOb.GetComponent<Collider> () == null)
					{
						CapsuleCollider capsuleCollider = newCharacterOb.AddComponent <CapsuleCollider>();
						capsuleCollider.center = new Vector3 (0f, 1f, 0f);
						capsuleCollider.height = 2f;
					}
				}

				Hotspot hotspot = newCharacterOb.AddComponent <Hotspot>();
				hotspot.hotspotName = characterName;
			}

			if (Is2D)
			{
				character.spriteChild = newCharacterOb.transform;

				if (animationEngine2DOption.Value == AnimationEngine2D.SpritesUnity)
				{
					character.animationEngine = AnimationEngine.SpritesUnity;
				}
				else if (animationEngine2DOption.Value == AnimationEngine2D.SpritesUnityComplex)
				{
					character.animationEngine = AnimationEngine.SpritesUnityComplex;
				}
			}
			else
			{
				character.animationEngine = AnimationEngine.Mecanim;
			}

			if (charType == CharType.Player && KickStarter.settingsManager && KickStarter.settingsManager.hotspotDetection == HotspotDetection.PlayerVicinity)
			{
				GameObject detectorOb = new GameObject ("HotspotDetector");
				detectorOb.transform.parent = newBaseObject.transform;
				detectorOb.transform.position = Vector3.zero;
				detectorOb.AddComponent <DetectHotspots>();

				if (Is2D)
				{
					CircleCollider2D circleCollider = detectorOb.AddComponent <CircleCollider2D>();
					circleCollider.isTrigger = true;
				}
				else
				{
					SphereCollider sphereCollider = detectorOb.AddComponent <SphereCollider>();
					sphereCollider.isTrigger = true;
				}
			}


			GameObject soundChild = new GameObject ("Footsteps");
			soundChild.transform.parent = newBaseObject.transform;
			soundChild.transform.localPosition = Vector3.zero;
			AudioSource childAudioSource = soundChild.AddComponent <AudioSource>();
			childAudioSource.playOnAwake = false;
			Sound sound = soundChild.AddComponent <Sound>();

			FootstepSounds footstepSounds = character.gameObject.AddComponent<FootstepSounds> ();
			footstepSounds.soundToPlayFrom = sound;
			footstepSounds.character = character;

			if (charType == CharType.Player && assignPlayerPrefabID >= 0)
			{
				// Make a prefab and assign in Settings Manager
				string localPath = AssetInstallPath + newBaseObject.name + ".prefab";

				// Make sure the file name is unique, in case an existing Prefab has the same name.
				localPath = AssetDatabase.GenerateUniqueAssetPath (localPath);

				// Create the new Prefab and log whether Prefab was saved successfully.
				bool prefabSuccess;
				GameObject newPrefab = PrefabUtility.SaveAsPrefabAssetAndConnect (newBaseObject, localPath, InteractionMode.UserAction, out prefabSuccess);
				if (newPrefab)
				{

					var playerPrefab = KickStarter.settingsManager.playerSwitching == PlayerSwitching.DoNotAllow
									 ? KickStarter.settingsManager.PlayerPrefab
									 : KickStarter.settingsManager.GetPlayerPrefab (assignPlayerPrefabID);

					if (playerPrefab != null)
					{
						playerPrefab.playerOb = newPrefab.GetComponent<Player> ();
						baseObject = null;
						EditorGUIUtility.PingObject (newPrefab);
						return;
					}
				}
			}

			baseObject = null;
			EditorGUIUtility.PingObject (newBaseObject);
		}


		private void ShowPage ()
		{
			GUI.skin.label.wordWrap = true;
			
			switch (currentPage)
			{
				case PageType.CharType:
				{
					GUI.Label (new Rect (Padding + 20, 100, position.width - Padding - Padding - 40, 40), "Select the type of character to generate.");
					
					if (GUI.Button (new Rect (100, 200, BottomButtonWidth, BottomButtonHeight), "Player", ButtonStyle))
					{
						charType = CharType.Player;
						SetPage ((PageType) ((int) currentPage + 1));
					}

					if (GUI.Button (new Rect (position.width - BottomButtonWidth - 100, 200, BottomButtonWidth, BottomButtonHeight), "NPC", ButtonStyle))
					{
						charType = CharType.NPC;
						SetPage ((PageType) ((int) currentPage + 1));
					}
					break;
				}

				case PageType.BaseGraphic:
				{
					GameObject oldBaseObject = baseObject;
					GUI.Box (new Rect (Padding, 100, position.width - Padding - Padding, 120), "", CustomStyles.Header);
					GUI.Label (new Rect (Padding + 40, 100, 120, 20), "Base GameObject:", LabelStyle);
					baseObject = (GameObject) EditorGUI.ObjectField (new Rect (Padding + 20, 130, position.width - (Padding * 2f) - 40, 40), baseObject, typeof (GameObject), true);
					GUI.Label (new Rect (Padding + 20, 170, position.width - Padding - Padding - 40, 40), (IsFirstPerson ? "Optional) " : "") + "Assign a Renderer GameObject to use as the basis for the character.");
					
					GUI.Box (new Rect (Padding, 240, position.width - Padding - Padding, 90), "", CustomStyles.Header);
					GUI.Label (new Rect (Padding + 40, 240, 120, 20), "Character's name:", LabelStyle);
					characterName = GUI.TextField (new Rect (Padding + 20, 270, position.width - (Padding * 2f) - 40, 40), characterName, InputStyle);

					if (baseObject != null)
					{
						if (oldBaseObject == null && string.IsNullOrEmpty (characterName))
						{
							characterName = baseObject.name;
						}
					}
					break;
				}

				case PageType.Configuration:
					if (isSpriteBased)
					{
						GUI.Box (new Rect (Padding, 100, position.width - Padding - Padding, 80), "", CustomStyles.Header);
						animationEngine2DOption.SelectedIndex = EditorGUI.Popup (new Rect (Padding + 20, 105, 400, 20), "Animation engine:", animationEngine2DOption.SelectedIndex, animationEngine2DOption.Labels);
						GUI.Label (new Rect (Padding + 20, 130, position.width - Padding - Padding - 40, 40), animationEngine2DOption.Description);

						GUI.Box (new Rect (Padding, 200, position.width - Padding - Padding, 40), "", CustomStyles.Header);
						autoAddAnimatorStates = GUI.Toggle (new Rect (Padding + 20, 205, 200, 20), autoAddAnimatorStates, new GUIContent ("Auto-add animator states?", "If True, then the character's Animator will be populated with empty states for their standard animations, that can be replaced with proper animations later."));

						GUI.Box (new Rect (Padding, 260, position.width - Padding - Padding, 40), "", CustomStyles.Header);
						if (Is2D)
						{
							add2DCollision = GUI.Toggle (new Rect (Padding + 20, 265, 200, 20), add2DCollision, new GUIContent ("Add collision?", "If True, Collision components will be added to the character's root."));
						}
						else
						{
							GUI.Box (new Rect (Padding, 260, position.width - Padding - Padding, 80), "", CustomStyles.Header);
							motionControl3DOption.SelectedIndex = EditorGUI.Popup (new Rect (Padding + 20, 265, 400, 20), "Motion control:", motionControl3DOption.SelectedIndex, motionControl3DOption.Labels);
							GUI.Label (new Rect (Padding + 20, 290, position.width - Padding - Padding - 40, 40), motionControl3DOption.Description);
						}
					}
					else if (Is2D)
					{
						GUI.Box (new Rect (Padding, 100, position.width - Padding - Padding, 40), "", CustomStyles.Header);
						add2DCollision = GUI.Toggle (new Rect (Padding + 20, 105, position.width - (Padding * 2f) - 40, 20), add2DCollision, new GUIContent ("Add collision?", "If True, Collision components will be added to the character's root."));
					}
					else if (!Is2D)
					{
						GUI.Box (new Rect (Padding, 100, position.width - Padding - Padding, 80), "", CustomStyles.Header);
						motionControl3DOption.SelectedIndex = EditorGUI.Popup (new Rect (Padding + 20, 105, 400, 20), "Motion control:", motionControl3DOption.SelectedIndex, motionControl3DOption.Labels);
						GUI.Label (new Rect (Padding + 20, 130, position.width - Padding - Padding - 40, 40), motionControl3DOption.Description);
					}
					break;

				case PageType.Complete:
					GUILayout.Label ("Congratulations, your " + charType.ToString () + " has been created! Check the '" + charType.ToString () + "' Inspector to set up animation and other properties, as well as modify any generated Colliders / Rigidbody components.");
					if (charType == CharType.Player && assignPlayerPrefabID < 0)
					{
						GUILayout.Space (5f);
						GUILayout.Label ("To register this is as the main Player character, turn it into a prefab and assign it in the Settings Manager's 'Character' panel.");
					}
					break;

				default:
					break;
			}
		}


		private void ShowOptionGUI<T> (NGWData.Option<T> option, Rect position) where T : System.Enum
		{
			int numOptions = option.Labels.Length;
			int totalScrollViewHeight = 30 * numOptions;

			GUI.Box (new Rect (NewGameWizardWindow.Padding, 100, 315, totalScrollViewHeight + 40), "", CustomStyles.Header);

			scrollPosition = GUI.BeginScrollView (new Rect (NewGameWizardWindow.Padding + 20, 120, 285, 280), scrollPosition, new Rect (0, 0, NewGameWizardWindow.ScrollBoxWidth - 20, totalScrollViewHeight));

			string[] optionLabels = new string[numOptions];
			for (int i = 0; i < optionLabels.Length; i++)
			{
				optionLabels[i] = option.Labels[i];
			}

			option.SelectedIndex = GUI.SelectionGrid (new Rect (0, 0, 275, totalScrollViewHeight), option.SelectedIndex, optionLabels, 1, NewGameWizardWindow.ButtonStyle);

			GUI.EndScrollView ();

			// Details box
			var optionData = option.optionDatas[option.SelectedIndex];
			GUI.Box (new Rect (position.width - NewGameWizardWindow.PreviewImageWidth - 35 - NewGameWizardWindow.Padding, 100, NewGameWizardWindow.PreviewImageWidth + 40, 320), "", CustomStyles.Header);

			GUI.DrawTexture (new Rect (position.width - NewGameWizardWindow.PreviewImageWidth - NewGameWizardWindow.Padding - 15, 120, NewGameWizardWindow.PreviewImageWidth, NewGameWizardWindow.PreviewImageHeight), optionData.icon, ScaleMode.StretchToFill);
			GUI.Label (new Rect (position.width - NewGameWizardWindow.PreviewImageWidth - NewGameWizardWindow.Padding - 15, 320, NewGameWizardWindow.PreviewImageWidth, 40), optionData.label, CustomStyles.managerHeader);
			bool wordWrapBackup = GUI.skin.label.wordWrap;
			GUI.skin.label.wordWrap = true;
			GUI.Label (new Rect (position.width - NewGameWizardWindow.PreviewImageWidth - NewGameWizardWindow.Padding - 15, 340, NewGameWizardWindow.PreviewImageWidth, 80), optionData.description, NewGameWizardWindow.LabelStyle);
			GUI.skin.label.wordWrap = wordWrapBackup;
		}


		private void AutoAddAnimatorStates (Animator animator)
		{
			var controller = AnimatorController.CreateAnimatorControllerAtPath (AssetInstallPath + charType + "_" + characterName + ".controller");
			var rootStateMachine = controller.layers[0].stateMachine;

			switch (animationEngine2DOption.Value)
			{
				case AnimationEngine2D.SpritesUnity:
				{
					Vector3 startPosition = new Vector3 (50, 200);
					float verticalSeparation = -60;
					float horizontalSeparation = 220;
					rootStateMachine.AddState ("idle_D", startPosition);
					rootStateMachine.AddState ("idle_U", startPosition + (Vector3.down * verticalSeparation));
					rootStateMachine.AddState ("idle_L", startPosition + (Vector3.down * verticalSeparation * 2));
					rootStateMachine.AddState ("idle_R", startPosition + (Vector3.down * verticalSeparation * 3));

					rootStateMachine.AddState ("walk_D", startPosition + (Vector3.right * horizontalSeparation));
					rootStateMachine.AddState ("walk_U", startPosition + (Vector3.right * horizontalSeparation) + (Vector3.down * verticalSeparation));
					rootStateMachine.AddState ("walk_L", startPosition + (Vector3.right * horizontalSeparation) + (Vector3.down * verticalSeparation) * 2);
					rootStateMachine.AddState ("walk_R", startPosition + (Vector3.right * horizontalSeparation) + (Vector3.down * verticalSeparation) * 3);

					rootStateMachine.AddState ("talk_D", startPosition + (Vector3.right * horizontalSeparation * 2));
					rootStateMachine.AddState ("talk_U", startPosition + (Vector3.right * horizontalSeparation * 2) + (Vector3.down * verticalSeparation));
					rootStateMachine.AddState ("talk_L", startPosition + (Vector3.right * horizontalSeparation * 2) + (Vector3.down * verticalSeparation) * 2);
					rootStateMachine.AddState ("talk_R", startPosition + (Vector3.right * horizontalSeparation * 2) + (Vector3.down * verticalSeparation) * 3);
					break;
				}

				case AnimationEngine2D.SpritesUnityComplex:
				{
					controller.AddParameter ("Speed", AnimatorControllerParameterType.Float);
					controller.AddParameter ("Direction", AnimatorControllerParameterType.Int);
					controller.AddParameter ("Angle", AnimatorControllerParameterType.Float);
					controller.AddParameter ("IsTalking", AnimatorControllerParameterType.Bool);

					BlendTree idleBlendTree;
					var idleState = controller.CreateBlendTreeInController ("Idle", out idleBlendTree);
					idleBlendTree.blendType = BlendTreeType.Simple1D;
					idleBlendTree.blendParameter = "Angle";
					idleBlendTree.useAutomaticThresholds = false;
					idleBlendTree.AddChild (null, 0f);
					idleBlendTree.AddChild (null, 90f);
					idleBlendTree.AddChild (null, 180f);
					idleBlendTree.AddChild (null, 270f);
					idleBlendTree.AddChild (null, 360f);

					BlendTree walkBlendTree;
					var walkState = controller.CreateBlendTreeInController ("Walk", out walkBlendTree);
					walkBlendTree.blendType = BlendTreeType.Simple1D;
					walkBlendTree.blendParameter = "Angle";
					walkBlendTree.useAutomaticThresholds = false;
					walkBlendTree.AddChild (null, 0f);
					walkBlendTree.AddChild (null, 90f);
					walkBlendTree.AddChild (null, 180f);
					walkBlendTree.AddChild (null, 270f);
					walkBlendTree.AddChild (null, 360f);

					BlendTree talkBlendTree;
					var talkState = controller.CreateBlendTreeInController ("Talk", out talkBlendTree);
					talkBlendTree.blendType = BlendTreeType.Simple1D;
					talkBlendTree.blendParameter = "Angle";
					talkBlendTree.useAutomaticThresholds = false;
					talkBlendTree.AddChild (null, 0f);
					talkBlendTree.AddChild (null, 90f);
					talkBlendTree.AddChild (null, 180f);
					talkBlendTree.AddChild (null, 270f);
					talkBlendTree.AddChild (null, 360f);

					var idleTalkTransition = idleState.AddTransition (talkState, false);
					idleTalkTransition.AddCondition (UnityEditor.Animations.AnimatorConditionMode.If, 0, "IsTalking");
					idleTalkTransition.duration = 0;
					idleTalkTransition.hasExitTime = false;

					var talkIdleTransition = talkState.AddTransition (idleState, false);
					talkIdleTransition.AddCondition (UnityEditor.Animations.AnimatorConditionMode.IfNot, 0, "IsTalking");
					talkIdleTransition.duration = 0;
					talkIdleTransition.hasExitTime = false;

					var idleWalkTransition = idleState.AddTransition (walkState, false);
					idleWalkTransition.AddCondition (UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "Speed");
					idleWalkTransition.duration = 0;
					idleWalkTransition.hasExitTime = false;

					var walkIdleTransition = walkState.AddTransition (idleState, false);
					walkIdleTransition.AddCondition (UnityEditor.Animations.AnimatorConditionMode.Less, 0.1f, "Speed");
					walkIdleTransition.duration = 0;
					walkIdleTransition.hasExitTime = false;
					break;
				}

				default:
					break;
			}

			//AssetDatabase.SaveAssets ();
			animator.runtimeAnimatorController = controller;
		}


		private bool ClickedBottomButton (float posX, string label)
		{
			return GUI.Button (new Rect (posX, position.height - BottomButtonHeight - 50, BottomButtonWidth, BottomButtonHeight), label, ButtonStyle);
		}


		private bool Is2D
		{
			get
			{
				return SceneSettings.IsUnity2D ();
			}
		}

		
		private bool IsFirstPerson 
		{
			get
			{
				return !Is2D && charType == CharType.Player && KickStarter.settingsManager && KickStarter.settingsManager.movementMethod == MovementMethod.FirstPerson;
			}
		}


		private string AssetInstallPath
		{
			get
			{
				string installPath = "Assets/";
				if (KickStarter.settingsManager)
				{
					installPath = AssetDatabase.GetAssetPath (KickStarter.settingsManager);
					installPath = installPath.Substring (0, installPath.LastIndexOf ("/"));
					installPath = installPath.Substring (0, installPath.LastIndexOf ("/") + 1);
				}
				return installPath;
			}
		}


		private static Rect DefaultWindowRect { get { return new Rect (300, 200, 600, 440); }}

		private GUIStyle InputStyle { get { return Resource.NodeSkin.customStyles[37]; }}
		public static GUIStyle LabelStyle { get { return Resource.NodeSkin.customStyles[EditorGUIUtility.isProSkin ? 38 : 39]; }}
		public static GUIStyle ButtonStyle { get { return Resource.NodeSkin.customStyles[40]; }}

	}

}

#endif
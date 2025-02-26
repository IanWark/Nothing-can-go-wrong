﻿#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

namespace AC
{

	[CustomEditor (typeof (Player))]
	public class PlayerEditor : NPCEditor
	{

		public override void OnInspectorGUI ()
		{
			Player _target = (Player) target;
			
			SharedGUIOne (_target);

			SettingsManager settingsManager = KickStarter.settingsManager;
			if (settingsManager != null && settingsManager.playerSwitching == PlayerSwitching.Allow)
			{
				NPC_GUI (_target);
			}

			SharedGUITwo (_target);

			if (settingsManager && (settingsManager.hotspotDetection == HotspotDetection.PlayerVicinity || settingsManager.playerSwitching == PlayerSwitching.Allow || (SceneSettings.IsUnity2D () && KickStarter.settingsManager.movementMethod != MovementMethod.PointAndClick)))
			{
				CustomGUILayout.Header ("Player settings");
				CustomGUILayout.BeginVertical ();

				if (settingsManager.hotspotDetection == HotspotDetection.PlayerVicinity)
				{
					_target.hotspotDetector = (DetectHotspots) CustomGUILayout.ObjectField <DetectHotspots> ("Hotspot detector child:", _target.hotspotDetector, true, "", "The DetectHotspots component to rely on for hotspot detection. This should be a child object of the Player.");
				}

				#if UNITY_2019_2_OR_NEWER
				if (SceneSettings.IsUnity2D () && KickStarter.settingsManager.movementMethod != MovementMethod.PointAndClick)
				{
					_target.autoStickToNavMesh = CustomGUILayout.Toggle ("Auto-stick to NavMesh?", _target.autoStickToNavMesh, string.Empty, "If True, the Player will stick to the 2D NavMesh when moving under Direct control.");
				}
				#endif

				if (settingsManager.playerSwitching == PlayerSwitching.Allow)
				{
					_target.autoSyncHotspotState = CustomGUILayout.Toggle ("Auto-sync Hotspot state?", _target.autoSyncHotspotState, "", "If True, then any attached Hotspot will be made inactive while this character is the current active Player");
				}

				if (KickStarter.settingsManager.movementMethod == MovementMethod.Direct || KickStarter.settingsManager.movementMethod == MovementMethod.FirstPerson)
				{
					_target.jumpSpeed = CustomGUILayout.Slider ("Jump speed:", _target.jumpSpeed, 1f, 20f, "", "The player's jump speed");
				}

				CustomGUILayout.EndVertical ();
			}
			else if (KickStarter.settingsManager && (KickStarter.settingsManager.movementMethod == MovementMethod.Direct || KickStarter.settingsManager.movementMethod == MovementMethod.FirstPerson))
			{
				CustomGUILayout.Header ("Player settings");
				CustomGUILayout.BeginVertical ();
				_target.jumpSpeed = CustomGUILayout.Slider ("Jump speed:", _target.jumpSpeed, 1f, 20f, "", "The player's jump speed");
				CustomGUILayout.EndVertical ();
			}

			if (Application.isPlaying && _target.gameObject.activeInHierarchy)
			{
				CustomGUILayout.Header ("Current inventory");
				CustomGUILayout.BeginVertical ();

				bool isCarrying = false;

				if (KickStarter.saveSystem != null)
				{
					if ((_target.IsLocalPlayer () ||
						KickStarter.settingsManager.playerSwitching == PlayerSwitching.DoNotAllow ||
						_target.ID == KickStarter.saveSystem.CurrentPlayerID ||
						KickStarter.settingsManager.shareInventory))
					{
						if (KickStarter.runtimeInventory != null &&	KickStarter.runtimeInventory.localItems != null)
						{
							if (ListItems (KickStarter.runtimeInventory.PlayerInvCollection))
							{
								isCarrying = true;
							}
						}

						if (KickStarter.inventoryManager != null && KickStarter.runtimeDocuments != null && KickStarter.runtimeDocuments.GetCollectedDocumentIDs () != null)
						{
							for (int i=0; i<KickStarter.runtimeDocuments.GetCollectedDocumentIDs ().Length; i++)
							{
								Document document = KickStarter.inventoryManager.GetDocument (KickStarter.runtimeDocuments.GetCollectedDocumentIDs ()[i]);

								if (document != null)
								{
									isCarrying = true;

									EditorGUILayout.BeginHorizontal ();
									EditorGUILayout.LabelField ("Document:", GUILayout.Width (80f));
									EditorGUILayout.LabelField (document.Title, EditorStyles.boldLabel);
									EditorGUILayout.EndHorizontal ();
								}
							}
						}

						if (KickStarter.inventoryManager != null && KickStarter.runtimeObjectives != null)
						{
							ObjectiveInstance[] objectiveInstances = KickStarter.runtimeObjectives.GetObjectives ();
							foreach (ObjectiveInstance objectiveInstance in objectiveInstances)
							{
								EditorGUILayout.BeginHorizontal ();
								EditorGUILayout.LabelField ("Objective:", GUILayout.Width (80f));
								EditorGUILayout.LabelField (objectiveInstance.Objective.GetTitle () + ": " + objectiveInstance.CurrentState.GetLabel (), EditorStyles.boldLabel);
								EditorGUILayout.EndHorizontal ();
							}
						}
					}
					else
					{
						PlayerData playerData = KickStarter.saveSystem.GetPlayerData (_target.ID);
						if (playerData != null)
						{
							if (ListItems (InvCollection.LoadData (playerData.inventoryData)))
							{
								isCarrying = true;
							}

							if (!string.IsNullOrEmpty (playerData.collectedDocumentData))
							{
								EditorGUILayout.LabelField ("Documents:", playerData.collectedDocumentData);
							}
							if (!string.IsNullOrEmpty (playerData.playerObjectivesData))
							{
								EditorGUILayout.LabelField ("Objectives:", playerData.playerObjectivesData);
							}
						}
					}
				}

				if (!isCarrying)
				{
					EditorGUILayout.HelpBox ("This Player is not carrying any items.", MessageType.Info);
				}

				CustomGUILayout.EndVertical ();
			}
			
			UnityVersionHandler.CustomSetDirty (_target);
		}


		private bool ListItems (InvCollection invCollection)
		{
			bool isCarrying = false;
			foreach (InvInstance invInstance in invCollection.InvInstances)
			{
				if (InvInstance.IsValid (invInstance))
				{
					isCarrying = true;

					EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.LabelField ("Item:", GUILayout.Width (80f));
					if (invInstance.InvItem.canCarryMultiple)
					{
						EditorGUILayout.LabelField (invInstance.InvItem.label, EditorStyles.boldLabel, GUILayout.Width (135f));
						EditorGUILayout.LabelField ("Count:", GUILayout.Width (50f));
						EditorGUILayout.LabelField (invInstance.Count.ToString (), GUILayout.Width (44f));
					}
					else
					{
						EditorGUILayout.LabelField (invInstance.InvItem.label, EditorStyles.boldLabel);
					}
					EditorGUILayout.EndHorizontal ();
				}
			}

			return isCarrying;
		}

	}

}

#endif
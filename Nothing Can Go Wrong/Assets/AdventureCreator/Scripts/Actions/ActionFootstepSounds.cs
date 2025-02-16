/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2024
 *	
 *	"ActionFootstepSounds.cs"
 * 
 *	This Action changes the sounds listed in a FootstepSounds component.
 * 
 */

using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{
	
	[System.Serializable]
	public class ActionFootstepSounds : Action
	{

		public int constantID = 0;
		public int parameterID = -1;
		public FootstepSounds footstepSounds;
		protected FootstepSounds runtimeFootstepSounds;

		public bool isPlayer;
		public int playerID = -1;

		public int surfaceID = 0;
		public int surfaceIDParameterID = -1;
		protected Surface runtimeSurface;

		private enum FootstepSoundType { Walk, Run };
		[SerializeField] private FootstepSoundType footstepSoundType = FootstepSoundType.Walk;
		[SerializeField] private AudioClip[] newSounds;


		public override ActionCategory Category { get { return ActionCategory.Sound; }}
		public override string Title { get { return "Change footsteps"; }}
		public override string Description { get { return "Changes the sounds used by a FootstepSounds component."; }}


		public override void AssignValues (List<ActionParameter> parameters)
		{
			if (footstepSoundType == FootstepSoundType.Walk) {}
			
			if (isPlayer)
			{
				Player player = AssignPlayer (playerID, parameters, parameterID);
				if (player != null)
				{
					runtimeFootstepSounds = player.GetComponentInChildren <FootstepSounds>();
				}
			}
			else
			{
				runtimeFootstepSounds = AssignFile <FootstepSounds> (parameters, parameterID, constantID, footstepSounds);
			}

			surfaceID = AssignInteger (parameters, surfaceIDParameterID, surfaceID);
			runtimeSurface = KickStarter.settingsManager.GetSurface (surfaceID);
		}


		public override float Run ()
		{
			if (runtimeFootstepSounds == null)
			{
				LogWarning ("No FootstepSounds component set.");
			}
			else if (runtimeSurface == null)
			{
				LogWarning ("Surface with ID " + surfaceID + " not found");
			}
			else
			{
				runtimeFootstepSounds.CurrentSurface = runtimeSurface;
			}

			return 0f;
		}


		#if UNITY_EDITOR
		
		public override void ShowGUI (List<ActionParameter> parameters)
		{
			isPlayer = EditorGUILayout.Toggle ("Change Player's?", isPlayer);
			if (isPlayer)
			{
				PlayerField (ref playerID, parameters, ref parameterID);
			}
			else
			{
				ComponentField ("FootstepSounds:", ref footstepSounds, ref constantID, parameters, ref parameterID);
			}

			if (GUILayout.Button ("Surfaces window"))
			{
				SurfacesEditor.Init ();
			}

			int tempNumber = -1;

			if (KickStarter.settingsManager != null && KickStarter.settingsManager.surfaces != null && KickStarter.settingsManager.surfaces.Count > 0)
			{
				string[] labelList = new string[KickStarter.settingsManager.surfaces.Count];
				for (int i=0; i<KickStarter.settingsManager.surfaces.Count; i++)
				{
					labelList[i] = i.ToString () + ": " + KickStarter.settingsManager.surfaces[i].label;

					if (KickStarter.settingsManager.surfaces[i].ID == surfaceID)
					{
						tempNumber = i;
					}
				}

				if (tempNumber == -1)
				{
					// Wasn't found (was deleted?), so revert to zero
					if (surfaceID != 0)
						LogWarning ("Previously chosen Surface no longer exists!");
					tempNumber = 0;
					surfaceID = 0;
				}

				tempNumber = EditorGUILayout.Popup ("Surface:", tempNumber, labelList);
				surfaceID = KickStarter.settingsManager.surfaces [tempNumber].ID;
			}
			else
			{
				EditorGUILayout.HelpBox ("No Surfaces exist!", MessageType.Info);
				surfaceID = 0;
				tempNumber = 0;
			}
		}


		public override void AssignConstantIDs (bool saveScriptsToo, bool fromAssetFile)
		{
			FootstepSounds obToUpdate = footstepSounds;
			if (isPlayer && (KickStarter.settingsManager == null || KickStarter.settingsManager.playerSwitching == PlayerSwitching.DoNotAllow))
			{
				if (!fromAssetFile)
				{
					Player _player = UnityVersionHandler.FindObjectOfType<Player> ();
					if (_player)
					{
						obToUpdate = _player.GetComponentInChildren <FootstepSounds>();
					}
				}

				if (obToUpdate == null && KickStarter.settingsManager != null)
				{
					Player player = KickStarter.settingsManager.GetDefaultPlayer ();
					obToUpdate = player.GetComponentInChildren <FootstepSounds>();
				}
			}

			if (saveScriptsToo)
			{
				AddSaveScript <RememberFootstepSounds> (obToUpdate);
			}
			constantID = AssignConstantID<FootstepSounds> (obToUpdate, constantID, parameterID);
		}
		
		
		public override string SetLabel ()
		{
			if (parameterID == -1)
			{
				if (isPlayer)
				{
					return "Player";
				}
				else if (footstepSounds)
				{
					return footstepSounds.gameObject.name;
				}
			}
			return string.Empty;
		}


		public override bool ReferencesObjectOrID (GameObject _gameObject, int id)
		{
			if (!isPlayer && parameterID < 0)
			{
				if (footstepSounds && footstepSounds.gameObject == _gameObject) return true;
				if (constantID == id) return true;
			}
			if (isPlayer && _gameObject && _gameObject.GetComponent <Player>()) return true;
			return base.ReferencesObjectOrID (_gameObject, id);
		}


		public override bool ReferencesPlayer (int _playerID = -1)
		{
			if (!isPlayer) return false;
			if (_playerID < 0) return true;
			if (playerID < 0 && parameterID < 0) return true;
			return (parameterID < 0 && playerID == _playerID);
		}

		#endif


		/**
		 * <summary>Creates a new instance of the 'Sound: Change footsteps' Action</summary>
		 * <param name = "footstepSoundsToModify">The FootstepSounds component to affect</param>
		 * <param name = "newSurfaceID">The ID of the new surface to use</param>
		 * <returns>The generated Action</returns>
		 */
		public static ActionFootstepSounds CreateNew (FootstepSounds footstepSoundsToModify, int newSurfaceID)
		{
			ActionFootstepSounds newAction = CreateNew<ActionFootstepSounds> ();
			newAction.footstepSounds = footstepSoundsToModify;
			newAction.TryAssignConstantID (newAction.footstepSounds, ref newAction.constantID);
			newAction.surfaceID = newSurfaceID;
			return newAction;
		}
		
	}
	
}
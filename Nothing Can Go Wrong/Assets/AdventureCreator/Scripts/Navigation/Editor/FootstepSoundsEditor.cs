#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace AC
{

	[CustomEditor(typeof(FootstepSounds))]
	public class FootstepSoundsEditor : Editor
	{
		
		public override void OnInspectorGUI ()
		{
			FootstepSounds _target = (FootstepSounds) target;

			if (KickStarter.settingsManager && KickStarter.settingsManager.surfaces.Count == 0)
			{
				EditorGUILayout.HelpBox ("No Surfaces defined - open up the Surfaces window below to define them.", MessageType.Info);
			}

			if (GUILayout.Button ("Surfaces window"))
			{
				SurfacesEditor.Init ();
			}

			EditorGUILayout.Space ();

			CustomGUILayout.Header ("Properties");
			CustomGUILayout.BeginVertical ();
			_target.character = (Char) CustomGUILayout.ObjectField <Char> ("Character:", _target.character, true, string.Empty, "The Player or NPC that this component is for");
			if (_target.character != null || _target.GetComponent <Char>())
			{
				_target.doGroundedCheck = CustomGUILayout.ToggleLeft ("Only play when grounded?", _target.doGroundedCheck, string.Empty, "If True, sounds will only play when the character is grounded");

				if (_target.footstepPlayMethod == FootstepSounds.FootstepPlayMethod.ViaAnimationEvents)
				{
					_target.doMovementCheck = CustomGUILayout.ToggleLeft ("Only play when moving?", _target.doMovementCheck, string.Empty, "If True, sounds will only play when the character is walking or running");
				}
			}
			_target.soundToPlayFrom = (Sound) CustomGUILayout.ObjectField <Sound> ("Sound to play from:", _target.soundToPlayFrom, true, "", "The Sound object to play from");

			_target.footstepPlayMethod = (FootstepSounds.FootstepPlayMethod) CustomGUILayout.EnumPopup ("Play sounds:", _target.footstepPlayMethod, "", "How the sounds are played");
			if (_target.footstepPlayMethod == FootstepSounds.FootstepPlayMethod.Automatically)
			{
				_target.walkSeparationTime = CustomGUILayout.Slider ("Walk separation (s):", _target.walkSeparationTime, 0f, 3f, string.Empty, "The separation time between sounds when walking");
				_target.runSeparationTime = CustomGUILayout.Slider ("Run separation (s):", _target.runSeparationTime, 0f, 3f, string.Empty, "The separation time between sounds when running");
			}
			else if (_target.footstepPlayMethod == FootstepSounds.FootstepPlayMethod.ViaAnimationEvents)
			{
				EditorGUILayout.HelpBox ("A sound will be played whenever this component's PlayFootstep function is run. This component should be placed on the same GameObject as the Animator.", MessageType.Info);
			}
			_target.pitchVariance = CustomGUILayout.Slider ("Pitch variance:", _target.pitchVariance, 0f, 0.8f, string.Empty, "How much the audio pitch can randomly vary by.");
			_target.volumeVariance = CustomGUILayout.Slider ("Volume variance:", _target.volumeVariance, 0f, 0.8f, string.Empty, "How much the audio volume can randomly vary by.");
			CustomGUILayout.EndVertical ();

			if (_target.soundToPlayFrom == null && _target.GetComponent <AudioSource>() == null)
			{
				EditorGUILayout.HelpBox ("To play sounds, the 'Sound to play from' must be assigned, or an AudioSource must be attached.", MessageType.Warning);
			}

			EditorGUILayout.Space ();

			CustomGUILayout.Header ("Auto-detection");
			CustomGUILayout.BeginVertical ();
			_target.autoDetectSurface = CustomGUILayout.Toggle ("Auto-detect Surface?", _target.autoDetectSurface, string.Empty, ("If True, raycasting will be used to auto-detect the current Surface"));
			if (_target.autoDetectSurface)
			{
				_target.layerMask = AdvGame.LayerMaskField ("Layer mask:", _target.layerMask);
				_target.raycastLength = CustomGUILayout.FloatField ("Ray length:", _target.raycastLength, string.Empty, "How long raycasts should be");

				EditorGUILayout.HelpBox ("Auto-detection is based on the floor collider's Physics Material, whose name must end with '_' followed by the surface name, e.g. '_Grass'.", MessageType.Info);
			}
			CustomGUILayout.EndVertical ();
			EditorGUILayout.Space ();


			UnityVersionHandler.CustomSetDirty (_target);
		}

	}

}

#endif
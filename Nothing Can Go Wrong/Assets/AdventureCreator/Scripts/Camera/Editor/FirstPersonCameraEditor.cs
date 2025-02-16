#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

namespace AC
{

	[CustomEditor (typeof (FirstPersonCamera))]
	public class FirstPersonCameraEditor : Editor
	{
		
		private static GUILayoutOption
			labelWidth = GUILayout.MaxWidth (60),
			intWidth = GUILayout.MaxWidth (130);
		
		
		public override void OnInspectorGUI ()
		{
			FirstPersonCamera _target = (FirstPersonCamera) target;
			
			CustomGUILayout.Header ("Animation");
			CustomGUILayout.BeginVertical ();

			_target.headBob = EditorGUILayout.Toggle ("Bob head when moving?", _target.headBob);
			if (_target.headBob)
			{
				_target.headBobMethod = (FirstPersonHeadBobMethod) EditorGUILayout.EnumPopup ("Head-bob method:", _target.headBobMethod);
				if (_target.headBobMethod == FirstPersonHeadBobMethod.BuiltIn)
				{
					_target.builtInSpeedFactor = CustomGUILayout.FloatField ("Speed factor:", _target.builtInSpeedFactor);
					_target.bobbingAmount = CustomGUILayout.FloatField ("Height change factor:", _target.bobbingAmount);
					_target.headBobTransform = (Transform) CustomGUILayout.ObjectField<Transform> ("Transform:", _target.headBobTransform, true);
				}
				else if (_target.headBobMethod == FirstPersonHeadBobMethod.CustomAnimation)
				{
					_target.headBobSpeedParameter = CustomGUILayout.TextField ("Forward speed float:", _target.headBobSpeedParameter);
					_target.headBobSpeedSideParameter = CustomGUILayout.TextField ("Sideways speed float:", _target.headBobSpeedSideParameter);
					_target.headBobLerpSpeed = CustomGUILayout.FloatField ("Paramater value acceleration:", _target.headBobLerpSpeed);
					if (_target.GetComponent <Animator>() == null)
					{
						EditorGUILayout.HelpBox ("This GameObject must have an Animator component attached.", MessageType.Warning);
					}
				}
				else if (_target.headBobMethod == FirstPersonHeadBobMethod.CustomScript)
				{
					EditorGUILayout.HelpBox ("The component's public method 'GetHeadBobSpeed' will return the desired head-bobbing speed.", MessageType.Info);
				}
			}
			CustomGUILayout.EndVertical ();
			
			CustomGUILayout.Header ("Input");
			CustomGUILayout.BeginVertical ();
			_target.allowMouseWheelZooming = EditorGUILayout.Toggle ("Allow scrollwheel zooming?", _target.allowMouseWheelZooming);
			if (_target.allowMouseWheelZooming)
			{
				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.LabelField ("Min FOV:", labelWidth);
				_target.minimumZoom = CustomGUILayout.FloatField (_target.minimumZoom, intWidth);
				EditorGUILayout.LabelField ("Max FOV:", labelWidth);
				_target.maximumZoom = CustomGUILayout.FloatField (_target.maximumZoom, intWidth);
				EditorGUILayout.EndHorizontal ();
			}
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField ("Min pitch:", labelWidth);
			_target.minY = CustomGUILayout.FloatField (_target.minY, intWidth);
			EditorGUILayout.LabelField ("Max pitch:", labelWidth);
			_target.maxY = CustomGUILayout.FloatField (_target.maxY, intWidth);
			EditorGUILayout.EndHorizontal ();
			_target.sensitivity = CustomGUILayout.Vector2Field ("Freelook sensitivity:", _target.sensitivity);
			CustomGUILayout.EndVertical ();

			UnityVersionHandler.CustomSetDirty (_target);
		}
		
	}

}

#endif
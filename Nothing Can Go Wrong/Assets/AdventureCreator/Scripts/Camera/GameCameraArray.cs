/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2024
 *	
 *	"GameCameraArray.cs"
 * 
 *	A camera type that works by interpolating its position against an array of child cameras, that each have a "target position" used for weighting.
 * 
 */
 
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	#if UNITY_EDITOR

	[CustomEditor(typeof(GameCameraArray))]
	public class GameCameraArrayEditor : Editor
	{

		public override void OnInspectorGUI ()
		{
			GameCameraArray _target = (GameCameraArray) target;
			_target.ShowGUI ();
			if (GUI.changed)
			{
				UnityVersionHandler.CustomSetDirty (_target);
			}
		}

	}

	#endif


	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_game_camera_array.html")]
    public class GameCameraArray : CursorInfluenceCamera
    {

		#region Variables

		/** The follow speed when tracking a target */
		[Range (0f, 1f)] public float dampSpeed = 0.9f;

		private CameraNode[] nodes = new CameraNode[0];
		private float[] weights;
		
		#endregion


		#region UnityStandards

		protected override void OnEnable ()
		{
			nodes = GetComponentsInChildren<CameraNode> ();
			GameObject nodeParentOb = new GameObject (gameObject.name + "_Nodes");
			Transform nodeParent = nodeParentOb.transform;
			foreach (CameraNode node in nodes)
			{
				if (node.Camera) node.Camera.enabled = false;
				node.transform.SetParent (nodeParent);
			}
			weights = new float[nodes.Length];

			base.OnEnable ();
		}

		#endregion


		#region PublicFunctions

		public override void MoveCameraInstant ()
		{
			if (target == null) return;

			var cameraData = GetInterpolatedCameraData (target.position);
			transform.SetPositionAndRotation (cameraData.Position, cameraData.Rotation);
			Camera.fieldOfView = cameraData.FieldOfView;
			Camera.orthographicSize = cameraData.OrthographicSize;
		}


		public override void _Update ()
		{
			if (target == null) return;
			var cameraData = GetInterpolatedCameraData (target.position);

			Vector3 position = Vector3.Lerp (transform.position, cameraData.Position, LerpSpeed);
			Quaternion rotation = Quaternion.Lerp (transform.rotation, cameraData.Rotation, LerpSpeed);
			float fieldOfView = Mathf.Lerp (Camera.fieldOfView, cameraData.FieldOfView, LerpSpeed);
			float orthographicSize = Mathf.Lerp (Camera.orthographicSize, cameraData.OrthographicSize, LerpSpeed);

			transform.SetPositionAndRotation (position, rotation);
			Camera.fieldOfView = fieldOfView;
			Camera.orthographicSize = orthographicSize;
		}

		#endregion


		#if UNITY_EDITOR

		public void ShowGUI ()
		{
			ShowCursorInfluenceGUI ();

			CustomGUILayout.Header ("Follow target");
			CustomGUILayout.BeginVertical ();
				
			targetIsPlayer = CustomGUILayout.Toggle ("Target is Player?", targetIsPlayer, "", "If True, the camera will follow the active Player");
				
			if (!targetIsPlayer)
			{
				target = (Transform) CustomGUILayout.ObjectField <Transform> ("Target:", target, true, "", "The object for the camera to follow");
			}
				
			dampSpeed = CustomGUILayout.Slider ("Follow speed:", dampSpeed, 0.01f, 1f, "", "The follow speed when tracking a target");
			if (!followCursor)
			{
				updateWhilePaused = CustomGUILayout.Toggle ("Update while paused?", updateWhilePaused, "", "If True, the camera will not be frozen while the game is paused");
			}

			CustomGUILayout.EndVertical ();
		}

		#endif


		#region PrivateFunctions

		private CameraData GetInterpolatedCameraData (Vector3 targetPosition)
		{
			CalculateWeights (targetPosition);

			Vector3 position = Vector3.zero;
			float fieldOfView = 0f;
			float orthographicSize = 0f;

			Vector3 forwardSum = Vector3.zero;
            Vector3 upwardSum = Vector3.zero;
			
			for (int i = 0; i < nodes.Length; i++)
			{
				var camera = nodes[i].Camera;
				if (camera == null)
				{
					ACDebug.LogWarning ("Camera node " + nodes[i] + " has no Camera", nodes[i]);
					continue;
				}
				position += camera.transform.position * weights[i];
				fieldOfView += camera.fieldOfView * weights[i];
				orthographicSize += camera.orthographicSize * weights[i];

				forwardSum += weights[i] * (camera.transform.rotation * Vector3.forward);
                upwardSum += weights[i] * (camera.transform.rotation * Vector3.up);
			}

			forwardSum /= (float) nodes.Length;
            upwardSum /= (float) nodes.Length;

			Quaternion rotation = Quaternion.LookRotation (forwardSum, upwardSum);

			return new CameraData (position, rotation, fieldOfView, orthographicSize);
		}


		private void CalculateWeights (Vector3 samplePoint)
		{
			int numPoints = nodes.Length;

			float totalWeight = 0f;
    
			for (int i = 0; i < numPoints; i++)
			{
				// Calc vec i -> sample
				Vector3 point_i = nodes[i].TargetPosition;
				Vector3 vec_is  = samplePoint - point_i;
				
				float weight  = 1f;
				
				for (int j = 0; j < numPoints; j++)
				{
					if (i == j) continue;
					
					// Calc vec i -> j
					Vector3 point_j     = nodes[j].TargetPosition;            
					Vector3 vec_ij      = point_j - point_i;      
					
					// Calc Weight
					float lensq_ij      = Vector3.Dot ( vec_ij, vec_ij );
					float new_weight    = Vector3.Dot (vec_is, vec_ij ) / lensq_ij;
					new_weight          = 1f - new_weight;
					new_weight          = Mathf.Clamp01 (new_weight);
					
					weight              = Mathf.Min (weight, new_weight);
				}
			
				weights[i] = weight;
				totalWeight += weight;
			}
			
			for (int i = 0; i < numPoints; i++)
			{
				weights[i] = weights[i] / totalWeight;
			}
		}

		#endregion


		#region GetSet

		private float LerpSpeed { get { return (1f - Mathf.Pow (1f - Mathf.Clamp01 (dampSpeed), updateWhilePaused ? Time.unscaledDeltaTime : Time.deltaTime)); } }

		#endregion


		#region PrivateClasses

		private readonly struct CameraData
		{

			public readonly Vector3 Position;
			public readonly Quaternion Rotation;
			public readonly float FieldOfView;
			public readonly float OrthographicSize;

			public CameraData (Vector3 position, Quaternion rotation, float fieldOfView, float orthographicSize)
			{
				Position = position;
				Rotation = rotation;
				FieldOfView = fieldOfView;
				OrthographicSize = orthographicSize;
			}

		}

		#endregion

    }

}
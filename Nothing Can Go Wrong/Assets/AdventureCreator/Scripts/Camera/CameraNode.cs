/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2024
 *	
 *	"CameraNode.cs"
 * 
 *	A component attached to each child camera of a GameCameraArray, used as a reference for its interpolated position.
 * 
 */
 
using UnityEngine;
using UnityEditor;

namespace AC
{

	[AddComponentMenu("Adventure Creator/Camera/Camera node")]
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_camera_node.html")]
    public class CameraNode : MonoBehaviour
    {

		#region Variables

		[SerializeField] private Vector3 targetPosition;
		[SerializeField] private Camera _camera = null;

		#endregion


		#region UnityStandards

		private void OnValidate ()
		{
			if (_camera == null) _camera = GetComponent<Camera> ();
		}
		

		private void OnDrawGizmosSelected ()
		{
			Gizmos.color = new Color (1, 0, 0, 0.5f);
      		Gizmos.DrawSphere (TargetPosition, 0.25f);
		}

		#endregion


		#region GetSet
		
		/** The position of the target that this camera represents */
		public Vector3 TargetPosition { get { return targetPosition; } set { targetPosition = value; } }
		/** The camera component */
		public Camera Camera => _camera;

		#endregion
		
    }


#if UNITY_EDITOR

	[CustomEditor (typeof (CameraNode)), CanEditMultipleObjects]
	public class CameraNodeditor : Editor
	{

		protected virtual void OnSceneGUI ()
		{
			CameraNode cameraNode = (CameraNode) target;

			EditorGUI.BeginChangeCheck ();
			Vector3 newTargetPosition = Handles.PositionHandle (cameraNode.TargetPosition, Quaternion.identity);
			
			if (EditorGUI.EndChangeCheck ())
			{
				Undo.RecordObject (cameraNode, "Change Target Position");
				cameraNode.TargetPosition = newTargetPosition;
			}
		}

	}

#endif

}
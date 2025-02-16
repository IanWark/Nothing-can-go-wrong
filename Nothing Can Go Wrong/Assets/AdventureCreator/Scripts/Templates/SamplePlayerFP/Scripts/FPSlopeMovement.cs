using UnityEngine;
using AC;

namespace AC.Templates.FirstPersonPlayer
{

	public class FPSlopeMovement : MonoBehaviour
	{

		#region Variables

		[SerializeField] private AC.Char character = null;
		[SerializeField] private CharacterController characterController = null;
		[SerializeField] private float correctionForce = 5f;
		[SerializeField] private float rayLength = 0.5f;

		#endregion


		#region UnityStandards

		private void Update ()
		{
			if (character.charState == CharState.Move || character.charState == CharState.Decelerate)
			{
				if (IsOnSlope () && !character.IsJumping)
				{
					characterController.Move (Vector3.down * characterController.height / 2f * correctionForce * Time.deltaTime);
				}
			}
		}

		#endregion


		#region PrivateFunctions
		
		private bool IsOnSlope ()
		{
			RaycastHit hit;
			if (Physics.Raycast (character.Transform.TransformPoint (characterController.center), Vector3.down, out hit, (characterController.height / 2f) + rayLength))
			{
				return hit.normal != Vector3.up;
			}
			return false;
		}

		#endregion

	}

}
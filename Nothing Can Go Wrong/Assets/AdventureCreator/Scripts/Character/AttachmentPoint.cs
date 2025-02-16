/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2024
 *	
 *	"AttachmentPoint.cs"
 * 
 *	A data container for an attachment point, i.e. a Transform on the character that can be used to hold objects.
 * 
 */
 
using System;
using UnityEngine;

namespace AC
{

	[Serializable]
	/** A data container for an attachment point, i.e. a Transform on the character that can be used to hold objects. */
	public class AttachmentPoint
	{

		#region Variables

		/** A unique identifier */
		public int ID;
		/** A label to display in the Editor */
		public string label;
		/** The asociated transform */
		public Transform transform;
		[NonSerialized] public GameObject heldObject;
		
		#endregion


		#region Constructors

		public AttachmentPoint () {}


		public AttachmentPoint (int id, string _label, Transform _transform)
		{
			ID = id;
			label = _label;
			transform = _transform;
		}


		public AttachmentPoint (int[] idArray)
		{
			ID = 0;
			transform = null;
			
			foreach (int _id in idArray)
			{
				if (ID == _id)
					ID++;
			}

			label = "AP "+ ID;
		}

		#endregion

	}


	[Serializable]
	/** A container class for save-game data related to an attachment point */
	public class AttachmentPointData
	{

		/** The attachment point's ID number */
		public int attachmentPointID;
		/** The Constant ID number of the held item's RememberSceneItem component, if set */
		public int heldSceneItemConstantID;


		public AttachmentPointData ()
		{
			attachmentPointID = -1;
			heldSceneItemConstantID = 0;
		}


		public AttachmentPointData (AttachmentPoint attachmentPoint)
		{
			attachmentPointID = attachmentPoint.ID;
			heldSceneItemConstantID = 0;

			if (attachmentPoint.heldObject)
			{
				RememberSceneItem rememberSceneItem = attachmentPoint.heldObject.GetComponent<RememberSceneItem> ();
				if (rememberSceneItem)
				{
					heldSceneItemConstantID = rememberSceneItem.constantID;
				}
			}
		}


		public AttachmentPointData (int _attachmentPointID, int _heldSceneItemConstantID)
		{
			attachmentPointID = _attachmentPointID;
			heldSceneItemConstantID = _heldSceneItemConstantID;
		}

	}

}
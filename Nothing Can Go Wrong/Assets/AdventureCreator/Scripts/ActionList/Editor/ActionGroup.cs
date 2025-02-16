#if UNITY_EDITOR

using System;
using UnityEngine;

namespace AC
{

	[Serializable]
	public class ActionGroup
	{

		#region Variables

		/** A unique identifier */
		public int ID = 1;
		public Rect rect;
		public Color color = Color.white;
		public string label;

		#endregion

		
		#region Constructors

		public ActionGroup (int[] idArray)
		{
			ID = 1;
			foreach (int _id in idArray)
			{
				if (ID == _id)
					ID++;
			}
			color = Color.white;
			label = "Group " + ID;
		}

		#endregion

	}

}

#endif
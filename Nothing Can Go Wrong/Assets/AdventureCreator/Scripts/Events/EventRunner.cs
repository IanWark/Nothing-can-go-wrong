#if UNITY_2019_4_OR_NEWER

/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2024
 *	
 *	"EventRunner.cs"
 * 
 *	A component that allows ActionLists to run when events are invoked.
 * 
 */

using System.Collections.Generic;
using UnityEngine;

namespace AC
{

	/** A component that allows ActionLists to run when events are invoked. */
	[AddComponentMenu ("Adventure Creator/Logic/Event Runner")]
	[HelpURL ("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_event_runner.html")]
	public class EventRunner : MonoBehaviour, iActionListAssetReferencer
    {

		#region Variables

		/** A list of ActionLists that run when common events are fired */
		[SerializeReference] private List<EventBase> events = new List<EventBase> ();
		/** Where ActionLists are referenced */
		public ActionListSource source = ActionListSource.AssetFile;

		#endregion


		#region UnityStandards

		private void OnEnable ()
		{
			if (KickStarter.settingsManager)
			{
				foreach (EventBase _event in events)
				{
					_event.Register ();
				}
			}
		}


		private void OnDisable ()
		{
			if (KickStarter.settingsManager)
			{
				foreach (EventBase _event in events)
				{
					_event.Unregister ();
				}
			}
		}

		#endregion


#if UNITY_EDITOR

		private EventsEditorData data;
		private bool showProperties = true;

		public void ShowGUI ()
		{
			showProperties = CustomGUILayout.ToggleHeader (showProperties, "Properties");
			CustomGUILayout.BeginVertical ();
			source = (ActionListSource) CustomGUILayout.EnumPopup ("Actions source:", source);
			CustomGUILayout.EndVertical ();

			if (data == null) data = new EventsEditorData ();
			data.ShowGUI (events, source, this);

		}


		public bool ReferencesAsset (ActionListAsset actionListAsset)
		{
			if (source == ActionListSource.AssetFile)
			{
				foreach (EventBase _event in events)
				{
					if (_event.ActionListAsset == null) continue;
					if (_event.ActionListAsset == actionListAsset)
					{
						return true;
					}
				}
			}
			return false;
		}

		
		public List<ActionListAsset> GetReferencedActionListAssets ()
		{
			List<ActionListAsset> allAssets = new List<ActionListAsset> ();
			if (source == ActionListSource.AssetFile)
			{
				foreach (EventBase _event in events)
				{
					if (_event.ActionListAsset == null) continue;
					allAssets.Add (_event.ActionListAsset);
				}
			}
			return allAssets;
		}

#endif

    }

}

#endif
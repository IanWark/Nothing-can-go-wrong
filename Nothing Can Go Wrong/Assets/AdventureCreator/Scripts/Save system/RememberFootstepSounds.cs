/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2024
 *	
 *	"RememberFootstepSounds.cs"
 * 
 *	This script is attached to FootstepSound components whose change in AudioClips you wish to save. 
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
#if AddressableIsPresent
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets;
#endif

namespace AC
{

	/**
	 * This script is attached to FootstepSound components whose change in AudioClips you wish to save. 
	 */
	[AddComponentMenu("Adventure Creator/Save system/Remember Footstep Sounds")]
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_remember_footstep_sounds.html")]
	public class RememberFootstepSounds : Remember
	{

		private FootstepSounds footstepSounds;
		

		public override string SaveData ()
		{
			FootstepSoundData footstepSoundData = new FootstepSoundData ();

			footstepSoundData.objectID = constantID;
			footstepSoundData.savePrevented = savePrevented;

			if (FootstepSounds)
			{
				footstepSoundData.surfaceID = FootstepSounds.CurrentSurface != null ? FootstepSounds.CurrentSurface.ID : -1;
			}

			return Serializer.SaveScriptData <FootstepSoundData> (footstepSoundData);
		}
		

		public override IEnumerator LoadDataCo (string stringData)
		{
			FootstepSoundData data = Serializer.LoadScriptData <FootstepSoundData> (stringData);
			if (data == null) yield break;
			SavePrevented = data.savePrevented; if (savePrevented) yield break;

			if (FootstepSounds)
			{
				Surface surface = KickStarter.settingsManager.GetSurface (data.surfaceID);
				if (surface != null)
				{
					FootstepSounds.CurrentSurface = surface;
				}
			}
		}


		private FootstepSounds FootstepSounds
		{
			get
			{
				if (footstepSounds == null)
				{
					footstepSounds = GetComponent <FootstepSounds>();
				}
				return footstepSounds;
			}
		}

	}


	/** A data container used by the RememberFootstepSounds script. */
	[System.Serializable]
	public class FootstepSoundData : RememberData
	{

		public string walkSounds;
		public string runSounds;
		public int surfaceID;

		/** The default Constructor. */
		public FootstepSoundData () { }

	}

}
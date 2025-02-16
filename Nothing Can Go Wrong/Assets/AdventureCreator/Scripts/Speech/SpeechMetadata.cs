#if LocalizationIsPresent && AddressableIsPresent

/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2024
 *	
 *	"SpeechMetadata.cs"
 * 
 *	A custom Metadata file for Localization string table entries.  This provides audio and lipsync data fields, and will be read by the "Dialogue: Play speech" Action if it refers to an associated table entry.
 * 
 */
 
using System;
using UnityEngine;
using UnityEngine.Localization.Metadata;
using UnityEngine.AddressableAssets;

namespace AC
{

	/**
	 * A custom Metadata file for Localization string table entries.  This provides audio and lipsync data fields, and will be read by the "Dialogue: Play speech" Action if it refers to an associated table entry.
 	 */
	[Serializable]
	[Metadata (AllowedTypes = MetadataType.StringTableEntry, AllowMultiple = false, MenuItem = "AC Speech")]
	public class SpeechMetadata : IMetadata
	{

		#region Variables

		[SerializeField] private AssetReferenceT<AudioClip> audioClipReference;
		[SerializeField] private AssetReferenceT<TextAsset> lipSyncDataReference;

		#endregion


		#region GetSet

		/** A reference to the audio clip  */
		public AssetReferenceT<AudioClip> AudioClipReference => audioClipReference;
		/** A reference to the lipsync data TextAsset */
		public AssetReferenceT<TextAsset> LipSyncDataReference => lipSyncDataReference;

		#endregion

	}

}

#endif
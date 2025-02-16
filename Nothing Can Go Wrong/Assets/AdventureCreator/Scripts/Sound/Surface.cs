using System;
using System.Collections.Generic;
using UnityEngine;

namespace AC
{

	[Serializable]
	public class Surface
	{

		#region Variables

		public int ID;
		public string label;
		public AudioClip[] walkSounds = new AudioClip[0];
		public AudioClip[] runSounds = new AudioClip[0];

		#endregion


		#region Constructors

		public Surface (int id)
		{
			ID = id;
			walkSounds = new AudioClip[0];
			runSounds = new AudioClip[0];
			label = "Surface"+ ID;
		}


		public Surface (int[] idArray)
		{
			ID = 0;
			walkSounds = new AudioClip[0];
			runSounds = new AudioClip[0];
			
			foreach (int _id in idArray)
			{
				if (ID == _id)
					ID++;
			}

			label = "Surface"+ ID;
		}

		#endregion


#if UNITY_EDITOR
		
		public void ShowGUI ()
		{
			label = CustomGUILayout.TextField ("Label:", label);
			walkSounds = ShowClipsGUI (walkSounds, "Walk sounds");
			runSounds = ShowClipsGUI (runSounds, "Run sounds (optional)");
		}


		private AudioClip[] ShowClipsGUI (AudioClip[] clips, string headerLabel)
		{
			CustomGUILayout.BeginVertical ();
			UnityEditor.EditorGUILayout.LabelField (headerLabel, UnityEditor.EditorStyles.boldLabel);
			List<AudioClip> clipsList = new List<AudioClip>();

			if (clips != null)
			{
				foreach (AudioClip clip in clips)
				{
					clipsList.Add (clip);
				}
			}

			int numParameters = clipsList.Count;
			numParameters = UnityEditor.EditorGUILayout.DelayedIntField ("# of footstep sounds:", numParameters);

			if (numParameters < clipsList.Count)
			{
				clipsList.RemoveRange (numParameters, clipsList.Count - numParameters);
			}
			else if (numParameters > clipsList.Count)
			{
				if (numParameters > clipsList.Capacity)
				{
					clipsList.Capacity = numParameters;
				}
				for (int i=clipsList.Count; i<numParameters; i++)
				{
					clipsList.Add (null);
				}
			}

			for (int i=0; i<clipsList.Count; i++)
			{
				clipsList[i] = (AudioClip) CustomGUILayout.ObjectField <AudioClip> ("Sound #" + (i+1).ToString (), clipsList[i], false, "", headerLabel);
			}
			CustomGUILayout.EndVertical ();

			return clipsList.ToArray ();
		}

#endif

	}

}
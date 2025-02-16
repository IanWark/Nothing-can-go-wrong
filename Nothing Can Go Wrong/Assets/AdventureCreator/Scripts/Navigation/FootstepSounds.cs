/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2024
 *	
 *	"FootstepSounds.cs"
 * 
 *	A component that can play footstep sounds whenever a Mecanim-animated Character moves.
 * The component stores an array of AudioClips, one of which is played at random whenever the PlayFootstep method is called.
 * This method should be invoked as part of a Unity AnimationEvent: http://docs.unity3d.com/Manual/animeditor-AnimationEvents.html
 * 
 */

using UnityEngine;

namespace AC
{

	/**
	 * A component that can play footstep sounds whenever a Mecanim-animated Character moves.
	 * The component stores an array of AudioClips, one of which is played at random whenever the PlayFootstep method is called.
	 * This method should be invoked as part of a Unity AnimationEvent: http://docs.unity3d.com/Manual/animeditor-AnimationEvents.html
	 */
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_footstep_sounds.html")]
	[AddComponentMenu("Adventure Creator/Characters/Footstep sounds")]
	public class FootstepSounds: MonoBehaviour
	{

		#region Variables

		[SerializeField] private AudioClip[] footstepSounds;
		[SerializeField] private AudioClip[] runSounds;
		/** The Sound object to play from */
		public Sound soundToPlayFrom;
		/** How the sounds are played */
		public FootstepPlayMethod footstepPlayMethod = FootstepPlayMethod.ViaAnimationEvents;
		public enum FootstepPlayMethod { Automatically, ViaAnimationEvents };
		/** The Player or NPC that this component is for */
		public Char character;
		/** If True, and character is assigned, sounds will only play when the character is grounded */
		public bool doGroundedCheck = false;
		/** If True, and character is assigned, sounds will only play when the character is moving */
		public bool doMovementCheck = true;
		/** If True, then raycasting will be used to auto-detect the current Surface */
		public bool autoDetectSurface = true;

		public float raycastLength = 0.5f;

		public LayerMask layerMask = new LayerMask ();

		/** How much the audio pitch can randomly vary by */
		public float pitchVariance = 0f;
		/** How much the audio volume can randomly vary by */
		public float volumeVariance = 0f;

		/** The separation time between sounds when walking */
		public float walkSeparationTime = 0.5f;
		/** The separation time between sounds when running */
		public float runSeparationTime = 0.25f;

		/** The current surface to get sounds from */
		public Surface CurrentSurface { get; set; }

		protected float originalRelativeSound = 1f;
		protected int lastIndex;
		protected AudioSource audioSource;
		protected float delayTime;

		#endregion


		#region UnityStandards

		protected void OnValidate ()
		{
			if (character == null)
			{
				character = GetComponent<AC.Char> ();
			}
		}

		
		protected void Awake ()
		{
			if (soundToPlayFrom)
			{
				audioSource = soundToPlayFrom.GetComponent <AudioSource>();
			}
			if (audioSource == null)
			{
				audioSource = GetComponent <AudioSource>();
			}

			if (character == null)
			{
				character = GetComponent <Char>();
			}
			delayTime = walkSeparationTime / 2f;

			if (CurrentSurface == null && KickStarter.settingsManager != null && KickStarter.settingsManager.surfaces != null && KickStarter.settingsManager.surfaces.Count > 0)
			{
				CurrentSurface = KickStarter.settingsManager.surfaces[0];
			}

			RecordOriginalRelativeSound ();
		}


		protected void Update ()
		{
			if (character == null || footstepPlayMethod == FootstepPlayMethod.ViaAnimationEvents) return;

			if (character.charState == CharState.Move && !character.IsJumping)
			{
				delayTime -= Time.deltaTime;

				if (delayTime <= 0f)
				{
					delayTime = (character.isRunning) ? runSeparationTime : walkSeparationTime;
					PlayFootstep ();
				}
			}
			else
			{
				delayTime = walkSeparationTime / 2f;
			}
		}

		#endregion


		#region PublicFunctions		

		/** Plays one of the footstepSounds at random on the assigned Sound object. */
		public void PlayFootstep ()
		{
			if (enabled && audioSource &&
			    (!doMovementCheck || character == null || character.charState == CharState.Move))
			{
				if (doGroundedCheck && character && !character.IsGrounded (true))
				{
					return;
				}

				RequestSurface ();

				if (CurrentSurface == null)
				{
					return;
				}

				bool doRun = (character.isRunning && CurrentSurface.runSounds.Length > 0) ? true : false;
				if (doRun)
				{
					PlaySound (CurrentSurface.runSounds, doRun);
				}
				else
				{
					PlaySound (CurrentSurface.walkSounds, doRun);
				}
			}
		}
		

		/** Records the associated Sound component's relative volume. */
		public void RecordOriginalRelativeSound ()
		{
			if (soundToPlayFrom)
			{
				originalRelativeSound = soundToPlayFrom.relativeVolume;
			}
		}

		#endregion


		#region ProtectedFunctions

		protected void RequestSurface ()
		{
			if (autoDetectSurface)
			{
				Vector3 origin = character ? character.transform.position : transform.position;

				if (SceneSettings.IsUnity2D ())
				{
					RaycastHit2D hit = UnityVersionHandler.Perform2DRaycast (origin + (raycastLength * 0.5f * Vector3.up), Vector3.down, raycastLength * 1.5f, layerMask);
					if (hit.collider)
					{
						ProcessCollider (hit.collider);
					}
				}
				else
				{
					Vector3 up = character ? character.UpDirection : Vector3.up;
					RaycastHit hit;
					if (Physics.Raycast (origin + (raycastLength * 0.5f * up), -up, out hit, raycastLength * 1.5f, layerMask))
					{
						ProcessCollider (hit.collider);
					}
				}
			}

			if (KickStarter.eventManager)
			{
				KickStarter.eventManager.Call_OnRequestFootstepSounds (this);
			}
		}


		protected void ProcessCollider (Collider collider)
		{
			if (collider.sharedMaterial == null) return;
			ProcessName (collider.sharedMaterial.name);
		}


		protected void ProcessCollider (Collider2D collider)
		{
			if (collider.sharedMaterial == null) return;
			ProcessName (collider.sharedMaterial.name);
		}


		protected void ProcessName (string name)
		{
			string[] nameArray = name.Split ("_"[0]);
			if (nameArray.Length > 0)
			{
				string label = nameArray[nameArray.Length - 1];
				Surface surface = KickStarter.settingsManager.GetSurface (label);
				if (surface != null)
				{
					CurrentSurface = surface;
				}
			}
		}


		protected void PlaySound (AudioClip[] clips, bool isRunSound)
		{
			if (clips == null || clips.Length == 0) return;

			if (clips.Length == 1)
			{
				PlaySound (clips[0], isRunSound);
				return;
			}

			int newIndex = Random.Range (0, clips.Length - 1);
			if (newIndex == lastIndex)
			{
				newIndex ++;
				if (newIndex >= clips.Length)
				{
					newIndex = 0;
				}
			}

			PlaySound (clips[newIndex], isRunSound);
			lastIndex = newIndex;
		}


		protected void PlaySound (AudioClip clip, bool isRunSound)
		{
			if (clip == null) return;

			audioSource.clip = clip;

			if (pitchVariance > 0f)
			{
				float randomPitch = 1f + Random.Range (-pitchVariance, pitchVariance);
				audioSource.pitch = randomPitch;
			}

			float localVolume = (volumeVariance > 0f) ? (1f - Random.Range (0f, volumeVariance)): 1f;
			
			if (soundToPlayFrom)
			{
				if (soundToPlayFrom.audioSource)
				{
					soundToPlayFrom.audioSource.PlayOneShot (clip, localVolume);
				}
				if (KickStarter.eventManager) KickStarter.eventManager.Call_OnPlayFootstepSound (character, this, !isRunSound, soundToPlayFrom.audioSource, clip);
			}
			else
			{
				audioSource.PlayOneShot (clip, localVolume);
				if (KickStarter.eventManager) KickStarter.eventManager.Call_OnPlayFootstepSound (character, this, !isRunSound, audioSource, clip);
			}
		}

		#endregion

	}

}
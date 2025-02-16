/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2024
 *	
 *	"InteractiveBoundary.cs"
 * 
 *	This script is used to limit Hotspot interactivity to players that are within a given volume.
 * 
 */

using UnityEngine;
using System.Collections.Generic;

namespace AC
{

	/**
	 * Used to limit a Hotspot's interactivity to Players that are within a given volume.
	 * Attach this to a Trigger collider, and assign in a Hotspot's Inspector. When assigned, the Hotspot will only be interactable when the Player is within the collider's boundary.
	 */
	[AddComponentMenu("Adventure Creator/Hotspots/Interactive boundary")]
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_interactive_boundary.html")]
	public class InteractiveBoundary : MonoBehaviour
	{

		#region Variables

		protected bool forcePresence;
		protected List<GameObject> playersPresent = new List<GameObject>();
		private Collider _collider;
		private Collider2D _collider2D;

		/** The way in which objects are detected (RigidbodyCollision, TransformPosition) */
		public TriggerDetectionMethod detectionMethod = TriggerDetectionMethod.RigidbodyCollision;
		
		#endregion


		#region UnityStandards

		private void Awake ()
		{
			_collider = GetComponent<Collider> ();
			_collider2D = GetComponent<Collider2D> ();
		}


		protected void Update ()
		{
			if (detectionMethod == TriggerDetectionMethod.RigidbodyCollision) return;
			if (_collider == null && _collider2D == null) return;

			Bounds bounds = _collider ? _collider.bounds : _collider2D.bounds;

			foreach (Player player in KickStarter.stateHandler.Players)
			{
				if (bounds.Contains (player.Transform.position))
				{
					if (!playersPresent.Contains (player.gameObject))
					{
						playersPresent.Add (player.gameObject);
					}
				}
				else
				{
					if (playersPresent.Contains (player.gameObject))
					{
						playersPresent.Remove (player.gameObject);
					}
				}
			}
		}


		protected void OnTriggerEnter (Collider other)
		{
			if (detectionMethod == TriggerDetectionMethod.TransformPosition) return;
			if (KickStarter.player && other.gameObject == KickStarter.player.gameObject && !playersPresent.Contains (other.gameObject))
			{
				playersPresent.Add (other.gameObject);
			}
        }


		protected void OnTriggerExit (Collider other)
		{
			if (detectionMethod == TriggerDetectionMethod.TransformPosition) return;
			if (KickStarter.player && other.gameObject == KickStarter.player.gameObject && playersPresent.Contains (other.gameObject))
			{
				playersPresent.Remove (other.gameObject);
			}
		}


		protected void OnTriggerEnter2D (Collider2D other)
		{
			if (detectionMethod == TriggerDetectionMethod.TransformPosition) return;
			if (KickStarter.player && other.gameObject == KickStarter.player.gameObject && !playersPresent.Contains (other.gameObject))
			{
				playersPresent.Add (other.gameObject);
			}
		}


		protected void OnTriggerExit2D (Collider2D other)
		{
			if (detectionMethod == TriggerDetectionMethod.TransformPosition) return;
			if (KickStarter.player && other.gameObject == KickStarter.player.gameObject && playersPresent.Contains (other.gameObject))
			{
				playersPresent.Remove (other.gameObject);
			}
		}

		#endregion


		#region GetSet

		/** True if the active Player is within the Collider boundary */
		public bool PlayerIsPresent
		{
			get
			{
				if (forcePresence)
				{
					return true;
				}
				return (KickStarter.player && playersPresent.Contains (KickStarter.player.gameObject));
			}
		}


		/** If True, the Player will always be considered as present within the Collider boundary, even when not physically so */
		public bool ForcePresence
		{
			set
			{
				forcePresence = value;
			}
		}

		#endregion

	}

}
/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2024
 *	
 *	"LightSwitch.cs"
 * 
 *	This can be used, via the Object: Send Message Action,
 *	to turn its attached light component on and off.
 * 
 */

using UnityEngine;
#if URPIsPresent
using UnityEngine.Rendering.Universal;
#endif

namespace AC
{

	/**
	 * This script provides functions to enable and disable the Light component on the GameObject it is attached to.
	 * These functions can be called either through script, or with the "Object: Send message" Action.
	 */
	#if !URPIsPresent
	[RequireComponent (typeof (Light))]
	#endif
	[AddComponentMenu("Adventure Creator/Misc/Light switch")]
	[HelpURL("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_light_switch.html")]
	public class LightSwitch : MonoBehaviour
	{

		#region Variables

		/** If True, then the Light component will be enabled when the game begins. */
		public bool enableOnStart = false;

		protected Light _light;
		#if URPIsPresent
		protected Light2D light2D;
		#endif

		#endregion


		#region UnityStandards
		
		protected void Awake ()
		{
			Switch (enableOnStart);
		}

		#endregion


		#region PublicFunctions		

		/** Enables the Light component on the GameObject this script is attached to. */
		public void TurnOn ()
		{
			Switch (true);
		}
		

		/** Disables the Light component on the GameObject this script is attached to. */
		public void TurnOff ()
		{
			Switch (false);
		}

		#endregion


		#region ProtectedFunctions

		protected void Switch (bool turnOn)
		{
			if (_light == null)
			{
				_light = GetComponent <Light>();
			}
			if (_light)
			{
				_light.enabled = turnOn;
			}
			#if URPIsPresent
			if (light2D == null)
			{
				light2D = GetComponent <Light2D>();
			}
			if (light2D)
			{
				light2D.enabled = turnOn;
			}
			#endif
		}

		#endregion
		
	}

}
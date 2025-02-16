/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2024
 *	
 *	"AutoCorrectUIDimensions.cs"
 * 
 *	This script can re-position and re-scale a Unity UI-based Menu if the playable area is not the same as the game screen. This can be the case if an aspect ratio is enforced, or if running on a mobile device with notched features.
 * 
 */

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace AC
{

	/** This script can re-position and re-scale a Unity UI-based Menu if the playable area is not the same as the game screen. This can be the case if an aspect ratio is enforced, or if running on a mobile device with notched features. */
	[AddComponentMenu ("Adventure Creator/UI/Auto-correct UI Dimensions")]
	[HelpURL ("https://www.adventurecreator.org/scripting-guide/class_a_c_1_1_auto_correct_ui_dimensions.html")]
	public class AutoCorrectUIDimensions : MonoBehaviour
	{

		#region Variables

		protected CanvasScaler canvasScaler;

		/** What RectTransform to reposition. If unset, and the Canvas is linked to an AC Menu, then this will be auto-set to the Menu's RectTransform boundary */
		public RectTransform transformToControl = null;

		public Vector2 minAnchorPoint = new Vector2 (0.5f, 0.5f);
		public Vector2 maxAnchorPoint = new Vector2 (0.5f, 0.5f);

		public bool updatePosition = true;
		public bool updateScale = true;
		public bool accountForSafeArea = false;

		protected Vector2 originalReferenceResolution;

		#endregion


		#region UnityStandards

		protected void Start ()
		{
			Initialise ();
			OnUpdatePlayableScreenArea ();
		}


		protected void OnEnable ()
		{
			Initialise ();
			EventManager.OnUpdatePlayableScreenArea += OnUpdatePlayableScreenArea;
			StartCoroutine (UpdateInOneFrame ());
		}


		protected void OnDisable ()
		{
			EventManager.OnUpdatePlayableScreenArea -= OnUpdatePlayableScreenArea;
		}

		#endregion


		#region ProtectedFunctions

		protected void Initialise ()
		{
			canvasScaler = GetComponent<CanvasScaler> ();
			if (canvasScaler)
			{
				originalReferenceResolution = canvasScaler.referenceResolution;
			}
			
			if (updateScale && canvasScaler == null)
			{
				ACDebug.LogWarning ("The Auto Correct UI Dimensions component on " + gameObject.name + " must be attached to a GameObject with a CanvasScaler component - be sure to attach it to the root Canvas object.", this);
			}
		}


		protected IEnumerator UpdateInOneFrame ()
		{
			yield return new WaitForEndOfFrame ();
			OnUpdatePlayableScreenArea ();
		}


		protected void OnUpdatePlayableScreenArea ()
		{
			if (transformToControl == null && KickStarter.playerMenus)
			{
				Canvas canvas = GetComponent<Canvas> ();
				if (canvas)
				{
					Menu menu = KickStarter.playerMenus.GetMenuWithCanvas (canvas);
					if (menu != null)
					{
						transformToControl = menu.rectTransform;
					}
				}
			}

			if (updatePosition && transformToControl == null)
			{
				ACDebug.LogWarning ("Cannot find which Transform to reposition with the AutoCorrectUIDimensions component - either assign the Transform To Control field, or link the Canvas to a Menu with a RectTransform boundary.", this);
			}

			// Position
			if (updatePosition && transformToControl)
			{
				transformToControl.anchorMin = ConvertToPlayableSpace (minAnchorPoint);
				transformToControl.anchorMax = ConvertToPlayableSpace (maxAnchorPoint);
			}

			// Scale
			if (updateScale && canvasScaler)
			{
				Vector2 safeSize = KickStarter.mainCamera.GetPlayableScreenArea (true).size;
				canvasScaler.referenceResolution = new Vector2 (originalReferenceResolution.x / safeSize.x, originalReferenceResolution.y / safeSize.y);
			}
			
			Canvas.ForceUpdateCanvases ();
		}


		protected Vector2 ConvertToPlayableSpace (Vector2 screenPosition)
		{
			Rect playableScreenArea = KickStarter.mainCamera.GetPlayableScreenArea (true);
			
			if (accountForSafeArea)
			{
				Rect safeArea = Screen.safeArea;
				
				var safePlayableScreenArea = new Rect
				(
					safeArea.x / ACScreen.width,
					safeArea.y / ACScreen.height,
					safeArea.width / ACScreen.width,
					safeArea.height / ACScreen.height
				);
				
				playableScreenArea = Rect.MinMaxRect
				(
					Mathf.Max (playableScreenArea.x, safePlayableScreenArea.x),
					Mathf.Max (playableScreenArea.y, safePlayableScreenArea.y),
					Mathf.Min (playableScreenArea.xMax, safePlayableScreenArea.xMax),
					Mathf.Min (playableScreenArea.yMax, safePlayableScreenArea.yMax)
				);
			}
			
			return new Vector2 (screenPosition.x * playableScreenArea.width, screenPosition.y * playableScreenArea.height) + playableScreenArea.position;
		}

		#endregion

	}

}
#if UNITY_STANDALONE && !UNITY_2018_2_OR_NEWER
#define ALLOW_MOVIETEXTURES
#endif

/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2024
 *	
 *	"MenuGraphic.cs"
 * 
 *	This MenuElement provides a space for
 *	animated and static textures
 * 
 */

using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	/** A MenuElement that provides a space for animated or still images. */
	public class MenuGraphic : MenuElement
	{

		/** The Unity UI Image this is linked to (Unity UI Menus only) */
		public Image uiImage;
		/** The type of graphic that is shown (Normal, DialoguePortrait, DocumentTexture, ObjectiveTexture, PageTexture) */
		public AC_GraphicType graphicType = AC_GraphicType.Normal;
		/** The CursorIconBase that stores the graphic and animation data */
		public CursorIconBase graphic;

		public RawImage uiRawImage;
		[SerializeField] private UIImageType uiImageType = UIImageType.Image;
		private enum UIImageType { Image, RawImage };
		/** The name of the MenuJournal element to refer to, if graphicType = GraphicType.PageTexture */
		public string linkedJournalElementName;
		/** The change to make to the associated UI object when invisible */
		public UIComponentHideStyle uiComponentHideStyle = UIComponentHideStyle.DisableObject;

		private Texture localTexture;
		private AC.Char portraitCharacterOverride;

		private Rect speechRect = new Rect ();
		private Sprite sprite;
		private Speech speech;
		private CursorIconBase portrait;
		private bool isDuppingSpeech;
		private MenuJournal linkedJournal;


		public override void Declare ()
		{
			uiImage = null;
			uiRawImage = null;
			
			graphicType = AC_GraphicType.Normal;
			isVisible = true;
			isClickable = false;
			graphic = new CursorIconBase ();
			numSlots = 1;
			linkedJournalElementName = string.Empty;
			uiComponentHideStyle = UIComponentHideStyle.DisableObject;
			SetSize (new Vector2 (10f, 5f));
			
			base.Declare ();
		}
		

		public override MenuElement DuplicateSelf (bool fromEditor, bool ignoreUnityUI)
		{
			MenuGraphic newElement = CreateInstance <MenuGraphic>();
			newElement.Declare ();
			newElement.CopyGraphic (this, ignoreUnityUI);
			return newElement;
		}
		
		
		private void CopyGraphic (MenuGraphic _element, bool ignoreUnityUI)
		{
			if (ignoreUnityUI)
			{
				uiImage = null;
				uiRawImage = null;
			}
			else
			{
				uiImage = _element.uiImage;
				uiRawImage = _element.uiRawImage;
			}
			uiImageType = _element.uiImageType;
			linkedJournalElementName = _element.linkedJournalElementName;
			uiComponentHideStyle = _element.uiComponentHideStyle;

			graphicType = _element.graphicType;
			graphic = new CursorIconBase ();
			graphic.Copy (_element.graphic);
			base.Copy (_element);
		}
		

		public override void LoadUnityUI (AC.Menu _menu, Canvas canvas, bool addEventListeners = true)
		{
			if (uiImageType == UIImageType.Image)
			{
				LinkUIElement (canvas, ref uiImage);
				uiRawImage = null;
			}
			else if (uiImageType == UIImageType.RawImage)
			{
				LinkUIElement (canvas, ref uiRawImage);
				uiImage = null;
			}
		}
		

		public override RectTransform GetRectTransform (int _slot)
		{
			if (uiImageType == UIImageType.Image && uiImage)
			{
				return uiImage.rectTransform;
			}
			else if (uiImageType == UIImageType.RawImage && uiRawImage)
			{
				return uiRawImage.rectTransform;
			}
			return null;
		}
		
		
		#if UNITY_EDITOR
		
		public override void ShowGUI (Menu menu, System.Action<ActionListAsset> showALAEditor)
		{
			string apiPrefix = "(AC.PlayerMenus.GetElementWithName (\"" + menu.title + "\", \"" + title + "\") as AC.MenuGraphic)";

			MenuSource source = menu.menuSource;
			CustomGUILayout.BeginVertical ();
			
			if (source != MenuSource.AdventureCreator)
			{
				uiImageType = (UIImageType) EditorGUILayout.EnumPopup (new GUIContent ("UI image type:", "The type of UI component to link to"), uiImageType);
				if (uiImageType == UIImageType.Image)
				{
					uiImage = LinkedUiGUI <Image> (uiImage, "Linked Image:", menu);
				}
				else if (uiImageType == UIImageType.RawImage)
				{
					uiRawImage = LinkedUiGUI <RawImage> (uiRawImage, "Linked Raw Image:", menu);
				}
				CustomGUILayout.EndVertical ();
				CustomGUILayout.BeginVertical ();
			}
			
			graphicType = (AC_GraphicType) CustomGUILayout.EnumPopup ("Graphic type:", graphicType, apiPrefix + ".graphicType", "The type of graphic that is shown");
			if (graphicType == AC_GraphicType.Normal)
			{
				graphic.ShowGUI (false, false, "Texture:", CursorRendering.Software, apiPrefix + ".graphic", "The texture to display");
			}
			else
			{
				graphic.ShowGUI (false, false, "Fallback texture:", CursorRendering.Software, apiPrefix + ".graphic", "The texture to display if none other is available");
			}

			if (graphicType == AC_GraphicType.PageTexture)
			{
				linkedJournalElementName = CustomGUILayout.TextField ("Journal element name:", linkedJournalElementName, apiPrefix + ".linkedJournalElementName", "The name of the Journal element (in the same Menu) to refer to");
			}

			if (menu.menuSource != MenuSource.AdventureCreator)
			{
				uiComponentHideStyle = (UIComponentHideStyle) CustomGUILayout.EnumPopup ("When invisible:", uiComponentHideStyle, apiPrefix + ".uiComponentHideStyle", "The method by which this element (or slots within it) are hidden from view when made invisible");
			}

			CustomGUILayout.EndVertical ();
			
			base.ShowGUI (menu, showALAEditor);
		}

		#endif

		
		public override bool ReferencesObjectOrID (GameObject gameObject, int id)
		{
			if (uiImageType == UIImageType.Image && uiImage && uiImage.gameObject == gameObject) return true;
			if (uiImageType == UIImageType.RawImage && uiRawImage && uiRawImage.gameObject == gameObject) return true;
			if (linkedUiID == id && id != 0) return true;
			return false;
		}


		public override int GetSlotIndex (GameObject gameObject)
		{
			if (uiImageType == UIImageType.Image && uiImage && uiImage.gameObject == gameObject)
			{
				return 0;
			}
			if (uiImageType == UIImageType.RawImage && uiRawImage && uiRawImage.gameObject == gameObject)
			{
				return 0;
			}
			return base.GetSlotIndex (gameObject);
		}


		/**
		 * <summary>Updates the element's texture, provided that its graphicType = AC_GraphicType.Normal</summary>
		 * <param name = "newTexture">The new texture to assign the element</param>
		 */
		public void SetNormalGraphicTexture (Texture newTexture)
		{
			graphic.texture = newTexture;
			graphic.ClearCache ();
		}


		private void UpdateSpeechLink ()
		{
			if (!isDuppingSpeech && parentMenu)
			{
				Speech _speech = KickStarter.dialog.GetLatestSpeech (parentMenu);
				if (_speech != null)
				{
					speech = _speech;
				}
			}
		}


		public override void SetSpeech (Speech _speech)
		{
			isDuppingSpeech = true;
			speech = _speech;
		}
		

		public override void ClearSpeech ()
		{
			if (graphicType == AC_GraphicType.DialoguePortrait)
			{
				localTexture = null;
			}
		}


		public override void OnMenuTurnOn (Menu menu)
		{
			base.OnMenuTurnOn (menu);

			graphic.Reset ();

			PreDisplay (0, Options.GetLanguage (), false);

			#if ALLOW_MOVIETEXTURE
			if (localTexture != null)
			{
				MovieTexture movieTexture = localTexture as MovieTexture;
				if (movieTexture != null)
				{
					movieTexture.Play ();
				}
			}
			#endif
		}


		public override void PreDisplay (int _slot, int languageNumber, bool isActive)
		{
			switch (graphicType)
			{
				case AC_GraphicType.DialoguePortrait:
					if (portraitCharacterOverride)
					{
						portrait = portraitCharacterOverride.GetPortrait ();
						localTexture = portrait.texture;
					}
					else
					{
						UpdateSpeechLink ();
						if (speech != null && speech.GetSpeakingCharacter ())
						{
							portrait = speech.GetSpeakingCharacter ().GetPortrait ();
							localTexture = portrait.texture;
						}
					}
					break;

				case AC_GraphicType.DocumentTexture:
					if (Application.isPlaying && DocumentInstance.IsValid (KickStarter.runtimeDocuments.ActiveDocumentInstance))
					{
						Texture2D newTexture = KickStarter.runtimeDocuments.ActiveDocumentInstance.Document.texture;
						if (localTexture != newTexture)
						{
							if (newTexture)
							{
								sprite = Sprite.Create (newTexture, new Rect (0f, 0f, newTexture.width, newTexture.height), new Vector2 (0.5f, 0.5f));
							}
							else
							{
								sprite = null;
							}
						}
						localTexture = newTexture;
					}
					break;

				case AC_GraphicType.ObjectiveTexture:
					if (Application.isPlaying && KickStarter.runtimeObjectives.SelectedObjective != null)
					{
						if (localTexture != KickStarter.runtimeObjectives.SelectedObjective.Texture && KickStarter.runtimeObjectives.SelectedObjective.Texture)
						{
							Texture2D objTex = KickStarter.runtimeObjectives.SelectedObjective.Texture;
							sprite = Sprite.Create (objTex, new Rect (0f, 0f, objTex.width, objTex.height), new Vector2 (0.5f, 0.5f));
						}
						localTexture = KickStarter.runtimeObjectives.SelectedObjective.Texture;
					}
					break;

				case AC_GraphicType.PageTexture:
					if (Application.isPlaying)
					{
						if (linkedJournal == null)
						{
							if (parentMenu && !string.IsNullOrEmpty (linkedJournalElementName))
							{
								MenuElement linkedElement = parentMenu.GetElementWithName (linkedJournalElementName);
								if (linkedElement)
								{
									linkedJournal = (MenuJournal) linkedElement;
								}
								if (linkedJournal == null) ACDebug.LogWarning ("Graphic element " + title + " cannot find the linked Journal element " + linkedJournalElementName);
							}
						}
						if (linkedJournal)
						{
							Texture2D pageTexture = null;

							if (linkedJournal.journalType == JournalType.DisplayActiveDocument)
							{
								if (DocumentInstance.IsValid (KickStarter.runtimeDocuments.ActiveDocumentInstance))
								{
									int pageNumber = linkedJournal.GetCurrentPageNumber () - 1;
									pageTexture = KickStarter.runtimeDocuments.ActiveDocumentInstance.GetPageTexture (pageNumber);
								}
							}
							else
							{
								JournalPage page = linkedJournal.GetCurrentPage ();
								if (page != null)
								{
									pageTexture = page.texture;
								}
							}

							if (localTexture != pageTexture)
							{
								if (pageTexture)
								{
									sprite = Sprite.Create (pageTexture, new Rect (0f, 0f, pageTexture.width, pageTexture.height), new Vector2 (0.5f, 0.5f));
								}
								else
								{
									sprite = null;
								}
							}
							localTexture = pageTexture;
						}
					}
					break;

				default:
					break;
			}

			SetUIGraphic ();
		}


		public override void Display (GUIStyle _style, int _slot, float zoom, bool isActive)
		{
			base.Display (_style, _slot, zoom, isActive);

			switch (graphicType)
			{
				case AC_GraphicType.Normal:
					if (graphic != null)
					{
						graphic.DrawAsInteraction (ZoomRect (relativeRect, zoom), true);
					}
					break;

				case AC_GraphicType.DialoguePortrait:
					if (localTexture)
					{
						if (portrait.isAnimated)
						{
							Char character = portraitCharacterOverride;
							if (character == null && speech != null && speech.speaker)
							{
								character = speech.speaker;
							}

							if (character)
							{
								if (character.isLipSyncing)
								{
									speechRect = portrait.GetAnimatedRect (character.GetLipSyncFrame ());
								}
								else if (character.isTalking)
								{
									speechRect = portrait.GetAnimatedRect ();
								}
								else
								{
									speechRect = portrait.GetAnimatedRect (0);
								}

								GUI.DrawTextureWithTexCoords (ZoomRect (relativeRect, zoom), localTexture, speechRect);
							}
						}
						else
						{
							GUI.DrawTexture (ZoomRect (relativeRect, zoom), localTexture, ScaleMode.StretchToFill, true, 0f);
						}
					}
					else if (graphic != null && graphic.texture)
					{
						graphic.DrawAsInteraction (ZoomRect (relativeRect, zoom), true);
					}
					break;

				case AC_GraphicType.DocumentTexture:
				case AC_GraphicType.ObjectiveTexture:
				case AC_GraphicType.PageTexture:
					if (localTexture)
					{
						GUI.DrawTexture (ZoomRect (relativeRect, zoom), localTexture, ScaleMode.StretchToFill, true, 0f);
					}
					else if (graphic != null && graphic.texture)
					{
						graphic.DrawAsInteraction (ZoomRect (relativeRect, zoom), true);
					}
					break;
			}
		}
	

		public override void RecalculateSize (MenuSource source)
		{
			graphic.Reset (false);
			SetUIGraphic ();
			base.RecalculateSize (source);
		}


		private void SetUIGraphic ()
		{
			if (!Application.isPlaying) return;

			if (uiImageType == UIImageType.Image && uiImage)
			{
				Sprite _sprite = null;
				switch (graphicType)
				{
					case AC_GraphicType.Normal:
						_sprite = graphic.GetAnimatedSprite (true);
						break;

					case AC_GraphicType.DialoguePortrait:
						if (speech != null && portraitCharacterOverride == null)
						{
							_sprite = speech.GetPortraitSprite ();
						}
						else if (portraitCharacterOverride != null)
						{
							_sprite = portraitCharacterOverride.GetPortraitSprite ();
						}
						if (_sprite == null && graphic != null && graphic.texture)
						{
							_sprite = graphic.GetAnimatedSprite (true);
						}
						break;

					case AC_GraphicType.DocumentTexture:
					case AC_GraphicType.ObjectiveTexture:
					case AC_GraphicType.PageTexture:
						_sprite = sprite;
						if (_sprite == null && graphic != null && graphic.texture)
						{
							_sprite = graphic.GetAnimatedSprite (true);
						}
						break;

					default:
						break;

				}
				if (_sprite) uiImage.sprite = _sprite;
				UpdateUIElement (uiImage, uiComponentHideStyle);
			}
			if (uiImageType == UIImageType.RawImage && uiRawImage)
			{
				Texture texture = null;
				switch (graphicType)
				{
					case AC_GraphicType.Normal:
						if (graphic.texture && graphic.texture is RenderTexture)
						{
							texture = graphic.texture;
						}
						else
						{
							texture = graphic.GetAnimatedTexture (true);
						}
						break;

					case AC_GraphicType.DocumentTexture:
					case AC_GraphicType.ObjectiveTexture:
					case AC_GraphicType.DialoguePortrait:
					case AC_GraphicType.PageTexture:
						texture = localTexture;
						if (localTexture == null && graphic.texture)
						{
							if (texture && graphic.texture is RenderTexture)
							{
								texture = graphic.texture;
							}
							else
							{
								texture = graphic.GetAnimatedTexture (true);
							}
						}
						break;

					default:
						break;
				}

				if (texture) uiRawImage.texture = texture;

				UpdateUIElement (uiRawImage, uiComponentHideStyle);
			}
		}
		
		
		protected override void AutoSize ()
		{
			if (graphic.texture && (graphicType == AC_GraphicType.Normal || !Application.isPlaying))
			{
				GUIContent content = new GUIContent (graphic.texture);
				AutoSize (content);
			}
		}


		public AC.Char PortraitCharacterOverride
		{
			set
			{
				portraitCharacterOverride = value;
			}
		}
		
	}
	
}
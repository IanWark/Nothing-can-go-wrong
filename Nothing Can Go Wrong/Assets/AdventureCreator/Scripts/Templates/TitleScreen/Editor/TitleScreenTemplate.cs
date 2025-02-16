#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace AC.Templates.TitleScreen
{

	public class TitleScreenTemplate : Template
	{

		#region Variables

		[SerializeField] private MenuManager menuManager = null;
		[SerializeField] protected SceneAsset sceneAsset = null;

		#endregion


		#region PublicFunctions

		public override bool CanInstall (ref string errorText)
		{
			if (Resource.References.settingsManager == null)
			{
				errorText = "No Settings Manager assigned";
				return false;
			}

			if (Resource.References.menuManager == null || menuManager == null)
			{
				errorText = "No Menu Manager assigned";
				return false;
			}

			if (Resource.References.menuManager == menuManager)
			{
				errorText = "Wrong Menu Manager assigned";
				return false;
			}

			if (sceneAsset == null)
			{
				errorText = "No Scene assigned";
				return false;
			}

			return true;
		}

#if UNITY_EDITOR

		public override void AmendData (NGWData data)
		{
			if (data.interfaceOption == NGWData.InterfaceOption.AdventureCreator)
			{
				data.interfaceOption = NGWData.InterfaceOption.UnityUI;
			}
		}

#endif

		#endregion


		#region ProtectedFunctions

		protected override void MakeChanges (string installPath, bool canDeleteOldAssets, System.Action onComplete, System.Action<string> onFail)
		{
			CopyMenus (installPath, menuManager, Resource.References.menuManager, canDeleteOldAssets ? ExistingMenuBehaviour.Delete : ExistingMenuBehaviour.Rename);

			foreach (Menu menu in Resource.References.menuManager.menus)
			{
				if (menu.title == "Title")
				{
					(menu.GetElementWithName ("GameName") as MenuLabel).label = Resource.References.settingsManager.saveFileName;

					ActionListAsset onClickContinue = CreateActionList_Continue ("Title_Continue_OnClick", installPath);
					(menu.GetElementWithName ("Continue") as MenuButton).actionList = onClickContinue;

					ActionListAsset onClickNewGame = CreateActionList_NewGame ("Title_NewGame_OnClick", installPath);
					(menu.GetElementWithName ("NewGame") as MenuButton).actionList = onClickNewGame;

					ActionListAsset onClickQuit = CreateActionList_Quit ("Title_Quit_OnClick", installPath);
					(menu.GetElementWithName ("Quit") as MenuButton).actionList = onClickQuit;
					break;
				}
			}

			CopyAsset<SceneAsset> (installPath, sceneAsset, ".unity");

			onComplete.Invoke ();
		}

		#endregion


		#region PrivateFunctions

		private ActionListAsset CreateActionList_Continue (string assetName, string installPath)
		{
			List<Action> actions = new List<Action>
			{
				ActionMenuState.CreateNew_TurnOffMenu ("Title", false, true, true),
				ActionFade.CreateNew (FadeType.fadeOut, 1f, true),
				ActionPause.CreateNew (1f),
				ActionSaveHandle.CreateNew_ContinueLast (),
			};

			ActionListAsset newAsset = ActionListAsset.CreateFromActions (assetName, installPath, actions, ActionListType.PauseGameplay);
			return newAsset;
		}


		private ActionListAsset CreateActionList_NewGame (string assetName, string installPath)
		{
			List<Action> actions = new List<Action>
			{
				ActionMenuState.CreateNew_TurnOffMenu ("Title", false, true, true),
				ActionFade.CreateNew (FadeType.fadeOut, 1f, true),
				ActionPause.CreateNew (1f),
				ActionComment.CreateNew ("To change which scene is run when beginning a new game, edit the Title_NewGame_OnClick ActionList."),
				ActionScene.CreateNew_Switch (1, false, false),
			};

			ActionListAsset newAsset = ActionListAsset.CreateFromActions (assetName, installPath, actions, ActionListType.PauseGameplay);
			return newAsset;
		}


		private ActionListAsset CreateActionList_Quit (string assetName, string installPath)
		{
			List<Action> actions = new List<Action>
			{
				ActionMenuState.CreateNew_TurnOffMenu ("Title", false, true, true),
				ActionFade.CreateNew (FadeType.fadeOut, 1f, true),
				ActionPause.CreateNew (1f),
				ActionEndGame.CreateNew_QuitGame (),
			};

			ActionListAsset newAsset = ActionListAsset.CreateFromActions (assetName, installPath, actions, ActionListType.PauseGameplay);
			return newAsset;
		}

		#endregion


		public override string Label { get { return "Title screen"; }}
		public override string PreviewText { get { return "Adds a 'Title screen' scene and Menu to begin the game from."; }}
		public override Type[] AffectedManagerTypes { get { return new Type[] { typeof (SettingsManager), typeof (MenuManager) }; }}
		public override bool RequiresInstallPath { get { return true; }}
		public override string FolderName { get { return "TitleScreen"; }}
		public override TemplateCategory Category { get { return TemplateCategory.Misc; }}
		public override bool IsExclusiveToCategory { get { return false; }}

	}


	[CustomEditor (typeof (TitleScreenTemplate))]
	public class TitleScreenTemplateEditor : TemplateEditor
	{}

}

#endif
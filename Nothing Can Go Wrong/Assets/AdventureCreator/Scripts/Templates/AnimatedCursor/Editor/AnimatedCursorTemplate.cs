#if UNITY_EDITOR

using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

namespace AC.Templates
{

	public class AnimatedCursorTemplate : Template
	{

		#region Variables

		[SerializeField] private UnityUICursor uiCursorPrefab = null;
		[SerializeField] private AnimatorController animator = null;

		#endregion


		#region PublicFunctions

		public override bool CanInstall (ref string errorText)
		{
			if (Resource.References.cursorManager == null)
			{
				errorText = "No Cursor Manager assigned";
				return false;
			}

			if (uiCursorPrefab == null)
			{
				errorText = "No UI Cursor prefab assigned";
				return false;
			}

			return true;
		}


		public override bool CanSuggest (NGWData data)
		{
			return data.InputMethod == InputMethod.MouseAndKeyboard;
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
			// Animator
			AnimatorController newAnimator = CopyAsset<AnimatorController> (installPath, animator, ".controller");
			if (newAnimator == null)
			{
				onFail.Invoke ("Controller copy failed.");
				return;
			}

			// Cursor
			UnityUICursor newUICursorPrefab = CopyAsset<UnityUICursor> (installPath, uiCursorPrefab, ".prefab");
			if (newUICursorPrefab == null)
			{
				onFail.Invoke ("Prefab copy failed.");
				return;
			}

			newUICursorPrefab.GetComponentInChildren<Animator> ().runtimeAnimatorController = newAnimator;
			EditorUtility.SetDirty (newUICursorPrefab.gameObject);

			// Settings
			Undo.RecordObjects (new UnityEngine.Object[] { Resource.References.cursorManager }, "");
			Resource.References.cursorManager.uiCursorPrefab = newUICursorPrefab.gameObject;
			Resource.References.cursorManager.cursorRendering = CursorRendering.UnityUI;
			EditorUtility.SetDirty (Resource.References.cursorManager);

			onComplete?.Invoke ();
		}

		#endregion


		#region GetSet

		public override string Label { get { return "Animated cursor"; }}
		public override string PreviewText { get { return "Helps make the interface feel more polished by adding some subtle animations to the cursor."; }}
		public override Type[] AffectedManagerTypes { get { return new Type[] { typeof (CursorManager) }; }}
		public override string FolderName { get { return "AnimatedCursor"; }}
		public override bool RequiresInstallPath { get { return true; }}
		public override bool SelectedByDefault { get { return false; }}
		public override TemplateCategory Category { get { return TemplateCategory.Misc; }}
		public override bool IsExclusiveToCategory { get { return false; }}
		
		#endregion

	}


	[CustomEditor (typeof (AnimatedCursorTemplate))]
	public class AnimatedCursorTemplateEditor : TemplateEditor
	{}

}

#endif
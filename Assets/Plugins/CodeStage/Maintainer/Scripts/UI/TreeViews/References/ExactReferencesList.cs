#region copyright
// ------------------------------------------------------------------------
//  Copyright (C) Dmitriy Yukhanov - focus [http://codestage.net]
// ------------------------------------------------------------------------
#endregion

namespace CodeStage.Maintainer.UI
{
	using System;
	using Core;
	using References;
	using Tools;
	using UnityEditor.IMGUI.Controls;
	using UnityEngine;

	internal class ExactReferencesList<T> : ListTreeView<T> where T : ExactReferencesListElement
	{
		public ExactReferencesList(TreeViewState state, TreeModel<T> model):base(state, model)
		{
		}

		protected override void PostInit()
		{
			base.PostInit();
			showAlternatingRowBackgrounds = false;
			rowHeight = RowHeight - 4;
		}

		protected override void DoubleClickedItem(int id)
		{
			base.DoubleClickedItem(id);
			var item = (ExactReferencesListItem<T>)FindItem(id, rootItem);
			ShowItem(item);
		}

		protected override TreeViewItem GetNewTreeViewItemInstance(int id, int depth, string name, T data)
		{
			return new ExactReferencesListItem<T>(id, depth, name, data);
		}

		protected override void RowGUI(RowGUIArgs args)
		{
			CenterRectUsingSingleLineHeight(ref args.rowRect);

			var item = (ExactReferencesListItem<T>)args.item;
			var lastRect = args.rowRect;
			lastRect.xMin += 4;

			if (item.data == null || item.data.entry == null)
			{
				GUI.Label(lastRect, item.displayName);
				return;
			}

			var entry = item.data.entry;
			Rect iconRect;
			if (entry.location == Location.NotFound)
			{
				iconRect = lastRect;
				iconRect.width = UIHelpers.WarningIconSize;
				iconRect.height = UIHelpers.WarningIconSize;

				GUI.DrawTexture(iconRect, CSEditorIcons.WarnSmallIcon, ScaleMode.ScaleToFit);
				lastRect.xMin += UIHelpers.WarningIconSize + UIHelpers.EyeButtonPadding;
			}
			else if (entry.location == Location.Invisible)
			{
				iconRect = lastRect;
				iconRect.width = UIHelpers.WarningIconSize;
				iconRect.height = UIHelpers.WarningIconSize;

				GUI.DrawTexture(iconRect, CSEditorIcons.InfoSmallIcon, ScaleMode.ScaleToFit);
				lastRect.xMin += UIHelpers.WarningIconSize + UIHelpers.EyeButtonPadding;
			}
			else
			{
				iconRect = lastRect;
				iconRect.width = UIHelpers.EyeButtonSize;
				iconRect.height = UIHelpers.EyeButtonSize;
				if (UIHelpers.IconButton(iconRect, CSIcons.Show))
				{
					ShowItem(item);
				}
				lastRect.xMin += UIHelpers.EyeButtonSize + UIHelpers.EyeButtonPadding;
			}

			var boxRect = iconRect;
			boxRect.height = lastRect.height;
			boxRect.xMin = iconRect.xMax;
			boxRect.xMax = lastRect.xMax;

			GUI.backgroundColor = entry.location != Location.NotFound ? CSColors.backgroundGreenTint : CSColors.backgroundRedTint;
			GUI.Box(boxRect, GUIContent.none);
			GUI.backgroundColor = Color.white;

			var label = entry.GetLabel();
			DefaultGUI.Label(lastRect, label, args.selected, args.focused);
		}

		private static void ShowItem(ExactReferencesListItem<T> item)
		{
			var assetPath = item.data.assetPath;
			var referencingEntry = item.data.entry;

			if (referencingEntry.location == Location.SceneLightingSettings ||
			    referencingEntry.location == Location.SceneNavigationSettings)
			{
				var sceneOpenResult = CSSceneTools.OpenSceneWithSavePrompt(assetPath);
				if (!sceneOpenResult.success)
				{
					Debug.LogError(Maintainer.ConstructError("Can't open scene " + assetPath));
					MaintainerWindow.ShowNotification("Can't show it properly");
					return;
				}
			}

			switch (referencingEntry.location)
			{
				case Location.ScriptAsset:
				case Location.ScriptableObjectAsset:

					if (!CSSelectionTools.RevealAndSelectFileAsset(assetPath))
					{
						MaintainerWindow.ShowNotification("Can't show it properly");
					}

					break;
				case Location.PrefabAssetObject:
					if (!CSSelectionTools.RevealAndSelectSubAsset(assetPath, referencingEntry.transformPath,
						referencingEntry.objectId))
					{
						MaintainerWindow.ShowNotification("Can't show it properly");
					}

					break;
				case Location.PrefabAssetGameObject:
				case Location.SceneGameObject:

					if (!CSSelectionTools.RevealAndSelectGameObject(assetPath, referencingEntry.transformPath,
						referencingEntry.objectId, referencingEntry.componentId))
					{
						MaintainerWindow.ShowNotification("Can't show it properly");
					}

					break;

				case Location.SceneLightingSettings:

					if (!CSMenuTools.ShowSceneSettingsLighting())
					{
						Debug.LogError(Maintainer.ConstructError("Can't open Lighting settings!"));
						MaintainerWindow.ShowNotification("Can't show it properly");
					}

					break;

				case Location.SceneNavigationSettings:

					if (!CSMenuTools.ShowSceneSettingsNavigation())
					{
						Debug.LogError(Maintainer.ConstructError("Can't open Navigation settings!"));
						MaintainerWindow.ShowNotification("Can't show it properly");
					}

					break;

				case Location.NotFound:
				case Location.Invisible:
					break;

				case Location.TileMap:

					if (!CSSelectionTools.RevealAndSelectGameObject(assetPath, referencingEntry.transformPath,
						referencingEntry.objectId, referencingEntry.componentId))
					{
						MaintainerWindow.ShowNotification("Can't show it properly");
					}

					// TODO: open tile map editor window?

					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

		}
	}
}
#region copyright
//------------------------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov - focus [http://codestage.net]
//------------------------------------------------------------------------
#endregion

namespace CodeStage.Maintainer.UI
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Core;
	using References;
	using Tools;

	using UnityEditor;
	using UnityEditor.IMGUI.Controls;
	using UnityEditor.VersionControl;
	using UnityEngine;

	internal class ReferencesTreeView<T> : MaintainerTreeView<T> where T : ReferencesTreeElement
	{
		private const int IconWidth = 16;
		private const int IconPadding = 7;
		private const int DepthIndentation = 10;
		
		public enum SortOption
		{
			AssetPath,
			AssetType,
			AssetSize,
			ReferencesCount,
		}

		private enum Columns
		{
			Path,
			Type,
			Size,
			ReferencesCount
		}

		// count should be equal to columns count
		private readonly SortOption[] sortOptions =
		{
			SortOption.AssetPath,
			SortOption.AssetType,
			SortOption.AssetSize,
			SortOption.ReferencesCount,
		};

		public ReferencesTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader, TreeModel<T> model) : base(state, multiColumnHeader, model) {}

		public void SelectRowWithPath(string path)
		{
			foreach (var row in rows)
			{
				var rowLocal = (MaintainerTreeViewItem<T>)row;

				if (rowLocal.data.assetPath == path)
				{
					EditorApplication.delayCall += () =>
					{
						var id = rowLocal.id;
						SetExpanded(id, true);

						var childId = -1;
						if (rowLocal.data.HasChildren && rowLocal.data.children.Count > 0)
						{
							var child = rowLocal.data.children[0];
							childId = child.id;
						}
						
						FrameItem(childId > -1 ? childId : id);

						SetSelection(new List<int> { id });
						SetFocusAndEnsureSelectedItem();

						MaintainerWindow.RepaintInstance();
					};
				}
			}
		}

		protected override bool CanMultiSelect(TreeViewItem item)
		{
			return false;
		}

		protected override void DoubleClickedItem(int id)
		{
			base.DoubleClickedItem(id);
			var item = (ReferencesTreeViewItem<T>)FindItem(id, rootItem);
			ShowItem(item);
		}

		protected override void PostInit()
		{
			columnIndexForTreeFoldouts = 0;
		}

		protected override IList<int> GetAncestors(int id)
		{
			return TreeModel.GetAncestors(id);
		}

		protected override IList<int> GetDescendantsThatHaveChildren(int id)
		{
			return TreeModel.GetDescendantsThatHaveChildren(id);
		}

		protected override TreeViewItem GetNewTreeViewItemInstance(int id, int depth, string name, T data)
		{
			return new ReferencesTreeViewItem<T>(id, depth, name, data);
		}

		protected override void SortByMultipleColumns()
		{
			var sortedColumns = multiColumnHeader.state.sortedColumns;

			if (sortedColumns.Length == 0)
				return;

			var myTypes = rootItem.children.Cast<ReferencesTreeViewItem<T>>();
			var orderedQuery = InitialOrder(myTypes, sortedColumns);
			for (var i = 1; i < sortedColumns.Length; i++)
			{
				var sortOption = sortOptions[sortedColumns[i]];
				var ascending = multiColumnHeader.IsSortedAscending(sortedColumns[i]);

				switch (sortOption)
				{
					case SortOption.AssetPath:
						orderedQuery = orderedQuery.ThenBy(l => l.data.assetPath, ascending);
						break;
					case SortOption.AssetType:
						orderedQuery = orderedQuery.ThenBy(l => l.data.assetTypeName, ascending);
						break;
					case SortOption.AssetSize:
						orderedQuery = orderedQuery.ThenBy(l => l.data.assetSize, ascending);
						break;
					case SortOption.ReferencesCount:
						orderedQuery = orderedQuery.ThenBy(l => l.data.ChildrenCount, ascending);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}

			rootItem.children = orderedQuery.Cast<TreeViewItem>().ToList();
		}

		private IOrderedEnumerable<ReferencesTreeViewItem<T>> InitialOrder(IEnumerable<ReferencesTreeViewItem<T>> myTypes, IList<int> history)
		{
			var sortOption = sortOptions[history[0]];
			var ascending = multiColumnHeader.IsSortedAscending(history[0]);

			switch (sortOption)
			{
				case SortOption.AssetPath:
					return myTypes.Order(l => l.data.assetPath, ascending);
				case SortOption.AssetType:
					return myTypes.Order(l => l.data.assetTypeName, ascending);
				case SortOption.AssetSize:
					return myTypes.Order(l => l.data.assetSize, ascending);
				case SortOption.ReferencesCount:
					return myTypes.Order(l => l.data.ChildrenCount, ascending);
				default:
					return myTypes.Order(l => l.data.name, ascending);
			}
		}

		protected override void RowGUI(RowGUIArgs args)
		{
			var item = (ReferencesTreeViewItem<T>)args.item;

			for (var i = 0; i < args.GetNumVisibleColumns(); ++i)
			{
				CellGUI(args.GetCellRect(i), item, (Columns)args.GetColumn(i), ref args);
			}
		}

		protected override DragAndDropVisualMode HandleDragAndDrop(DragAndDropArgs args)
		{
			var objectReferences = DragAndDrop.objectReferences;

			if (objectReferences == null || objectReferences.Length == 0)
			{
				return DragAndDropVisualMode.Rejected;
			}

			for (var i = 0; i < objectReferences.Length; i++)
			{
				var monoBehaviour = objectReferences[i] as MonoBehaviour;
				if (monoBehaviour == null) continue;
					
				var monoScript = MonoScript.FromMonoBehaviour(monoBehaviour);
				if (monoScript == null) continue;

				objectReferences[i] = monoScript;
			}

			var assetsPaths = ReferencesFinder.GetSelectedAssets(objectReferences);
			if (assetsPaths.Length == 0)
			{
				return DragAndDropVisualMode.Rejected;
			}

			if (Event.current.type == EventType.DragPerform)
			{
				EditorApplication.delayCall += () => { ReferencesFinder.FindAssetsReferences(assetsPaths.ToArray()); };
				DragAndDrop.AcceptDrag();
			}

			return DragAndDropVisualMode.Generic;
		}

		private void CellGUI(Rect cellRect, ReferencesTreeViewItem<T> item, Columns column, ref RowGUIArgs args)
		{
			baseIndent = item.depth * DepthIndentation;

			CenterRectUsingSingleLineHeight(ref cellRect);

			switch (column)
			{
				case Columns.Path:

					var iconPadding = !Provider.isActive ? 0 : IconPadding;
					var entryRect = cellRect;

					var num = GetContentIndent(item) + extraSpaceBeforeIconAndLabel;
					entryRect.xMin += num;

					if (item.icon != null)
					{
						var iconRect = entryRect;
						iconRect.width = IconWidth;
						iconRect.x += iconPadding;	
						iconRect.height = EditorGUIUtility.singleLineHeight;

						GUI.DrawTexture(iconRect, item.icon, ScaleMode.ScaleToFit);

						// BASED ON DECOMPILED CODE
						// AssetsTreeViewGUI:
						// float num = (!Provider.isActive) ? 0f : 7f;
						// iconRightPadding = num;
						// iconLeftPadding = num;

						// TreeViewGUI:
						// iconTotalPadding = iconLeftPadding + iconRightPadding

						entryRect.xMin +=

							// TreeViewGUI: public float k_IconWidth = 16f;
							IconWidth +

							// TreeViewGUI: iconTotalPadding
							iconPadding * 2 +

							// TreeViewGUI: public float k_SpaceBetweenIconAndText = 2f;
							2f;
					}

					Rect lastRect;

					var eyeButtonRect = entryRect;
					eyeButtonRect.width = UIHelpers.EyeButtonSize;
					eyeButtonRect.height = UIHelpers.EyeButtonSize;
					eyeButtonRect.x += UIHelpers.EyeButtonPadding;

					lastRect = eyeButtonRect;

					if (UIHelpers.IconButton(eyeButtonRect, CSIcons.Show))
					{
						ShowItem(item);
					}

					if (item.depth == 1 && item.data.isReferenced)
					{
						var findButtonRect = entryRect;
						findButtonRect.width = UIHelpers.EyeButtonSize;
						findButtonRect.height = UIHelpers.EyeButtonSize;
						findButtonRect.x += UIHelpers.EyeButtonPadding*2 + UIHelpers.EyeButtonSize;

						lastRect = findButtonRect;

						if (UIHelpers.IconButton(findButtonRect, CSIcons.Find, "Search for references"))
						{
							EditorApplication.delayCall += ()=>ReferencesFinder.FindAssetReferencesFromResults(item.data.assetPath);
						}
					}

					var labelRect = entryRect;
					labelRect.xMin = lastRect.xMax + UIHelpers.EyeButtonPadding;

					if (item.data.depth == 0 && !item.data.HasChildren)
					{
						GUI.contentColor = CSColors.labelDimmedColor;
					}
					DefaultGUI.Label(labelRect, args.label, args.selected, args.focused);

					GUI.contentColor = Color.white;

					break;

				case Columns.Type:

					DefaultGUI.Label(cellRect, item.data.assetTypeName, args.selected, args.focused);
					break;

				case Columns.Size:

					DefaultGUI.Label(cellRect, item.data.assetSizeFormatted, args.selected, args.focused);
					break;

				case Columns.ReferencesCount:

					if (item.depth == 0)
					{
						DefaultGUI.Label(cellRect, item.data.ChildrenCount.ToString(), args.selected, args.focused);
					}
					break;
				
				default:
					throw new ArgumentOutOfRangeException("column", column, null);
			}
		}

		private static void ShowItem(ReferencesTreeViewItem<T> item)
		{
			var assetPath = item.data.assetPath;
			if (item.data.assetSettingsKind == AssetSettingsKind.NotSettings)
			{
				if (!CSSelectionTools.RevealAndSelectFileAsset(assetPath))
				{
					MaintainerWindow.ShowNotification("Can't show it properly");
				}
			}
			else
			{
				if (!CSEditorTools.RevealInSettings(item.data.assetSettingsKind, assetPath))
				{
					MaintainerWindow.ShowNotification("Can't show it properly");
				}
			}
		}

		public static MultiColumnHeaderState CreateDefaultMultiColumnHeaderState()
		{
			var columns = new[]
			{
				new MultiColumnHeaderState.Column
				{
					headerContent = new GUIContent("Path", "Paths to the assets."),
					headerTextAlignment = TextAlignment.Left,
					sortedAscending = true,
					canSort = true,
					sortingArrowAlignment = TextAlignment.Center,
					width = 200,
					minWidth = 400,
					autoResize = true,
					allowToggleVisibility = false
				},
				new MultiColumnHeaderState.Column
				{
					headerContent = new GUIContent("Type", CSEditorIcons.FilterByType, "Assets types."),
					headerTextAlignment = TextAlignment.Left,
					sortedAscending = true,
					canSort = true,
					sortingArrowAlignment = TextAlignment.Center,
					width = 100,
					minWidth = 70,
					autoResize = false,
					allowToggleVisibility = true
				},
				new MultiColumnHeaderState.Column
				{
					headerContent = new GUIContent("Size", "Assets sizes."),
					headerTextAlignment = TextAlignment.Left,
					sortedAscending = false,
					canSort = true,
					sortingArrowAlignment = TextAlignment.Center,
					width = 100,
					minWidth = 70,
					autoResize = false,
					allowToggleVisibility = true
				},
				new MultiColumnHeaderState.Column
				{
					headerContent = new GUIContent("Refs", "Shows how much times asset was referenced somewhere."),
					headerTextAlignment = TextAlignment.Left,
					sortedAscending = false,
					canSort = true,
					sortingArrowAlignment = TextAlignment.Center,
					width = 50,
					minWidth = 33,
					maxWidth = 50,
					autoResize = false,
					allowToggleVisibility = true
				},
			};

			var state = new MultiColumnHeaderState(columns)
			{
				sortedColumns = new[] {0, 3},
				sortedColumnIndex = 3
			};
			return state;
		}
	}
}
#region copyright
// ------------------------------------------------------------------------
//  Copyright (C) Dmitriy Yukhanov - focus [http://codestage.net]
// ------------------------------------------------------------------------
#endregion

namespace CodeStage.Maintainer.UI
{
	using Core;
	using References;
	using UnityEditor.IMGUI.Controls;
	using UnityEngine;

	public class ExactReferencesListPanel
	{
		private ExactReferencesListElement[] listElements;
		private TreeModel<ExactReferencesListElement> listModel;
		private ExactReferencesList<ExactReferencesListElement> list;

		private MaintainerTreeViewItem<ReferencesTreeElement> lastSelectedRow;

		internal ExactReferencesListPanel(MaintainerWindow window)
		{
		}

		internal void Refresh(bool newData)
		{
			if (newData)
			{
				listModel = null;
			}

			if (listModel == null && lastSelectedRow != null)
			{
				UpdateTreeModel();
			}
		}

		internal virtual void Draw(MaintainerTreeViewItem<ReferencesTreeElement> selectedRow)
		{
				if (selectedRow == null)
				{
					DrawRow("Please select any child item above to look for exact references location.");
					return;
				}
				
				if (selectedRow.data == null)
				{
					DrawRow("Selected item has no exact references support.");
					return;
				}

				var entries = selectedRow.data.referencingEntries;

				if (entries == null || entries.Length == 0)
				{
					if (selectedRow.data.depth == 0)
					{
						DrawRow("Please select any child item above to look for exact references location.");
						return;
					}

					DrawRow("Selected item has no exact references support.");
					return;
				}

				if (lastSelectedRow != selectedRow)
				{
					lastSelectedRow = selectedRow;
					UpdateTreeModel();
				}

				DrawReferencesPanel();
		}

		private void DrawRow(string label)
		{
			lastSelectedRow = new ListTreeViewItem<ReferencesTreeElement>(0, 0, label, null)
			{
				depth = 0,
				id = 1
			};
			UpdateTreeModel();
			DrawReferencesPanel();
		}

		private void DrawReferencesPanel()
		{
			list.OnGUI(GUILayoutUtility.GetRect(0, 0, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)));
		}

		private void UpdateTreeModel()
		{
			listElements = GetTreeElementsFromRow(lastSelectedRow);
			listModel = new TreeModel<ExactReferencesListElement>(listElements);
			list = new ExactReferencesList<ExactReferencesListElement>(new TreeViewState(), listModel);
			list.Reload();
		}

		private ExactReferencesListElement[] GetTreeElementsFromRow(MaintainerTreeViewItem<ReferencesTreeElement> item)
		{
			var data = item.data;
			var entries = data != null ? data.referencingEntries : null;

			int count;
			if (entries != null && entries.Length > 0)
			{
				count = entries.Length + 1;
			}
			else
			{
				count = 2;
			}

			var result = new ExactReferencesListElement[count];
			result[0] = new ExactReferencesListElement
			{
				id = 0,
				name = "root",
				depth = -1
			};

			if (entries == null || entries.Length == 0)
			{
				result[1] = new ExactReferencesListElement
				{
					id = 1,
					entry = null,
					name = item.displayName
				};

				return result;
			}

			for (var i = 0; i < entries.Length; i++)
			{
				var entry = entries[i];
				result[i + 1] = new ExactReferencesListElement
				{
					id = i + 1,
					entry = entry,
					assetPath = item.data.assetPath
				};
			}

			return result;
		}
	}
}
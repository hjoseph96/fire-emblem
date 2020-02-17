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
	using Settings;
	using Tools;
	using UnityEditor;
	using UnityEditor.IMGUI.Controls;
	using UnityEngine;

	public class ReferencesTreePanel
	{
		private ReferencesTreeElement[] treeElements;
		private TreeModel<ReferencesTreeElement> treeModel;
		private ReferencesTreeView<ReferencesTreeElement> treeView;
		private SearchField searchField;

		//private readonly MaintainerWindow window;
		private readonly ExactReferencesListPanel exactReferencesPanel;

		private object splitterState;

		public ReferencesTreePanel(MaintainerWindow window)
		{
			//this.window = window;
			exactReferencesPanel = new ExactReferencesListPanel(window);
		}

		public void Refresh(bool newData)
		{
			if (newData)
			{
				MaintainerPersonalSettings.References.referencesTreeViewState = new TreeViewState();
				treeModel = null;
			}

			if (treeModel == null)
			{
				UpdateTreeModel();
			}

			exactReferencesPanel.Refresh(newData);
		}

		public void SelectItemWithPath(string path)
		{
			treeView.SelectRowWithPath(path);
		}

		private void UpdateTreeModel()
		{
			var firstInit = MaintainerPersonalSettings.References.referencesTreeHeaderState == null || MaintainerPersonalSettings.References.referencesTreeHeaderState.columns == null || MaintainerPersonalSettings.References.referencesTreeHeaderState.columns.Length == 0;
			var headerState = ReferencesTreeView<ReferencesTreeElement>.CreateDefaultMultiColumnHeaderState();
			if (MultiColumnHeaderState.CanOverwriteSerializedFields(MaintainerPersonalSettings.References.referencesTreeHeaderState, headerState))
				MultiColumnHeaderState.OverwriteSerializedFields(MaintainerPersonalSettings.References.referencesTreeHeaderState, headerState);
			MaintainerPersonalSettings.References.referencesTreeHeaderState = headerState;

			var multiColumnHeader = new MaintainerMultiColumnHeader(headerState);

			if (firstInit)
			{
				MaintainerPersonalSettings.References.referencesTreeViewState = new TreeViewState();
			}

			treeElements = LoadLastTreeElements();
			treeModel = new TreeModel<ReferencesTreeElement>(treeElements);
			treeView = new ReferencesTreeView<ReferencesTreeElement>(MaintainerPersonalSettings.References.referencesTreeViewState, multiColumnHeader, treeModel);
			treeView.SetSearchString(MaintainerPersonalSettings.References.searchString);
			treeView.Reload();

			searchField = new SearchField();
			searchField.downOrUpArrowKeyPressed += treeView.SetFocusAndEnsureSelectedItem;

			if (firstInit)
			{
				multiColumnHeader.ResizeToFit();
			}
		}

		public void Draw()
		{
			using (new GUILayout.HorizontalScope())
			{
				GUILayout.Space(5);
				using (new GUILayout.VerticalScope())
				{
					EditorGUI.BeginChangeCheck();
					var searchString =
						searchField.OnGUI(
							GUILayoutUtility.GetRect(0, 0, 20, 20, GUILayout.ExpandWidth(true),
								GUILayout.ExpandHeight(false)), MaintainerPersonalSettings.References.searchString);
					if (EditorGUI.EndChangeCheck())
					{
						MaintainerPersonalSettings.References.searchString = searchString;
						treeView.SetSearchString(searchString);
						treeView.Reload();
					}

					GetSplitterState();

					CSReflectionTools.BeginVerticalSplit(splitterState, new GUILayoutOption[0]);

					using (new GUILayout.VerticalScope())
					{
						treeView.OnGUI(GUILayoutUtility.GetRect(0, 0, GUILayout.ExpandWidth(true),
							GUILayout.ExpandHeight(true)));
						GUILayout.Space(2f);
					}

					using (new GUILayout.VerticalScope())
					{
						GUILayout.Space(2f);

						using (new GUILayout.VerticalScope(UIHelpers.panelWithoutBackground))
						{
							GUILayout.Label("Exact references", UIHelpers.centeredLabel);
							GUILayout.Space(1f);
						}

						GUILayout.Space(-1f);

						var selected = treeView.GetSelection();
						if (selected != null && selected.Count > 0)
						{
							var selectedRow = treeView.GetRow(selected[0]);
							exactReferencesPanel.Draw(selectedRow);
						}
						else
						{
							exactReferencesPanel.Draw(null);
						}
					}

					CSReflectionTools.EndVerticalSplit();

					SaveSplitterState();
				}

				GUILayout.Space(5);
			}
		}

		private void GetSplitterState()
		{
			if (splitterState != null)
			{
				return;
			}

			var savedState = MaintainerPersonalSettings.References.splitterState;
			object result;

			try
			{
				if (!string.IsNullOrEmpty(savedState))
				{
					result = JsonUtility.FromJson(savedState, CSReflectionTools.splitterStateType);
				}
				else
				{
					result = Activator.CreateInstance(CSReflectionTools.splitterStateType, 
						new [] {100f, 50f}, 
						new [] {90, 47}, 
						null);
				}
			}
			catch (Exception e)
			{
				Debug.LogError(Maintainer.ConstructError("Couldn't create instance of the SplitterState class!\n" + e, ReferencesFinder.ModuleName));
				throw e;
			}

			splitterState = result;
		}

		private void SaveSplitterState()
		{
			MaintainerPersonalSettings.References.splitterState = EditorJsonUtility.ToJson(splitterState, false);
		}

		public void CollapseAll()
		{
			treeView.CollapseAll();
		}

		public void ExpandAll()
		{
			treeView.ExpandAll();
		}

		private ReferencesTreeElement[] LoadLastTreeElements()
		{
			var loaded = SearchResultsStorage.ReferencesSearchResults;
			if (loaded == null || loaded.Length == 0)
			{
				loaded = new ReferencesTreeElement[1];
				loaded[0] = new ReferencesTreeElement { id = 0, depth = -1, name = "root" };
			}
			return loaded;
		}
	}
}
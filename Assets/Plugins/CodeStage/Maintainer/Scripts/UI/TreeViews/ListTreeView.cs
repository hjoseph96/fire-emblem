#region copyright
// ------------------------------------------------------------------------
//  Copyright (C) Dmitriy Yukhanov - focus [http://codestage.net]
// ------------------------------------------------------------------------
#endregion

namespace CodeStage.Maintainer.UI
{
	using Core;
	using UnityEditor.IMGUI.Controls;

	internal class ListTreeViewItem<T> : MaintainerTreeViewItem<T> where T : TreeElement
	{
		internal ListTreeViewItem(int id, int depth, string displayName, T data) : base(id, depth, displayName, data)
		{
			
		}
	}

	internal class ListTreeView<T> : MaintainerTreeView<T> where T : TreeElement
	{
		public ListTreeView(TreeViewState state, TreeModel<T> model) : base(state, model)
		{

		}

		public ListTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader, TreeModel<T> model) : base(state, multiColumnHeader, model)
		{

		}

		protected override bool CanMultiSelect(TreeViewItem item)
		{
			return false;
		}

		protected override TreeViewItem GetNewTreeViewItemInstance(int id, int depth, string name, T data)
		{
			return new ListTreeViewItem<T>(id, depth, name, data);
		}

		protected override void SortByMultipleColumns()
		{
			return;
		}
	}
}
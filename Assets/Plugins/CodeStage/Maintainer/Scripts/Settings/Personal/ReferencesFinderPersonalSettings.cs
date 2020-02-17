#region copyright
//------------------------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov - focus [http://codestage.net]
//------------------------------------------------------------------------
#endregion

#pragma warning disable 0414

namespace CodeStage.Maintainer.Settings
{
	using System;
	using UnityEditor.IMGUI.Controls;

	/// <summary>
	/// References Finder module settings saved in Library or UserSettings (since Unity 2020.1) folder.
	/// </summary>
	/// Contains user-specific settings for this module.
	/// See IDE hints for all list.
	[Serializable]
	public class ReferencesFinderPersonalSettings
	{
		public bool showAssetsWithoutReferences;
		public bool selectedFindClearsResults;

		public bool fullProjectScanWarningShown;
		public string searchString;

		public TreeViewState referencesTreeViewState;
		public MultiColumnHeaderState referencesTreeHeaderState;

		public string splitterState;
	}
}
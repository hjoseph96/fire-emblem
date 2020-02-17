#region copyright
//------------------------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov - focus [http://codestage.net]
//------------------------------------------------------------------------
#endregion

#pragma warning disable 0414

namespace CodeStage.Maintainer.Settings
{
	using System;
	using Cleaner;
	using UI;

	/// <summary>
	/// Project Cleaner module settings saved in Library or UserSettings (since Unity 2020.1) folder.
	/// </summary>
	/// Contains user-specific settings for this module.
	/// See IDE hints for all list.
	[Serializable]
	public class ProjectCleanerPersonalSettings
	{
		public RecordsTabState tabState;

		public int filtersTabIndex = 0;

		public bool firstTime = true;
		public bool trashBinWarningShown = false;
		public bool deletionPromptShown = false;

		/* sorting */

		public CleanerSortingType sortingType = CleanerSortingType.BySize;
		public SortingDirection sortingDirection = SortingDirection.Ascending;
	}
}
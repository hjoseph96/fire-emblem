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
	/// Issues Finder module settings saved in Library or UserSettings (since Unity 2020.1) folder.
	/// </summary>
	/// Contains user-specific settings for this module.
	/// See IDE hints for all list.
	[Serializable]
	public class IssuesFinderPersonalSettings
	{
		public RecordsTabState tabState = new RecordsTabState();
		public int filtersTabIndex = 0;

		/* sorting */

		public IssuesSortingType sortingType = IssuesSortingType.BySeverity;
		public SortingDirection sortingDirection = SortingDirection.Ascending;
	}
}
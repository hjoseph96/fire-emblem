#region copyright
//------------------------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov - focus [http://codestage.net]
//------------------------------------------------------------------------
#endregion

namespace CodeStage.Maintainer.References
{
	using System;
	using Core;

	[Serializable]
	public class ExactReferencesListElement : TreeElement
	{
		public string assetPath;
		public ReferencingEntryData entry;
	}
}
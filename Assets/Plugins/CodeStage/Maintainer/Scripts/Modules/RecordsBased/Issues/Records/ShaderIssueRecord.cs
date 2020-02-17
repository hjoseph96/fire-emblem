#region copyright
//------------------------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov - focus [http://codestage.net]
//------------------------------------------------------------------------
#endregion

#if UNITY_2019_1_OR_NEWER
namespace CodeStage.Maintainer.Issues
{
	using System;
	using System.Text;
	using Core;
	using Tools;
	using UI;

	[Serializable]
	public class ShaderIssueRecord : AssetIssueRecord, IShowableRecord
	{
		public void Show()
		{
			if (!CSSelectionTools.RevealAndSelectFileAsset(Path))
			{
				MaintainerWindow.ShowNotification("Can't show it properly");
			}
		}

		internal static ShaderIssueRecord Create(string path)
		{
			return new ShaderIssueRecord(path);
		}

		internal override bool MatchesFilter(FilterItem newFilter)
		{
			return false;
		}

		internal override bool CanBeFixed()
		{
			return false;
		}

		protected ShaderIssueRecord(string path):base(IssueKind.ShaderError, RecordLocation.Asset, path)
		{

		}

		protected override void ConstructBody(StringBuilder text)
		{
			text.Append("<b>Shader:</b> ");
			text.Append(CSPathTools.NicifyAssetPath(Path, true));
		}
	}
}
#endif
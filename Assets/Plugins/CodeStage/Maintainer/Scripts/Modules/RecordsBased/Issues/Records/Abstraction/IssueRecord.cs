#region copyright
//------------------------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov - focus [http://codestage.net]
//------------------------------------------------------------------------
#endregion

namespace CodeStage.Maintainer.Issues
{
	using System;
	using System.Collections.Generic;
	using System.Text;

	[Serializable]
	public class FixResult
	{
		public bool Success { get; private set; }
		public string ErrorText { get; private set; }

		public FixResult(bool success, string errorText = null)
		{
			Success = success;
			ErrorText = errorText;
		}

		public static FixResult CreateError(string errorText)
		{
			return new FixResult(false, errorText);
		}

		public void SetErrorText(string errorText)
		{
			ErrorText = errorText;
		}
	}

	[Serializable]
	public abstract class IssueRecord: RecordBase
	{
		private static readonly Dictionary<IssueKind, RecordSeverity> RecordTypeSeverity = new Dictionary<IssueKind, RecordSeverity>
		{
			{IssueKind.MissingComponent, RecordSeverity.Error},
			{IssueKind.MissingPrefab, RecordSeverity.Error},
#if UNITY_2019_1_OR_NEWER
			{IssueKind.ShaderError, RecordSeverity.Error},
#endif
			{IssueKind.MissingReference, RecordSeverity.Warning},
			{IssueKind.DuplicateComponent, RecordSeverity.Warning},
			{IssueKind.InconsistentTerrainData, RecordSeverity.Warning},
			{IssueKind.UnnamedLayer, RecordSeverity.Info},
			{IssueKind.HugePosition, RecordSeverity.Warning},
			{IssueKind.DuplicateLayers, RecordSeverity.Info},
			{IssueKind.Other, RecordSeverity.Info},
			{IssueKind.Error, RecordSeverity.Error}
		};

		public IssueKind Kind { get; private set; }
		public RecordSeverity Severity { get; private set; }

		public FixResult fixResult;

		internal FixResult Fix(bool batchMode)
		{
			fixResult = PerformFix(batchMode);
			return fixResult;
		}

		internal abstract bool CanBeFixed();

		// ----------------------------------------------------------------------------
		// base constructors
		// ----------------------------------------------------------------------------

		protected IssueRecord(IssueKind kind, RecordLocation location):base(location)
		{
			Kind = kind;
			Severity = RecordTypeSeverity[kind];
		}

		// ----------------------------------------------------------------------------
		// issue compact line generation
		// ----------------------------------------------------------------------------

		protected override void ConstructCompactLine(StringBuilder text)
		{
			ConstructHeader(text);
		}

		// ----------------------------------------------------------------------------
		// issue header generation
		// ----------------------------------------------------------------------------

		protected override void ConstructHeader(StringBuilder text)
		{
			switch (Kind)
			{
				case IssueKind.MissingComponent:
					text.Append(headerFormatArgument > 1 ? string.Format("{0} missing components", headerFormatArgument) : "Missing component");
					break;
				case IssueKind.MissingReference:
					text.Append("Missing reference");
					break;
				case IssueKind.DuplicateComponent:
					text.Append("Duplicate component");
					break;
				case IssueKind.MissingPrefab:
					text.Append("Instance of missing prefab");
					break;
				case IssueKind.UnnamedLayer:
					text.Append("GameObject with unnamed layer");
					break;
				case IssueKind.HugePosition:
					text.Append("GameObject with huge position");
					break;
				case IssueKind.InconsistentTerrainData:
					text.Append("Terrain and TerrainCollider with different Terrain Data");
					break;
				case IssueKind.DuplicateLayers:
					text.Append("Duplicate layer(s) found at the 'Tags and Layers' settings");
					break;
#if UNITY_2019_1_OR_NEWER
				case IssueKind.ShaderError:
					text.Append("Shader with error(s)");
					break;
#endif
				case IssueKind.Error:
					text.Append("Error!");
					break;
				case IssueKind.Other:
					text.Append("Other");
					break;
				default:
					text.Append("Unknown issue!");
					break;
			}
		}

		protected virtual FixResult PerformFix(bool batchMode)
		{
			return null;
		}
	}
}
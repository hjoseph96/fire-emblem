#region copyright
//------------------------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov - focus [http://codestage.net]
//------------------------------------------------------------------------
#endregion

namespace CodeStage.Maintainer.References
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Globalization;
	using System.IO;
	using UnityEditor;

	using Core;
	using Entry;
	using Routines;
	using Settings;
	using Tools;
	using UI;
	using Debug = UnityEngine.Debug;
	using Object = UnityEngine.Object;

	/// <summary>
	/// Allows to find references of specific objects in your project (where objects are referenced).
	/// </summary>
	public class ReferencesFinder
	{
		internal const string ModuleName = "References Finder";

		internal const string ProgressCaption = ModuleName + ": phase {0} of {1}";
		internal const string ProgressText = "{0}: asset {1} of {2}";
		internal const int PhasesCount = 2;

		public static bool debugMode;

		internal static readonly List<AssetConjunctions> ConjunctionInfoList = new List<AssetConjunctions>();

		/// <summary>
		/// Finds references for current Project View selection.
		/// </summary>
		/// <param name="showResults">Shows results in %Maintainer window if true.</param>
		/// <returns>Array of ReferencesTreeElement for the TreeView buildup or manual parsing.</returns>
		public static ReferencesTreeElement[] FindSelectedAssetsReferences(bool showResults = true)
		{
			var selection = GetSelectedAssets();
			return FindAssetsReferences(selection);
		}

		/// <summary>
		/// Finds references for specific asset.
		/// </summary>
		/// <param name="asset">Specific asset.</param>
		/// <param name="showResults">Shows results in %Maintainer window if true.</param>
		/// <returns>Array of ReferencesTreeElement for the TreeView buildup or manual parsing.</returns>
		public static ReferencesTreeElement[] FindAssetReferences(string asset, bool showResults = true)
		{
			return FindAssetsReferences(new []{ asset }, showResults);
		}

		/// <summary>
		/// Finds references for specific assets.
		/// </summary>
		/// <param name="assets">Specific assets.</param>
		/// <param name="showResults">Shows results in %Maintainer window if true.</param>
		/// <returns>Array of ReferencesTreeElement for the TreeView buildup or manual parsing.</returns>
		public static ReferencesTreeElement[] FindAssetsReferences(string[] assets, bool showResults = true)
		{
			var assetsFilters = new FilterItem[assets.Length];
			for (var i = 0; i < assets.Length; i++)
			{
				assetsFilters[i] = FilterItem.Create(assets[i], FilterKind.Path);
			}

			return FindAssetsReferences(assetsFilters, false, showResults);
		}

		/// <summary>
		/// Returns references of all assets at the project, e.g. where each asset is referenced.
		/// </summary>
		/// <param name="showResults">Shows results in %Maintainer window if true.</param>
		/// <returns>Array of ReferencesTreeElement for the TreeView buildup or manual parsing.</returns>
		public static ReferencesTreeElement[] FindAllAssetsReferences(bool showResults = true)
		{
			if (!MaintainerPersonalSettings.References.fullProjectScanWarningShown)
			{
				if (!EditorUtility.DisplayDialog(ModuleName,
					"Full project scan may take significant amount of time if your project is very big.\nAre you sure you wish to make a full project scan?\nThis message shows only before first full scan.",
					"Yes", "Nope"))
				{
					return null;
				}

				MaintainerPersonalSettings.References.fullProjectScanWarningShown = true;
				MaintainerSettings.Save();
			}

			return GetReferences(null, null, showResults);
		}

		internal static ReferencesTreeElement[] FindAssetReferencesFromResults(string asset)
		{
			return FindAssetsReferences(new []{FilterItem.Create(asset, FilterKind.Path)}, true, true);
		}

		internal static ReferencesTreeElement[] FindAssetsReferences(FilterItem[] assets, bool ignoreClearOption, bool showResults)
		{
			if (MaintainerPersonalSettings.References.selectedFindClearsResults && !ignoreClearOption)
			{
				SearchResultsStorage.ReferencesFinderLastSearched = new FilterItem[0];
				SearchResultsStorage.ReferencesSearchResults = new ReferencesTreeElement[0];
			}

			var lastSearched = SearchResultsStorage.ReferencesFinderLastSearched;

			var newItem = false;

			foreach (var asset in assets)
			{
				newItem |= CSFilterTools.TryAddNewItemToFilters(ref lastSearched, asset);
			}

			if (assets.Length == 1)
			{
				ReferencesTab.AutoSelectPath = assets[0].value;
			}

			if (newItem)
			{
				return GetReferences(lastSearched, assets, showResults);
			}

			//ReferencesTab.AutoShowExistsNotification = true;
			MaintainerWindow.ShowReferences();

			return SearchResultsStorage.ReferencesSearchResults;
		}

		internal static ReferencesTreeElement[] GetReferences(FilterItem[] allTargetAssets, FilterItem[] newTargetAssets, bool showResults = true)
		{
			var results = new List<ReferencesTreeElement>();

			ConjunctionInfoList.Clear();

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

			try
			{
				var sw = Stopwatch.StartNew();

				CSEditorTools.lastRevealSceneOpenResult = null;

				var searchCanceled = LookForReferences(allTargetAssets, results);
				sw.Stop();

				EditorUtility.ClearProgressBar();

				if (!searchCanceled)
				{
					var resultsCount = results.Count;
					if (resultsCount <= 1)
					{
						ReferencesTab.AutoSelectPath = null;
						MaintainerWindow.ShowNotification("Nothing found!");
					}
					else if (newTargetAssets != null && newTargetAssets.Length > 0)
					{
						var found = false;
						foreach (var result in results)
						{
							if (result.depth == 0 && CSFilterTools.IsValueMatchesAnyFilter(result.assetPath, newTargetAssets))
							{
								found = true;
								break;
							}
						}

						if (!found)
						{
							ReferencesTab.AutoSelectPath = null;
							MaintainerWindow.ShowNotification("Nothing found!");
						}
					}

					Debug.Log(Maintainer.LogPrefix + ModuleName + " results: " + (resultsCount - 1) +
					          " items found in " + sw.Elapsed.TotalSeconds.ToString("0.000", CultureInfo.InvariantCulture) +
					          " seconds.");
				}
				else
				{
					Debug.Log(Maintainer.LogPrefix + ModuleName + "Search canceled by user!");
				}
			}
			catch (Exception e)
			{
				Debug.LogError(Maintainer.LogPrefix + ModuleName + ": " + e);
				EditorUtility.ClearProgressBar();
			}

			BuildSelectedAssetsFromResults(results);

			SearchResultsStorage.ReferencesSearchResults = results.ToArray();

			if (showResults)
			{
				MaintainerWindow.ShowReferences();
			}

			return results.ToArray();
		}

		internal static string[] GetSelectedAssets()
		{
			var selectedIDs = Selection.instanceIDs;
			return GetSelectedAssets(selectedIDs);
		}

		internal static string[] GetSelectedAssets(Object[] objects)
		{
			var selectedIDs = new int[objects.Length];
			for (var i = 0; i < objects.Length; i++)
			{
				selectedIDs[i] = objects[i].GetInstanceID();
			}

			return GetSelectedAssets(selectedIDs);
		}

		internal static string[] GetSelectedAssets(int[] instanceIDs)
		{
			var paths = new List<string>(instanceIDs.Length);

			foreach (var id in instanceIDs)
			{
				if (AssetDatabase.IsSubAsset(id)) continue;
				var path = AssetDatabase.GetAssetPath(id);
				path = CSPathTools.EnforceSlashes(path);
				if (!File.Exists(path) && !Directory.Exists(path)) continue;
				paths.Add(path);
			}

			return paths.ToArray();
		}

		private static void BuildSelectedAssetsFromResults(List<ReferencesTreeElement> results)
		{
			var resultsCount = results.Count;
			var showProgress = resultsCount > 500000;

			if (showProgress) EditorUtility.DisplayProgressBar(ModuleName, "Parsing results...", 0);

			var rootItems = new List<FilterItem>(resultsCount);
			var updateStep = Math.Max(resultsCount / MaintainerSettings.UpdateProgressStep, 1);
			for (var i = 0; i < resultsCount; i++)
			{
				if (showProgress && i % updateStep == 0) EditorUtility.DisplayProgressBar(ModuleName, "Parsing results...", (float)i / resultsCount);

				var result = results[i];
				if (result.depth != 0) continue;
				rootItems.Add(FilterItem.Create(result.assetPath, FilterKind.Path));
			}

			SearchResultsStorage.ReferencesFinderLastSearched = rootItems.ToArray();
		}

		private static bool LookForReferences(FilterItem[] selectedAssets, List<ReferencesTreeElement> results)
		{
			var canceled = !CSSceneTools.SaveCurrentModifiedScenes(false);

			if (!canceled)
			{
				var map = AssetsMap.GetUpdated();
				if (map == null) return true;

				var count = map.assets.Count;
				var updateStep = Math.Max(count / MaintainerSettings.UpdateProgressStep, 1);

				var root = new ReferencesTreeElement
				{
					id = results.Count,
					name = "root",
					depth = -1
				};
				results.Add(root);

				for (var i = 0; i < count; i++)
				{
					if (i % updateStep == 0 && EditorUtility.DisplayCancelableProgressBar(
						    string.Format(ProgressCaption, 1, PhasesCount),
						    string.Format(ProgressText, "Building references tree", i + 1, count),
						    (float) i / count))
					{
						canceled = true;
						break;
					}

					var assetInfo = map.assets[i];

					// excludes settings assets from the list depth 0 items
					if (assetInfo.Kind == AssetKind.Settings) continue;

					// excludes all assets except selected ones from the list depth 0 items, if any was selected
					if (selectedAssets != null)
					{
						if (!CSFilterTools.IsValueMatchesAnyFilter(assetInfo.Path, selectedAssets)) continue;
					}

					if (MaintainerSettings.References.pathIncludesFilters != null &&
					    MaintainerSettings.References.pathIncludesFilters.Length > 0)
					{
						// excludes all root assets except included ones
						if (!CSFilterTools.IsValueMatchesAnyFilter(assetInfo.Path, MaintainerSettings.References.pathIncludesFilters)) continue;
					}

					// excludes ignored root asset
					if (CSFilterTools.IsValueMatchesAnyFilter(assetInfo.Path, MaintainerSettings.References.pathIgnoresFilters)) continue;

					var branchElements = new List<ReferencesTreeElement>();
					TreeBuilder.BuildTreeBranch(assetInfo, 0, results.Count, branchElements);
					results.AddRange(branchElements);
				}
			}

			if (!canceled)
			{
				canceled = ReferenceEntryFinder.FillReferenceEntries();
			}

			if (!canceled)
			{
				AssetsMap.Save();
			}

			if (canceled)
			{
				ReferencesTab.AutoShowExistsNotification = false;
				ReferencesTab.AutoSelectPath = null;
			}

			return canceled;
		}
	}
}
#region copyright
//------------------------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov - focus [http://codestage.net]
//------------------------------------------------------------------------
#endregion

namespace CodeStage.Maintainer.Core
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.IO;

	using UnityEditor;
	using UnityEditor.Compilation;
	using UnityEngine;

	using Tools;

	[Serializable]
	public enum AssetKind
	{
		Regular = 0,
		Settings = 10,
		FromPackage = 20,
		Unsupported = 100
	}

	[Serializable]
	public enum AssetSettingsKind
	{
		NotSettings = 0,
		AudioManager = 100,
		ClusterInputManager = 200,
		DynamicsManager = 300,
		EditorBuildSettings = 400,
		EditorSettings = 500,
		GraphicsSettings = 600,
		InputManager = 700,
		NavMeshAreas = 800,
		NavMeshLayers = 900,
		NavMeshProjectSettings = 1000,
		NetworkManager = 1100,
		Physics2DSettings = 1200,
		ProjectSettings = 1300,
		PresetManager = 1400,
		QualitySettings = 1500,
		TagManager = 1600,
		TimeManager = 1700,
		UnityAdsSettings = 1800,
		UnityConnectSettings = 1900,
		VFXManager = 2000,
		Unknown = 100000
	}

	public class RawAssetInfo
	{
		public string path;
		public string guid;
		public AssetKind kind;
	}

	[Serializable]
	public class AssetInfo
	{
		public string GUID { get; private set; }
		public string Path { get; private set; }
		public AssetKind Kind { get; private set; }
		public AssetSettingsKind SettingsKind { get; private set; }
		public Type Type { get; private set; }
		public long Size { get; private set; }

		public string[] dependenciesGUIDs = new string[0];
		public ReferenceInfo[] referencesInfo = new ReferenceInfo[0];
		public ReferencedAtInfo[] referencedAtInfoList = new ReferencedAtInfo[0];

		public bool needToRebuildReferences = true;

		private ulong lastHash;
		private FileInfo fileInfo;
		private FileInfo metaFileInfo;

		[NonSerialized]
		private int[] allAssetObjects;

		public static AssetInfo Create(RawAssetInfo rawAssetInfo, Type type, AssetSettingsKind settingsKind)
		{
			if (string.IsNullOrEmpty(rawAssetInfo.guid))
			{
				Debug.LogError("Can't create AssetInfo since guid is invalid!");
				return null;
			}

			var newAsset = new AssetInfo
			{
				GUID = rawAssetInfo.guid,
				Path = rawAssetInfo.path,
				Kind = rawAssetInfo.kind,
				Type = type,
				SettingsKind = settingsKind,
				fileInfo = new FileInfo(rawAssetInfo.path),
				metaFileInfo = new FileInfo(rawAssetInfo.path + ".meta")
			};

			newAsset.UpdateIfNeeded();

			return newAsset;
		}

		private AssetInfo() { }

		public bool Exists()
		{
			ActualizePath();
			fileInfo.Refresh();
			return fileInfo.Exists;
		}

		public void UpdateIfNeeded()
		{
			if (string.IsNullOrEmpty(Path))
			{
				Debug.LogWarning(Maintainer.LogPrefix + "Can't update Asset since path is not set!");
				return;
			}

			/*if (Path.Contains("qwerty.unity"))
			{
				Debug.Log(Path);
			}*/

			fileInfo.Refresh();

			if (!fileInfo.Exists)
			{
				Debug.LogWarning(Maintainer.LogPrefix + "Can't update asset since file at path is not found:\n" + fileInfo.FullName + "\nAsset Path: " + Path);
				return;
			}

			ulong currentHash = 0;

			if (metaFileInfo == null)
			{
				metaFileInfo = new FileInfo(fileInfo.FullName + ".meta");
			}

			metaFileInfo.Refresh();
			if (metaFileInfo.Exists)
			{
				currentHash += (ulong)metaFileInfo.LastWriteTimeUtc.Ticks;
				currentHash += (ulong)metaFileInfo.Length;
			}

			currentHash += (ulong)fileInfo.LastWriteTimeUtc.Ticks;
			currentHash += (ulong)fileInfo.Length;

			if (lastHash == currentHash)
			{
				for (var i = dependenciesGUIDs.Length - 1; i > -1; i--)
				{
					var guid = dependenciesGUIDs[i];
					var path = AssetDatabase.GUIDToAssetPath(guid);
					path = CSPathTools.EnforceSlashes(path);
					if (!string.IsNullOrEmpty(path) && File.Exists(path)) continue;

					ArrayUtility.RemoveAt(ref dependenciesGUIDs, i);
					foreach (var referenceInfo in referencesInfo)
					{
						if (referenceInfo.assetInfo.GUID != guid) continue;

						ArrayUtility.Remove(ref referencesInfo, referenceInfo);
						break;
					}
				}

				if (!needToRebuildReferences) return;
			}

			foreach (var referenceInfo in referencesInfo)
			{
				foreach (var info in referenceInfo.assetInfo.referencedAtInfoList)
				{
					if (info.assetInfo != this) continue;

					ArrayUtility.Remove(ref referenceInfo.assetInfo.referencedAtInfoList, info);
					break;
				}
			}

			lastHash = currentHash;

			needToRebuildReferences = true;
			Size = fileInfo.Length;

			referencesInfo = new ReferenceInfo[0];
			dependenciesGUIDs = new string[0];

			var dependencies = new List<string>();

			if (SettingsKind == AssetSettingsKind.NotSettings)
			{
				var getRegularDependencies = true;

				/* pre-regular dependencies additions */

				if (Type == CSReflectionTools.assemblyDefinitionAssetType)
				{
					if (Kind == AssetKind.Regular)
					{
						//TODO: check if bug 1020737 is fixed and this can be removed
						dependencies.AddRange(GetAssetsReferencedFromAssemblyDefinition(Path));
						getRegularDependencies = false;
					}
				}

#if UNITY_2019_2_OR_NEWER
				if (Type == CSReflectionTools.assemblyDefinitionReferenceAssetType)
				{
					if (Kind == AssetKind.Regular)
					{
						dependencies.AddRange(GetAssetsReferencedFromAssemblyDefinitionReference(Path));
						getRegularDependencies = false;
					}
				}
#endif
				
#if UNITY_2018_2_OR_NEWER
				// checking by name since addressables are in optional external package
				if (Type != null && Type.Name == "AddressableAssetGroup")
				{
					var references = AddressablesReferenceFinder.Extract(Path);
					if (references != null && references.Count > 0)
					{
						dependencies.AddRange(references);
					}
				}
#endif

				/* regular dependencies additions */

				if (getRegularDependencies) dependencies.AddRange(AssetDatabase.GetDependencies(Path, false));

				/* post-regular dependencies additions */

				if (Type == CSReflectionTools.spriteAtlasType)
				{
					CSArrayTools.TryAddIfNotExists(ref dependencies, GetGetAssetsInFoldersReferencedFromSpriteAtlas(Path));
				}
			}
			else
			{
				dependencies.AddRange(GetAssetsReferencedInPlayerSettingsAsset(Path, SettingsKind));
			}

			// kept for debugging purposes
			/*if (Path.Contains("1.unity"))
			{
				Debug.Log("1.unity non-recursive dependencies:");
				foreach (var reference in references)
				{
					Debug.Log(reference);
				}
			}*/

			if (Type == CSReflectionTools.shaderType)
			{
				// below is an another workaround for dependencies not include #include-ed files, like *.cginc
				ScanFileForIncludes(dependencies, Path);
			}

			if (Type == CSReflectionTools.textAssetType && Path.EndsWith(".cginc"))
			{
				// below is an another workaround for dependencies not include #include-ed files, like *.cginc
				ScanFileForIncludes(dependencies, Path);
			}

			var guids = new string[dependencies.Count];

			for (var i = 0; i < dependencies.Count; i++)
			{
				guids[i] = AssetDatabase.AssetPathToGUID(dependencies[i]);
			}

			dependenciesGUIDs = guids;
		}

		public List<AssetInfo> GetReferencesRecursive()
		{
			var result = new List<AssetInfo>();

			WalkReferencesRecursive(result, referencesInfo);

			return result;
		}

		public List<AssetInfo> GetReferencedAtRecursive()
		{
			var result = new List<AssetInfo>();

			WalkReferencedAtRecursive(result, referencedAtInfoList);

			return result;
		}

		public void Clean()
		{
			foreach (var referenceInfo in referencesInfo)
			{
				foreach (var info in referenceInfo.assetInfo.referencedAtInfoList)
				{
					if (info.assetInfo != this) continue;
					ArrayUtility.Remove(ref referenceInfo.assetInfo.referencedAtInfoList, info);
					break;
				}
			}

			foreach (var referencedAtInfo in referencedAtInfoList)
			{
				foreach (var info in referencedAtInfo.assetInfo.referencesInfo)
				{
					if (info.assetInfo != this) continue;
					ArrayUtility.Remove(ref referencedAtInfo.assetInfo.referencesInfo, info);
					referencedAtInfo.assetInfo.needToRebuildReferences = true;
					break;
				}
			}
		}

		public int[] GetAllAssetObjects()
		{
			if (allAssetObjects != null) return allAssetObjects;

			var assetType = Type;
			var assetTypeName = assetType != null ? assetType.Name : null;

			if ((assetType == CSReflectionTools.fontType ||
				assetType == CSReflectionTools.texture2DType ||
#if !UNITY_2018_1_OR_NEWER
				assetType == CSReflectionTools.substanceArchiveType ||
#endif
				assetType == CSReflectionTools.gameObjectType ||
				assetType == CSReflectionTools.defaultAssetType && Path.EndsWith(".dll") ||
				assetTypeName == "AudioMixerController" ||
				Path.EndsWith("LightingData.asset")) &&
				assetType != CSReflectionTools.lightingDataAsset)
			{
				var loadedObjects = AssetDatabase.LoadAllAssetsAtPath(Path);
				var referencedObjectsCandidatesList = new List<int>(loadedObjects.Length);
				for (var i = 0; i < loadedObjects.Length; i++)
				{
					var loadedObject = loadedObjects[i];
					if (loadedObject == null) continue;
					var instance = loadedObject.GetInstanceID();
					if (assetType == CSReflectionTools.gameObjectType)
					{
						if (!AssetDatabase.IsSubAsset(instance) && !AssetDatabase.IsMainAsset(instance)) continue;
					}

					referencedObjectsCandidatesList.Add(instance);
				}

				allAssetObjects = referencedObjectsCandidatesList.ToArray();
			}
			else
			{
				var mainAsset = AssetDatabase.LoadMainAssetAtPath(Path);
				if (mainAsset != null)
				{
					allAssetObjects = new[] { AssetDatabase.LoadMainAssetAtPath(Path).GetInstanceID() };
				}
				else
				{
					allAssetObjects = new int[0];
				}
			}

			return allAssetObjects;
		}

		private static void ScanFileForIncludes(List<string> referencePaths, string filePath)
		{
			var fileLines = File.ReadAllLines(filePath);
			foreach (var line in fileLines)
			{
				var includeIndex = line.IndexOf("include", StringComparison.Ordinal);
				if (includeIndex == -1) continue;

				var noSharp = line.IndexOf('#', 0, includeIndex) == -1;
				if (noSharp) continue;

				var indexOfFirstQuote = line.IndexOf('"', includeIndex);
				if (indexOfFirstQuote == -1) continue;

				var indexOfLastQuote = line.IndexOf('"', indexOfFirstQuote + 1);
				if (indexOfLastQuote == -1) continue;

				var path = line.Substring(indexOfFirstQuote + 1, indexOfLastQuote - indexOfFirstQuote - 1);
				path = CSPathTools.EnforceSlashes(path);

				string assetPath;

				if (path.StartsWith("Assets/"))
				{
					assetPath = path;
				}
				else if (path.IndexOf('/') != -1)
				{
					var folder = System.IO.Path.GetDirectoryName(filePath);
					if (folder == null) continue;

					var combinedPath = System.IO.Path.Combine(folder, path);
					var fullPath = CSPathTools.EnforceSlashes(System.IO.Path.GetFullPath(combinedPath));
					var assetsIndex = fullPath.IndexOf("Assets/", StringComparison.Ordinal);
					if (assetsIndex == -1) continue;

					assetPath = fullPath.Substring(assetsIndex, fullPath.Length - assetsIndex);
				}
				else
				{
					var folder = System.IO.Path.GetDirectoryName(filePath);
					if (folder == null) continue;

					assetPath = CSPathTools.EnforceSlashes(System.IO.Path.Combine(folder, path));
				}

				if (!File.Exists(assetPath)) continue;

				if (referencePaths.IndexOf(assetPath) != -1) continue;
				{
					referencePaths.Add(assetPath);
				}
			}
		}

		private void WalkReferencesRecursive(List<AssetInfo> result, ReferenceInfo[] referenceInfos)
		{
			foreach (var referenceInfo in referenceInfos)
			{
				if (result.IndexOf(referenceInfo.assetInfo) == -1)
				{
					result.Add(referenceInfo.assetInfo);
					WalkReferencesRecursive(result, referenceInfo.assetInfo.referencesInfo);
				}
			}
		}

		private void WalkReferencedAtRecursive(List<AssetInfo> result, ReferencedAtInfo[] referencedAtInfos)
		{
			foreach (var referencedAtInfo in referencedAtInfos)
			{
				if (result.IndexOf(referencedAtInfo.assetInfo) == -1)
				{
					result.Add(referencedAtInfo.assetInfo);
					WalkReferencedAtRecursive(result, referencedAtInfo.assetInfo.referencedAtInfoList);
				}
			}
		}

		private static string[] GetAssetsReferencedInPlayerSettingsAsset(string assetPath, AssetSettingsKind settingsKind)
		{
			var referencedAssets = new List<string>();

			if (settingsKind == AssetSettingsKind.EditorBuildSettings)
			{
				referencedAssets.AddRange(CSSceneTools.GetScenesInBuild(true));
			}
			else
			{
				var settingsAsset = AssetDatabase.LoadAllAssetsAtPath(assetPath);
				if (settingsAsset != null && settingsAsset.Length > 0)
				{
					var settingsAssetSerialized = new SerializedObject(settingsAsset[0]);

					var sp = settingsAssetSerialized.GetIterator();
					while (sp.Next(true))
					{
						if (sp.propertyType == SerializedPropertyType.ObjectReference)
						{
							var instanceId = sp.objectReferenceInstanceIDValue;
							if (instanceId != 0)
							{
								var path = CSPathTools.EnforceSlashes(AssetDatabase.GetAssetPath(instanceId));
								if (!string.IsNullOrEmpty(path) && path.StartsWith("Assets"))
								{
									if (referencedAssets.IndexOf(path) == -1)
										referencedAssets.Add(path);
								}
							}
						}
					}
				}
			}

			return referencedAssets.ToArray();
		}

		private void ActualizePath()
		{
			if (Kind == AssetKind.FromPackage) return;

			var actualPath = CSPathTools.EnforceSlashes(AssetDatabase.GUIDToAssetPath(GUID));
			if (!string.IsNullOrEmpty(actualPath) && actualPath != Path)
			{
				fileInfo = new FileInfo(actualPath);
				metaFileInfo = new FileInfo(actualPath + ".meta");
				Path = actualPath;
			}
		}

		private List<string> GetGetAssetsInFoldersReferencedFromSpriteAtlas(string assetPath)
		{
			var result = new List<string>();

			var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.U2D.SpriteAtlas>(assetPath);
			var so = new SerializedObject(asset);

			// source: SpriteAtlasInspector
			var packablesProperty = so.FindProperty("m_EditorData.packables");
			if (packablesProperty == null || !packablesProperty.isArray)
			{
				Debug.LogError(Maintainer.LogPrefix + "Can't parse UnityEngine.U2D.SpriteAtlas, please report to " + Maintainer.SupportEmail);
			}
			else
			{
				var count = packablesProperty.arraySize;
				for (var i = 0; i < count; i++)
				{
					var packable = packablesProperty.GetArrayElementAtIndex(i);
					var objectReferenceValue = packable.objectReferenceValue;
					if (objectReferenceValue != null)
					{
						var path = AssetDatabase.GetAssetOrScenePath(objectReferenceValue);
						if (AssetDatabase.IsValidFolder(path))
						{
							var packablePaths = CSPathTools.GetAllPackableAssetsPathsRecursive(path);
							result.AddRange(packablePaths);
						}
					}
				}
			}

			return result;
		}

		private List<string> GetAssetsReferencedFromAssemblyDefinition(string assetPath)
		{
			var result = new List<string>();

			var asset = AssetDatabase.LoadAssetAtPath<UnityEditorInternal.AssemblyDefinitionAsset>(assetPath);
			var data = JsonUtility.FromJson<AssemblyDefinitionData>(asset.text);

			if (data.references != null && data.references.Length > 0)
			{
				foreach (var reference in data.references)
				{
#if !UNITY_2019_1_OR_NEWER
					var assemblyDefinitionFilePathFromAssemblyName = CompilationPipeline.GetAssemblyDefinitionFilePathFromAssemblyName(reference);
#else
					var assemblyDefinitionFilePathFromAssemblyName = CompilationPipeline.GetAssemblyDefinitionFilePathFromAssemblyReference(reference);
#endif
					if (!string.IsNullOrEmpty(assemblyDefinitionFilePathFromAssemblyName))
					{
						assemblyDefinitionFilePathFromAssemblyName = CSPathTools.EnforceSlashes(assemblyDefinitionFilePathFromAssemblyName);
						result.Add(assemblyDefinitionFilePathFromAssemblyName);
					}
				}
			}

			data.references = null;

			return result;
		}
		
#if UNITY_2019_2_OR_NEWER
		private IEnumerable<string> GetAssetsReferencedFromAssemblyDefinitionReference(string assetPath)
		{
			var result = new List<string>();
			
			var asset = AssetDatabase.LoadAssetAtPath<UnityEditorInternal.AssemblyDefinitionReferenceAsset>(assetPath);
			var data = JsonUtility.FromJson<AssemblyDefinitionReferenceData>(asset.text);

			if (!string.IsNullOrEmpty(data.reference))
			{
				var assemblyDefinitionPath = CompilationPipeline.GetAssemblyDefinitionFilePathFromAssemblyReference(data.reference);
				if (!string.IsNullOrEmpty(assemblyDefinitionPath))
				{ 
					assemblyDefinitionPath = CSPathTools.EnforceSlashes(assemblyDefinitionPath);
					result.Add(assemblyDefinitionPath);
				}
			}

			data.reference = null;

			return result;
		}
		
		private class AssemblyDefinitionReferenceData
		{
			public string reference;
		}
#endif

		private class AssemblyDefinitionData
		{
			public string[] references;
		}
	}
}
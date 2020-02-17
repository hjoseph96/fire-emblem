#region copyright
// -------------------------------------------------------------------
//  Copyright (C) 2019 Dmitriy Yukhanov - focus [http://codestage.net]
// -------------------------------------------------------------------
#endregion

#if UNITY_2018_2_OR_NEWER

namespace CodeStage.Maintainer.Core
{
	using Boo.Lang;
	using UnityEditor;
	using UnityEngine;

	public static class AddressablesReferenceFinder
	{
		public static List<string> Extract(string path)
		{
			var assetGroup = AssetDatabase.LoadMainAssetAtPath(path);

			if (assetGroup == null)
			{
				return null;
			}

			return ExtractReferencedAssets(assetGroup);
		}

		private static List<string> ExtractReferencedAssets(Object assetGroup)
		{
			var so = new SerializedObject(assetGroup);

			var serializedEntries = so.FindProperty("m_SerializeEntries");
			if (serializedEntries == null)
			{
				// legacy package version used this name
				serializedEntries = so.FindProperty("m_serializeEntries");

				if (serializedEntries == null)
				{
					Debug.LogError(Maintainer.ConstructError("Can't reach serialize entries in AddressableAssetGroup!"));
					return null;
				}
			}

			if (!serializedEntries.isArray)
			{
				Debug.LogError(Maintainer.ConstructError("Can't find serialize entries array in AddressableAssetGroup!"));
				return null;
			}

			var result = new List<string>();

			var count = serializedEntries.arraySize;
			for (var i = 0; i < count; i++)
			{
				var item = serializedEntries.GetArrayElementAtIndex(i);
				if (item == null)
				{
					Debug.LogWarning(Maintainer.ConstructWarning("Serialize entry from AddressableAssetGroup is null!"));
					continue;
				}

				var referencedGUID = item.FindPropertyRelative("m_GUID");
				if (referencedGUID == null || referencedGUID.propertyType != SerializedPropertyType.String)
				{
					Debug.LogError(Maintainer.ConstructError("Can't reach Serialize entry GUID of AddressableAssetGroup!"));
					return null;
				}

				var path = AssetDatabase.GUIDToAssetPath(referencedGUID.stringValue);
				if (!path.StartsWith("Assets"))
				{
					continue;
				}

				result.Add(path);
			}

			return result;
		}
	}
}

#endif
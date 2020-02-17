/*
 * Authors:  Jesse Graupmann & Tim Graupmann
 * Just Good Design, @copyright 2012-2013  All rights reserved.
 *
*/

using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public static partial class GoodEditorPanel
{
    #region CALLBACK EXTENSIONS
    public static T TryInvoke<T>(this Func<T, T> action, T content)
    {
        if (action != null)
        {
            return action.Invoke(content);
        }
        return content;
    }

    public static Action<GoodProcessThread> TryInvoke(this Action<GoodProcessThread> action, GoodProcessThread worker)
    {
        if (action != null)
        {
            action.Invoke(worker);
        }
        return action;
    }

    public static void TryInvoke<T, T2, T3>(this Action<T, T2, T3> action, T arg, T2 arg2, T3 arg3)
    {
        if (action != null) action.Invoke(arg, arg2, arg3);
    }

    public static void TryInvoke<T, T2>(this Action<T, T2> action, T arg, T2 arg2)
    {
        if (action != null) action.Invoke(arg, arg2);
    }

    public static void TryInvoke<T>(this Action<T> action, T arg)
    {
        if (action != null) action.Invoke(arg);
    }

    public static void TryInvoke(this Action action)
    {
        if (action != null) action.Invoke();
    }
    #endregion

    #region GUI - MESSAGES
    public static void HelpBox(string message)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Space(15);
        GUILayout.Label(message, EditorStyles.wordWrappedLabel);
        GUILayout.Space(15);
        GUILayout.EndHorizontal();
    }
    #endregion

    #region GUI - CONTEXT
    public static void AddValidatedMenuItem(GenericMenu menu, string title, bool isChecked, bool useCallback, GenericMenu.MenuFunction2 callback, object arg)
    {
        if (useCallback)
        {
            menu.AddItem(new GUIContent(title), isChecked, callback, arg);

        }
        else
        {
            menu.AddItem(new GUIContent(title), isChecked, null);
        }
    }

    public static void AddValidatedMenuItem(GenericMenu menu, string title, bool isChecked, bool useCallback, GenericMenu.MenuFunction callback)
    {
        if (useCallback)
        {
            menu.AddItem(new GUIContent(title), isChecked, callback);

        }
        else
        {
            menu.AddItem(new GUIContent(title), isChecked, null);
        }
    }
    #endregion

    #region GUI - UTILITIES
    public static float IndentedContentSize = 15;
    public static float IndentedContentPadding = 5;
    public static float IndentedContentMaxOffset = 0;
    public static GUILayoutOption MaxLabelWidth = GUILayout.Width(70);
    public static GUILayoutOption MaxButtonWidth = GUILayout.MaxWidth(180);

    public static void BeginIndentedContent(EditorWindow window)
    {
        BeginIndentedContent(window, IndentedContentSize);
    }

    public static void BeginIndentedContent(EditorWindow window, float space)
    {
        GUILayoutOption maxWindowWidth = GUILayout.MaxWidth(window.position.width - IndentedContentPadding + IndentedContentMaxOffset);
        GUILayout.BeginHorizontal(maxWindowWidth);
        GUILayout.Space(IndentedContentSize);
        GUILayout.BeginVertical();
    }

    public static void EndIndentedContent()
    {
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
    }

    public static void MakeGUILabel(EditorWindow window, string label, Action buildCustomContent)
    {
        GUILayoutOption maxWindowWidth =
            GUILayout.MaxWidth(window.position.width - IndentedContentPadding - IndentedContentSize + IndentedContentMaxOffset);

        GUILayout.BeginHorizontal(maxWindowWidth);
        GUILayout.Label(label, EditorStyles.wordWrappedMiniLabel, MaxLabelWidth);
        GUILayout.Space(IndentedContentPadding);
        buildCustomContent.TryInvoke();
        GUILayout.EndHorizontal();
    }

    public static string MakeGUILabel(EditorWindow window, string label, string content)
    {
        return MakeGUILabel(window, label, content, false);
    }

    public static string MakeGUILabel(EditorWindow window, string label, string content, bool isEditable)
    {
        GUILayoutOption maxWindowWidth =
            GUILayout.MaxWidth(window.position.width - IndentedContentPadding - IndentedContentSize + IndentedContentMaxOffset);

        GUILayout.BeginHorizontal(maxWindowWidth);
        GUILayout.Label(label, EditorStyles.wordWrappedMiniLabel, MaxLabelWidth);
        GUILayout.Space(IndentedContentPadding);

        if (isEditable)
        {
            content = EditorGUILayout.TextField(content, MaxLabelWidth, maxWindowWidth);
        }
        else
        {
            GUILayout.Label(content, EditorStyles.wordWrappedLabel, maxWindowWidth);

        }
        GUILayout.EndHorizontal();
        return content;
    }



    public static string MakeGUITextArea(EditorWindow window, string label, string content)
    {
        GUILayoutOption maxWindowWidth =
            GUILayout.MaxWidth(window.position.width - IndentedContentPadding - IndentedContentSize + IndentedContentMaxOffset);

        GUILayout.BeginHorizontal(maxWindowWidth);
        GUILayout.Label(label, EditorStyles.wordWrappedMiniLabel, MaxLabelWidth);
        GUILayout.Space(IndentedContentPadding);
        content = EditorGUILayout.TextArea(content, MaxLabelWidth, maxWindowWidth);
        GUILayout.EndHorizontal();
        return content;
    }

    public static void MakeGUIHeaderLabel(string label)
    {
        GUILayout.Label(label, EditorStyles.boldLabel);
    }

    public static bool MakeRightButton(string content)
    {
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        var isTrue = GUILayout.Button(new GUIContent(content, ""), MaxButtonWidth);
        GUILayout.Space(IndentedContentSize);
        GUILayout.EndHorizontal();
        return isTrue;
    }

    public static bool MakeButton(string content)
    {
        return GUILayout.Button(new GUIContent(content, ""), MaxButtonWidth);
    }

    public static bool MakeButton(string content, string tooltip)
    {
        return GUILayout.Button(new GUIContent(content, tooltip), MaxButtonWidth);
    }

    public static void GUIDisplayFolder(int width, string label, string path)
    {
        bool dirExists = Directory.Exists(path);

        if (!dirExists)
        {
            GUI.enabled = false;
        }
        GUILayout.BeginHorizontal(GUILayout.MaxWidth(width));
        GUILayout.Space(25);
        GUILayout.Label(string.Format("{0}:", label), GUILayout.Width(100));
        GUILayout.Space(5);
        GUILayout.Label(path.Replace("/", @"\"), EditorStyles.wordWrappedLabel, GUILayout.MaxWidth(width - 130));
        GUILayout.EndHorizontal();
        if (!dirExists)
        {
            GUI.enabled = true;
        }
    }

    public static void GUIDisplayFile(int width, string label, string path)
    {
        bool fileExists = File.Exists(path);

        if (!fileExists)
        {
            GUI.enabled = false;
        }
        GUILayout.BeginHorizontal(GUILayout.MaxWidth(width));
        GUILayout.Space(25);
        GUILayout.Label(string.Format("{0}:", label), GUILayout.Width(100));
        GUILayout.Space(5);
        GUILayout.Label(path.Replace("/", @"\"), EditorStyles.wordWrappedLabel, GUILayout.MaxWidth(width - 130));
        GUILayout.EndHorizontal();
        if (!fileExists)
        {
            GUI.enabled = true;
        }
    }

    public static void GUIDisplayUnityFile(int width, string label, string path)
    {
        bool fileExists = File.Exists(path);

        if (!fileExists)
        {
            GUI.enabled = false;
        }
        GUILayout.BeginHorizontal(GUILayout.MaxWidth(width));
        GUILayout.Space(25);
        GUILayout.Label(string.Format("{0}:", label), GUILayout.Width(100));
        GUILayout.Space(5);
        if (string.IsNullOrEmpty(path))
        {
#pragma warning disable 0618
#if UNITY_3_3
            EditorGUILayout.ObjectField(string.Empty, null, typeof(UnityEngine.Object));
#elif UNITY_3_5
            EditorGUILayout.ObjectField(string.Empty, null, typeof(UnityEngine.Object));
#else 
            EditorGUILayout.ObjectField(string.Empty, null, typeof(UnityEngine.Object));
#endif
#pragma warning restore 0618
        }
        else
        {
            try
            {
                DirectoryInfo assets = new DirectoryInfo("Assets");
                Uri assetsUri = new Uri(assets.FullName);
                FileInfo fi = new FileInfo(path);
                string relativePath = assetsUri.MakeRelativeUri(new Uri(fi.FullName)).ToString();
                UnityEngine.Object fileRef = AssetDatabase.LoadAssetAtPath(relativePath, typeof(UnityEngine.Object));
#pragma warning disable 0618
#if UNITY_3_3
                EditorGUILayout.ObjectField(string.Empty, fileRef, typeof(UnityEngine.Object));
#elif UNITY_3_5
                EditorGUILayout.ObjectField(string.Empty, fileRef, typeof(UnityEngine.Object));
#else
                EditorGUILayout.ObjectField(string.Empty, fileRef, typeof(UnityEngine.Object));
#endif
#pragma warning restore 0618
            }
            catch (System.Exception)
            {
                Debug.LogError(string.Format("Path is invalid: label={0} path={1}", label, path));
            }
        }
        GUILayout.EndHorizontal();
        if (!fileExists)
        {
            GUI.enabled = true;
        }
    }
    #endregion

    #region GUI - Selection

    public static bool HasGameObjectSelected()
    {
        if (null == Selection.activeGameObject)
        {
            return false;
        }

        string path;
        foreach (GameObject go in Selection.gameObjects)
        {
            path = AssetDatabase.GetAssetPath(go);
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }
        }

        path = AssetDatabase.GetAssetPath(Selection.activeGameObject);
        return string.IsNullOrEmpty(path);
    }

    public static bool HasObjectSelected()
    {
        if (null == Selection.activeObject)
        {
            return false;
        }

        string path;
        foreach (UnityEngine.Object obj in Selection.objects)
        {
            path = AssetDatabase.GetAssetPath(obj);
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }
        }

        path = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (string.IsNullOrEmpty(path))
        {
            return false;
        }

        return true;
    }

    #endregion

    #region File Search

    public static string FindAssetPath(string searchFile, string excludeSourceFolder)
    {
        return FindAssetPath(searchFile, new DirectoryInfo(Directory.GetCurrentDirectory()), excludeSourceFolder);
    }

    private static string FindAssetPath(string searchFile, DirectoryInfo directory, string excludeSourceFolder)
    {
        if (null == directory)
        {
            return string.Empty;
        }
        foreach (FileInfo file in directory.GetFiles())
        {
            if (string.IsNullOrEmpty(file.FullName) ||
                !searchFile.ToLower().Equals(file.Name.ToLower()))
            {
                continue;
            }
            return file.FullName;
        }
        foreach (DirectoryInfo subDir in directory.GetDirectories())
        {
            if (null == subDir)
            {
                continue;
            }
            if (subDir.Name.ToUpper().Equals(excludeSourceFolder.ToUpper())) // ".GIT" and ".SVN"
            {
                continue;
            }
            string path = FindAssetPath(searchFile, subDir, excludeSourceFolder);
            if (!string.IsNullOrEmpty(path))
            {
                return path;
            }
        }

        return string.Empty;
    }

    #endregion
}
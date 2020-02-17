/*
 * Authors:  Jesse Graupmann & Tim Graupmann
 * Just Good Design, @copyright 2012-2013  All rights reserved.
 *
*/

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using System;
using System.Collections.Generic;

public class GoodSVNPanel : EditorWindow
{
    // When you drag and drop a file onto the editor window, this command will be called for the files
    public enum DragDropCommand
    {
        None,
        Add,
        Update,
        Commit,
        Revert,
        SetSVNPath
    }

    #region PROPERTIES
    private static GoodSVNPanel window;
    private string[] fileStrings = new string[] { "Diff", "Rename", "Edit Conflicts" };
    private string[] filesStrings = new string[] { "Update", "Commit", "Add", "Remove", "Lock", "UnLock" };
    private string[] assetStrings = new string[] { "Update", "Commit", "Add" };
    private string[] projStrings = new string[] { "Update", "Commit", "Add" };
    private string[] commandStrings = new string[] { "Revert", "Status", "Log", "Browser", "Checkout", "CleanUp" };

    private static Vector2 scrollRectPrefs;
    private static Vector2 scrollRect;

    private static GoodSVN.DialogCloseSettings s_assetsOptions = GoodSVN.DialogCloseSettings.DontCloseAutomatically;
    private static GoodSVN.DialogCloseSettings s_projOptions = GoodSVN.DialogCloseSettings.DontCloseAutomatically;
    private static GoodSVN.DialogCloseSettings s_fileOptions = GoodSVN.DialogCloseSettings.DontCloseAutomatically;
    private static GoodSVN.DialogCloseSettings s_commandOptions = GoodSVN.DialogCloseSettings.DontCloseAutomatically;

    private static DragDropCommand s_fileDragDropCommand = DragDropCommand.Add;

    private static string s_lastExternalPath = Application.dataPath;

    private static bool s_fileIsOpen = true;
    private static bool s_assetsIsOpen = true;
    private static bool s_projIsOpen = true;
    private static bool s_commandsIsOpen = true;
    private static bool s_filesSelectedIsOpen = true;
    private static bool s_showCommandsAsButtons = true;

    private static Color s_colorFiles = Color.white;
    private static Color s_colorAssets = Color.white;
    private static Color s_colorProject = Color.white;
    private static Color s_colorCommands = Color.white;

    static public GUILayoutOption _labelWidth = GUILayout.Width(100);

    private static bool prefsLoaded;

    private static Texture goodTexture;
    #endregion

    #region INITIALIZE
    [MenuItem("Window/Just Good Design/Good SVN", priority = 200)]
    [MenuItem("Assets/Good SVN/Open Good SVN", priority = 5000)]
    static public void MenuOpenPanel()
    {
        window = (GoodSVNPanel)GetWindow(typeof(GoodSVNPanel), false, "Good SVN");
        window.Show(true);
        window.Focus();
    }
    #endregion

    #region INITIALIZE - MENU ITEMS - LINKS
    [MenuItem("Window/Just Good Design/Links/Good SVN Website", priority = 1000)]
    static public void ExternalLinkGoodSVN()
    {
        Application.OpenURL(GoodSVN.URL);
    }

    [MenuItem("Window/Just Good Design/Links/Tools/TortoiseSVN Website", priority = 1000)]
    static public void ExternalLinkTortoiseSVN()
    {
        Application.OpenURL("http://tortoisesvn.net/");
    }

    [MenuItem("Window/Just Good Design/Links/Tools/CrossOver Website", priority = 1000)]
    static public void ExternalLinkCrossOver()
    {
        Application.OpenURL("http://www.codeweavers.com/products/");
    }

    [MenuItem("Window/Just Good Design/Links/Tools/Putty Website", priority = 1000)]
    static public void ExternalLinkPutty()
    {
        Application.OpenURL("http://www.chiark.greenend.org.uk/~sgtatham/putty/download.html");
    }

    [MenuItem("Window/Just Good Design/Links/Tools/Winmerge Website", priority = 1000)]
    static public void ExternalLinkWinMerge()
    {
        Application.OpenURL("http://winmerge.org/");
    }
    #endregion

    #region PREFERENCES
    [PreferenceItem("G|SVN")]
    public static void PreferencesGUI()
    {
        if (!prefsLoaded)
        {
            LoadPrefs();
        }
        scrollRectPrefs = GUILayout.BeginScrollView(scrollRectPrefs);

        if (null != goodTexture)
        {
            GUIContent logoBtn = new GUIContent(goodTexture);
            Rect logoRect = GUILayoutUtility.GetRect(logoBtn, EditorStyles.label, GUILayout.ExpandWidth(false));
            if (null != logoBtn)
            {
                if (GUI.Button(logoRect, logoBtn, EditorStyles.label))
                {
                    Application.OpenURL(GoodSVN.URL);
                }
            }
        }

        EditorGUILayout.LabelField("Product", GoodSVN.PRODUCT);
        EditorGUILayout.LabelField("Version", GoodSVN.VERSION);
        EditorGUILayout.LabelField("Company", GoodSVN.COMPANY);
        EditorGUILayout.LabelField("Developers", "Jesse Graupmann");
        EditorGUILayout.LabelField(" ", "Tim Graupmann");
        EditorGUILayout.Space();
        GUILayout.Label("SVN Type", EditorStyles.boldLabel);

        int svnGridInt;

        switch (Application.platform)
        {
            case RuntimePlatform.OSXEditor:
                EditorGUILayout.BeginHorizontal();
                GoodSVN.SVNType = (GoodSVN.SVNTypes)EditorGUILayout.EnumPopup(GoodSVN.SVNType);
                EditorGUILayout.EndHorizontal();

                if (GoodSVN.SVNType == GoodSVN.SVNTypes.TortoiseSVN)
                {
                    GoodSVN.UseCrossover = EditorGUILayout.Toggle("Use with CrossOver", GoodSVN.UseCrossover);
                    GUI.enabled = false;
                    EditorGUILayout.Toggle("Use with VMWare", false);
                    GUI.enabled = true;

                    EditorGUILayout.Space();
                    // Preferences GUI   
                    if (GoodSVN.UseCrossover)
                    {
                        string oldBottleName = GoodSVN.GetCrossOverBottle();
                        string bottleName = EditorGUILayout.TextField("Bottle Name:", oldBottleName);
                        if (!oldBottleName.Equals(bottleName))
                        {
                            GoodSVN.SetCrossOverBottle(bottleName);
                        }
                    }
                    else
                    {
                        GUILayout.Label("SVN Path", EditorStyles.boldLabel);
                        GUILayout.Label(GoodSVN.PATH_TORTOISE_SVN);
                        svnGridInt = GUILayout.SelectionGrid(-1, new string[] { "Browse for Path...", "Reset Path" }, 2);
                        switch (svnGridInt)
                        {
                            case 0:
                                BrowseForSVN();
                                break;
                            case 1:
                                GoodSVN.PATH_TORTOISE_SVN = string.Empty;
                                SavePrefs();
                                break;
                        }
                    }
                }
                else
                {
                    EditorGUILayout.Space();
                    GUILayout.Label("Native Mac SVN is not yet supported");
                    EditorGUILayout.Space();
                    GUILayout.EndScrollView();

                    if (GUI.changed)
                        SavePrefs();

                    return;
                }
                break;

            default:
                EditorGUILayout.BeginHorizontal();
                GUI.enabled = false;
                GoodSVN.SVNType = (GoodSVN.SVNTypes)EditorGUILayout.EnumPopup(GoodSVN.SVNType);
                GUI.enabled = true;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();
                GUILayout.Label("SVN Path", EditorStyles.boldLabel);
                GUILayout.Label(GoodSVN.PATH_TORTOISE_SVN);
                svnGridInt = GUILayout.SelectionGrid(-1, new string[] { "Browse for Path...", "Reset Path" }, 2);
                switch (svnGridInt)
                {
                    case 0:
                        BrowseForSVN();
                        break;
                    case 1:
                        GoodSVN.PATH_TORTOISE_SVN = string.Empty;
                        SavePrefs();
                        break;
                }
                break;
        }

         
        EditorGUILayout.Space();
        GUILayout.Label("GUI", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Show Buttons");
        s_showCommandsAsButtons = EditorGUILayout.Toggle(s_showCommandsAsButtons);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Drag/Drop Command");
        s_fileDragDropCommand = (DragDropCommand)EditorGUILayout.EnumPopup(s_fileDragDropCommand);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        GUILayout.Label("Assets", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Dialog Closing");
        s_assetsOptions = (GoodSVN.DialogCloseSettings)EditorGUILayout.EnumPopup(s_assetsOptions);
        EditorGUILayout.EndHorizontal();
        s_colorAssets = EditorGUILayout.ColorField("Button Background Color", s_colorAssets);

        EditorGUILayout.Space();
        GUILayout.Label("Project Settings", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Dialog Closing");
        s_projOptions = (GoodSVN.DialogCloseSettings)EditorGUILayout.EnumPopup(s_projOptions);
        EditorGUILayout.EndHorizontal();
        s_colorProject = EditorGUILayout.ColorField("Button Background Color", s_colorProject);

        GUILayout.Label("Files", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Show Selected Files");
        s_filesSelectedIsOpen = EditorGUILayout.Toggle(s_filesSelectedIsOpen);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Dialog Closing");
        s_fileOptions = (GoodSVN.DialogCloseSettings)EditorGUILayout.EnumPopup(s_fileOptions);
        EditorGUILayout.EndHorizontal();
        s_colorFiles = EditorGUILayout.ColorField("Button Background Color", s_colorFiles);

        EditorGUILayout.Space();
        GUILayout.Label("Commands", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Dialog Closing");
        s_commandOptions = (GoodSVN.DialogCloseSettings)EditorGUILayout.EnumPopup(s_commandOptions);
        EditorGUILayout.EndHorizontal();
        s_colorCommands = EditorGUILayout.ColorField("Button Background Color", s_colorCommands);
         
        EditorGUILayout.Space();
        GUILayout.EndScrollView();

        if (GUI.changed)
            SavePrefs();
    }

    private static void LoadPrefs()
    {
        goodTexture = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/JustGood/Textures/JustGoodDesign.png", typeof(Texture2D));
        s_assetsOptions = (GoodSVN.DialogCloseSettings)GoodEditorPrefs.GetInt("GoodSVN_s_assetsOptions", (int)s_assetsOptions);
        s_fileOptions = (GoodSVN.DialogCloseSettings)GoodEditorPrefs.GetInt("GoodSVN_s_fileOptions", (int)s_fileOptions);
        s_commandOptions = (GoodSVN.DialogCloseSettings)GoodEditorPrefs.GetInt("GoodSVN_s_commandOptions", (int)s_commandOptions);
        s_projOptions = (GoodSVN.DialogCloseSettings)GoodEditorPrefs.GetInt("GoodSVN_s_projOptions", (int)s_projOptions);
        s_fileDragDropCommand = (DragDropCommand)GoodEditorPrefs.GetInt("GoodSVN_s_fileDragDropCommand", (int)s_fileDragDropCommand);
        GoodSVN.SVNType = (GoodSVN.SVNTypes)GoodEditorPrefs.GetInt("GoodSVN_GoodSVN.SVNType", (int)GoodSVN.SVNType);

        s_lastExternalPath = GoodEditorPrefs.GetString("GoodSVN_m_lastExternalPath", s_lastExternalPath);
        GoodSVN.PATH_TORTOISE_SVN = GoodEditorPrefs.GetString("GoodSVN_path_tortoise_svn", GoodSVN.PATH_TORTOISE_SVN);
        GoodSVN.UseCrossover = GoodEditorPrefs.GetBool("GoodSVN_UseCrossover", GoodSVN.UseCrossover);

        s_assetsIsOpen = GoodEditorPrefs.GetBool("GoodSVN_s_assetsIsOpen", s_assetsIsOpen);
        s_projIsOpen = GoodEditorPrefs.GetBool("GoodSVN_s_projIsOpen", s_projIsOpen);
        s_fileIsOpen = GoodEditorPrefs.GetBool("GoodSVN_s_fileIsOpen", s_fileIsOpen);
        s_commandsIsOpen = GoodEditorPrefs.GetBool("GoodSVN_s_commandsIsOpen", s_commandsIsOpen);
        s_filesSelectedIsOpen = GoodEditorPrefs.GetBool("GoodSVN_s_filesSelectedIsOpen", s_filesSelectedIsOpen);
        s_showCommandsAsButtons = GoodEditorPrefs.GetBool("GoodSVN_s_showCommandsAsButtons", s_showCommandsAsButtons);

        s_colorFiles = GoodEditorPrefs.GetColor("GoodSVN_s_colorFiles", s_colorFiles);
        s_colorAssets = GoodEditorPrefs.GetColor("GoodSVN_s_colorAssets", s_colorAssets);
        s_colorProject = GoodEditorPrefs.GetColor("GoodSVN_s_colorProject", s_colorProject);
        s_colorCommands = GoodEditorPrefs.GetColor("GoodSVN_s_colorCommands", s_colorCommands);

        prefsLoaded = true;
    }

    private static void SavePrefs()
    {
        GoodEditorPrefs.SetInt("GoodSVN_s_assetsOptions", (int)s_assetsOptions);
        GoodEditorPrefs.SetInt("GoodSVN_s_projOptions", (int)s_projOptions);
        GoodEditorPrefs.SetInt("GoodSVN_s_fileOptions", (int)s_fileOptions);
        GoodEditorPrefs.SetInt("GoodSVN_s_commandOptions", (int)s_commandOptions);

        GoodEditorPrefs.SetInt("GoodSVN_s_fileDragDropCommand", (int)s_fileDragDropCommand);

        GoodEditorPrefs.SetBool("GoodSVN_s_assetsIsOpen", s_assetsIsOpen);
        GoodEditorPrefs.SetBool("GoodSVN_s_projIsOpen", s_projIsOpen);
        GoodEditorPrefs.SetBool("GoodSVN_s_fileIsOpen", s_fileIsOpen);
        GoodEditorPrefs.SetBool("GoodSVN_s_commandsIsOpen", s_commandsIsOpen);
        GoodEditorPrefs.SetBool("GoodSVN_s_filesSelectedIsOpen", s_filesSelectedIsOpen);
        GoodEditorPrefs.SetBool("GoodSVN_s_showCommandsAsButtons", s_showCommandsAsButtons);

        GoodEditorPrefs.SetColor("GoodSVN_s_colorFiles", s_colorFiles);
        GoodEditorPrefs.SetColor("GoodSVN_s_colorAssets", s_colorAssets);
        GoodEditorPrefs.SetColor("GoodSVN_s_colorProject", s_colorProject);
        GoodEditorPrefs.SetColor("GoodSVN_s_colorCommands", s_colorCommands);

        GoodEditorPrefs.SetString("GoodSVN_m_lastExternalPath", s_lastExternalPath);
        GoodEditorPrefs.SetString("GoodSVN_path_tortoise_svn", GoodSVN.PATH_TORTOISE_SVN);
        GoodEditorPrefs.SetBool("GoodSVN_UseCrossover", GoodSVN.UseCrossover);
        GoodEditorPrefs.SetInt("GoodSVN_GoodSVN.SVNType", (int)GoodSVN.SVNType);
    }
    #endregion

    #region ENABLE / DISABLE
    private void OnEnable()
    {
        minSize = new Vector2(103, 18);
        if (!prefsLoaded)
        {
            LoadPrefs();
        }
    }
    #endregion

    #region UPDATES
    private void OnInspectorUpdate()
    {
        Repaint();
    }
    #endregion

#if EVAL_JUSTGOOD
    private const string KEY_EVAL_JG = "JG_Eval";
    private static string EvaluationKey
    {
        get
        {
            if (EditorPrefs.HasKey(KEY_EVAL_JG))
            {
                return EditorPrefs.GetString(KEY_EVAL_JG);
            }
            else
            {
                return string.Empty;
            }
        }
        set
        {
            EditorPrefs.SetString(KEY_EVAL_JG, value);
        }
    }

    public static bool CheckLicense()
    {
        int index;
        bool isKeyGood;
        bool isExpired;
        DateTime evalStart;
        DateTime evalEnd;
        JustGoodEvaluation.EvalValidate(GoodSVNPanel.EvaluationKey, out isKeyGood, out isExpired, out index,
                                         out evalStart,
                                         out evalEnd);
        if (!isKeyGood ||
            isExpired)
        {
            GoodSVNPanel.MenuOpenPanel();
            return false;
        }
        else
        {
            return true;
        }
    }
#endif

    #region GUI
    private void OnGUI()
    {
#if EVAL_JUSTGOOD
        int index;
        bool isKeyGood;
        bool isExpired;
        DateTime evalStart;
        DateTime evalEnd;
        JustGoodEvaluation.EvalValidate(EvaluationKey, out isKeyGood, out isExpired, out index, out evalStart,
                                        out evalEnd);
        if (isKeyGood)
        {
            if (isExpired)
            {
                GUILayout.Label(string.Format("Evaluation expired: {0} to {1}", evalStart, evalEnd));
                EvaluationKey = EditorGUILayout.TextField("Evaluation Key:", EvaluationKey);
                return;
            }
            else
            {
                GUILayout.Label(string.Format("Evaluation valid: {0} to {1}", evalStart, evalEnd));
                EvaluationKey = EditorGUILayout.TextField("Evaluation Key:", EvaluationKey);
            }
        }
        else
        {
            GUILayout.Label("[ERROR] Evaluation key is not valid");
            EvaluationKey = EditorGUILayout.TextField("Evaluation Key:", EvaluationKey);
            return;
        }
#endif

        var droppedPaths = GetDroppedPaths();

        // toolbar
        OnGUIToolbar();

        if (GoodSVN.CantSync)
        {
            GUILayout.Space(10);
            GoodEditorPanel.HelpBox("Can't Sync without TortoiseSVN.\n\nPlease Select a valid path.");
            if (GoodEditorPanel.MakeRightButton("Select Path..."))
            {
                BrowseForSVN();
            }
            DoDropCheckForSVNPath(droppedPaths);
        }
        else
        {
            scrollRect = GUILayout.BeginScrollView(scrollRect);

            bool hasSelection = Selection.activeObject != null;

            var count = 0;
            string listStr = string.Empty;
            if (hasSelection)
            {
                var list = GetSelectedList();
                count = list.Length;
                listStr = string.Join("\n", list);
            }

            if (s_showCommandsAsButtons)
            { 
                // assets 
                s_assetsIsOpen = EditorGUILayout.Foldout(s_assetsIsOpen, "Assets");
                int assetInx = -1;
                if (s_assetsIsOpen)
                {
                    GUI.backgroundColor = s_colorAssets;
                    assetInx = GUILayout.SelectionGrid(assetInx, assetStrings, 3);
                    GUI.backgroundColor = Color.white;
                }
                GUILayout.Space(5);

                // project settings 
                s_projIsOpen = EditorGUILayout.Foldout(s_projIsOpen, "Project Settings");
                int projInx = -1;
                if (s_projIsOpen)
                {
                    GUI.backgroundColor = s_colorProject;
                    projInx = GUILayout.SelectionGrid(-1, projStrings, 3);
                    GUI.backgroundColor = Color.white;
                }
                GUILayout.Space(5);

                // files 
                s_fileIsOpen = EditorGUILayout.Foldout(s_fileIsOpen, string.Format("Files [{0}]", count));
                GUI.enabled = hasSelection;


                int fileInx = -1;
                int filesInx = -1;
                if (s_fileIsOpen)
                {
                    GUI.backgroundColor = s_colorFiles;
                    GUI.enabled = !GoodEditorPanel.HasGameObjectSelected() &&
                        GoodEditorPanel.HasObjectSelected();
                    filesInx = GUILayout.SelectionGrid(-1, filesStrings, 3);
                    GUI.enabled = hasSelection &&
                        Selection.objects.Length == 1 &&
                        !GoodEditorPanel.HasGameObjectSelected() &&
                        GoodEditorPanel.HasObjectSelected();
                    fileInx = GUILayout.SelectionGrid(-1, fileStrings, 3);
                    GUI.enabled = hasSelection;
                    GUI.backgroundColor = Color.white;
                }

                if (!GUI.enabled) GUI.enabled = true;
                GUILayout.Space(5);


                // commands
                s_commandsIsOpen = EditorGUILayout.Foldout(s_commandsIsOpen, "Commands");
                int commandInx = -1;
                if (s_commandsIsOpen)
                {
                    GUI.backgroundColor = s_colorCommands;
                    GUI.enabled = !GoodEditorPanel.HasGameObjectSelected();
                    commandInx = GUILayout.SelectionGrid(commandInx, commandStrings, 3);
                    GUI.enabled = true;
                    GUI.backgroundColor = Color.white;
                }
                 
                // prefs
                if (GUI.changed)
                {
                    SavePrefs();
                }

                // actions
                if (commandInx >= 0)
                {
                    switch (commandStrings[commandInx])
                    {
                        case "Revert":
                            ShowRevert();
                            break;
                        case "Status":
                            ShowStatus();
                            break;
                        case "Browser":
                            ShowBrowser();
                            break;
                        case "Log":
                            ShowLog();
                            break;
                        case "Checkout":
                            ShowCheckout();
                            break;
                        case "CleanUp":
                            ShowCleanUp();
                            break;
                    }
                }
                if (filesInx >= 0)
                {
                    switch (filesStrings[filesInx])
                    {
                        case "Update":
                            UpdateFile();
                            break;
                        case "Commit":
                            CommitFile();
                            break;
                        case "Add":
                            AddFile();
                            break;
                        case "Remove":
                            RemoveFile();
                            break;
                        case "Lock":
                            LockFile();
                            break;
                        case "UnLock":
                            UnLockFile();
                            break;
                    }
                }

                if (fileInx >= 0)
                {
                    switch (fileStrings[fileInx])
                    {
                        case "Rename":
                            RenameFile();
                            break;
                        case "Diff":
                            DiffFile();
                            break;

                        case "Edit Conflicts":
                            ConflictEditorForFiles();
                            break;
                    }
                }

                if (assetInx >= 0)
                {
                    switch (assetStrings[assetInx])
                    {
                        case "Update":
                            UpdateAssets();
                            break;
                        case "Commit":
                            CommitAssets();
                            break;
                        case "Add":
                            AddAssets();
                            break;
                    }
                }

                if (projInx >= 0)
                {
                    switch (projStrings[projInx])
                    {
                        case "Update":
                            UpdateProjectSettings();
                            break;
                        case "Commit":
                            CommitProjectSettings();
                            break;
                        case "Add":
                            AddProjectSettings();
                            break;
                    }
                }
                
            }
            // selection   
            GUILayout.Space(10);
            if (s_filesSelectedIsOpen)
            { 
                if (hasSelection)
                {
                    GUILayout.Label("Selected Files:");
                    GoodEditorPanel.MakeGUILabel(this, "", listStr, false);
                    GUILayout.Space(5);
                }
            }
            GUILayout.EndScrollView();

            // DRAG / DROP
            if (s_fileDragDropCommand != DragDropCommand.None)
            {
                if (s_fileDragDropCommand == DragDropCommand.SetSVNPath)
                {
                    DoDropCheckForSVNPath(droppedPaths);
                }
                else
                {
                    if (droppedPaths != null && droppedPaths.Length > 0)
                    {
                        string files = string.Join("*", droppedPaths);
                        switch (s_fileDragDropCommand)
                        {
                            case DragDropCommand.Add:
                                GoodSVN.SVNAdd(files, (int)GoodSVN.DialogCloseSettings.DontCloseAutomatically);
                                break;

                            case DragDropCommand.Commit:
                                GoodSVN.SVNCommitFile(files, (int)GoodSVN.DialogCloseSettings.DontCloseAutomatically);
                                break;

                            case DragDropCommand.Update:
                                GoodSVN.SVNUpdate(files, (int)GoodSVN.DialogCloseSettings.DontCloseAutomatically);
                                break;

                            case DragDropCommand.Revert:
                                GoodSVN.ShowRevert(files, (int)GoodSVN.DialogCloseSettings.DontCloseAutomatically);
                                break;
                        }
                    }
                }
            }
        }
    }

#pragma warning disable 0414
    static GUIContent menuFileBtn = new GUIContent("Files");
    static GUIContent menuAssetsBtn = new GUIContent("Assets");
    static GUIContent menuSettingsBtn = new GUIContent("Settings");
    static GUIContent menuSVNBtn = new GUIContent("SVN");
    static GUIContent menuHelpBtn = new GUIContent("Help");

    Rect menuFileRect;
    Rect menuAssetRect;
    Rect menuProjectRect;
    Rect menuSettingsRect;
    Rect menuSVNRect;
    Rect menuHelpRect;
#pragma warning restore 0414

    private void OnGUIToolbar()
    {
        // TOOLBAR
        GUILayout.BeginHorizontal(EditorStyles.toolbar);

        // FILE  
        menuFileRect = GUILayoutUtility.GetRect(menuFileBtn, EditorStyles.toolbarDropDown, GUILayout.ExpandWidth(false));
        if (GUI.Button(menuFileRect, menuFileBtn, EditorStyles.toolbarDropDown))
        {
            GenericMenu menu = new GenericMenu();

            GoodEditorPanel.AddValidatedMenuItem(menu, "Update", false, ValidateUpdateFile(), UpdateFile);
            GoodEditorPanel.AddValidatedMenuItem(menu, "Commit", false, ValidateCommitFile(), CommitFile);
            GoodEditorPanel.AddValidatedMenuItem(menu, "Add", false, ValidateAddFile(), AddFile);
            GoodEditorPanel.AddValidatedMenuItem(menu, "Remove", false, ValidateRemoveFile(), RemoveFile);
            GoodEditorPanel.AddValidatedMenuItem(menu, "Lock", false, ValidateLockFile(), LockFile);
            GoodEditorPanel.AddValidatedMenuItem(menu, "UnLock", false, ValidateUnLockFile(), UnLockFile);
            GoodEditorPanel.AddValidatedMenuItem(menu, "Diff", false, ValidateDiffFile(), DiffFile);
            GoodEditorPanel.AddValidatedMenuItem(menu, "Rename", false, ValidateRenameFile(), RenameFile);
            GoodEditorPanel.AddValidatedMenuItem(menu, "Edit Conflicts", false, ValidateConflictEditorForFiles(), ConflictEditorForFiles);

            menu.DropDown(menuFileRect);
        }

        // ASSETS  
        menuAssetRect = GUILayoutUtility.GetRect(menuAssetsBtn, EditorStyles.toolbarDropDown, GUILayout.ExpandWidth(false));
        if (GUI.Button(menuAssetRect, menuAssetsBtn, EditorStyles.toolbarDropDown))
        {
            GenericMenu menu = new GenericMenu();
            GoodEditorPanel.AddValidatedMenuItem(menu, "Update", false, ValidateUpdateAssets(), UpdateAssets);
            GoodEditorPanel.AddValidatedMenuItem(menu, "Commit", false, ValidateCommitAssets(), CommitAssets);
            GoodEditorPanel.AddValidatedMenuItem(menu, "Add", false, ValidateAddAssets(), AddAssets);
            GoodEditorPanel.AddValidatedMenuItem(menu, "Revert", false, ValidateRevertAssets(), RevertAssets);

            menu.DropDown(menuAssetRect);
        } 

        // COMMANDS 
        menuSVNRect = GUILayoutUtility.GetRect(menuSVNBtn, EditorStyles.toolbarDropDown, GUILayout.ExpandWidth(false));
        if (GUI.Button(menuSVNRect, menuSVNBtn, EditorStyles.toolbarDropDown))
        {
            GenericMenu menu = new GenericMenu();
            
            GoodEditorPanel.AddValidatedMenuItem(menu, "Project/Update", false, ValidateUpdateProjectSettings(), UpdateProjectSettings);
            GoodEditorPanel.AddValidatedMenuItem(menu, "Project/Commit", false, ValidateCommitProjectSettings(), CommitProjectSettings);
            GoodEditorPanel.AddValidatedMenuItem(menu, "Project/Add", false, ValidateAddProjectSettings(), AddProjectSettings);
            GoodEditorPanel.AddValidatedMenuItem(menu, "Project/Revert", false, ValidateRevertProjectSettings(), RevertProjectSettings);

            // external folder
            menu.AddSeparator("");
            GoodEditorPanel.AddValidatedMenuItem(menu, "External Folder/Update", false, !GoodSVN.CantSync, ExternalFolderCommand, "folderUpdate");
            GoodEditorPanel.AddValidatedMenuItem(menu, "External Folder/Commit", false, !GoodSVN.CantSync, ExternalFolderCommand, "folderCommit");
            GoodEditorPanel.AddValidatedMenuItem(menu, "External Folder/Add", false, !GoodSVN.CantSync, ExternalFolderCommand, "folderAdd");
            menu.AddSeparator("External Folder/");
            GoodEditorPanel.AddValidatedMenuItem(menu, "External Folder/Lock", false, !GoodSVN.CantSync, ExternalFolderCommand, "folderLock");
            GoodEditorPanel.AddValidatedMenuItem(menu, "External Folder/UnLock", false, !GoodSVN.CantSync, ExternalFolderCommand, "folderUnLock");

            menu.AddSeparator("External Folder/");
            GoodEditorPanel.AddValidatedMenuItem(menu, "External Folder/Remove", false, !GoodSVN.CantSync, ExternalFolderCommand, "folderRemove");
            GoodEditorPanel.AddValidatedMenuItem(menu, "External Folder/Revert", false, !GoodSVN.CantSync, ExternalFolderCommand, "folderRevert");
            menu.AddSeparator("External Folder/");
            GoodEditorPanel.AddValidatedMenuItem(menu, "External Folder/Checkout", false, !GoodSVN.CantSync, ExternalFolderCommand, "folderCheckout");
            GoodEditorPanel.AddValidatedMenuItem(menu, "External Folder/Cleanup", false, !GoodSVN.CantSync, ExternalFolderCommand, "folderCleanup");
            
            // external file
            GoodEditorPanel.AddValidatedMenuItem(menu, "External File/Update", false, !GoodSVN.CantSync, ExternalFileCommand, "fileUpdate");
            GoodEditorPanel.AddValidatedMenuItem(menu, "External File/Commit", false, !GoodSVN.CantSync, ExternalFileCommand, "fileCommit");
            GoodEditorPanel.AddValidatedMenuItem(menu, "External File/Add", false, !GoodSVN.CantSync, ExternalFileCommand, "fileAdd");
            menu.AddSeparator("External File/");
            GoodEditorPanel.AddValidatedMenuItem(menu, "External File/Lock", false, !GoodSVN.CantSync, ExternalFileCommand, "fileLock");
            GoodEditorPanel.AddValidatedMenuItem(menu, "External File/UnLock", false, !GoodSVN.CantSync, ExternalFileCommand, "fileUnLock");
            menu.AddSeparator("External File/");
            GoodEditorPanel.AddValidatedMenuItem(menu, "External File/Rename", false, !GoodSVN.CantSync, ExternalFileCommand, "fileRename");
            menu.AddSeparator("External File/");
            GoodEditorPanel.AddValidatedMenuItem(menu, "External File/Remove", false, !GoodSVN.CantSync, ExternalFileCommand, "fileRemove");
            GoodEditorPanel.AddValidatedMenuItem(menu, "External File/Revert", false, !GoodSVN.CantSync, ExternalFileCommand, "fileRevert");

            menu.AddSeparator("");
            GoodEditorPanel.AddValidatedMenuItem(menu, "Revert", false, ValidateShowRevert(), ShowRevert);
            GoodEditorPanel.AddValidatedMenuItem(menu, "Status", false, ValidateShowStatus(), ShowStatus);
            GoodEditorPanel.AddValidatedMenuItem(menu, "Log", false, ValidateShowLog(), ShowLog);
            GoodEditorPanel.AddValidatedMenuItem(menu, "Browser", false, ValidateShowBrowser(), ShowBrowser);
            GoodEditorPanel.AddValidatedMenuItem(menu, "Checkout", false, ValidateShowCheckout(), ShowCheckout);
            GoodEditorPanel.AddValidatedMenuItem(menu, "CleanUp", false, ValidateShowCleanUp(), ShowCleanUp);

            menu.DropDown(menuSVNRect);
        }
        GUILayout.FlexibleSpace();
        // Settings 
        menuSettingsRect = GUILayoutUtility.GetRect(menuSettingsBtn, EditorStyles.toolbarDropDown, GUILayout.ExpandWidth(false));
        if (GUI.Button(menuSettingsRect, menuSettingsBtn, EditorStyles.toolbarDropDown))
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent( "Show Selected Files"), s_filesSelectedIsOpen, () => { s_filesSelectedIsOpen = !s_filesSelectedIsOpen; SavePrefs(); });
            menu.AddItem(new GUIContent( "Show Buttons"), s_showCommandsAsButtons, () => { s_showCommandsAsButtons = !s_showCommandsAsButtons; SavePrefs(); });
            menu.DropDown(menuSettingsRect);
        }
        

        // HELP 
        menuHelpRect = GUILayoutUtility.GetRect(menuHelpBtn, EditorStyles.toolbarDropDown, GUILayout.ExpandWidth(false));
        if (GUI.Button(menuHelpRect, menuHelpBtn, EditorStyles.toolbarDropDown))
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("About Good SVN"), false, ExternalLinkGoodSVN);
            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Tools/TortoiseSVN Website"), false, ExternalLinkTortoiseSVN);
            menu.AddItem(new GUIContent("Tools/CrossOver Website"), false, ExternalLinkCrossOver);
            menu.AddItem(new GUIContent("Tools/Putty Website"), false, ExternalLinkPutty);
            menu.AddItem(new GUIContent("Tools/Winmerge Website"), false, ExternalLinkWinMerge);
            menu.DropDown(menuHelpRect);
        }
         
        GUILayout.EndHorizontal();
    }  

    private void ExternalFileCommand(object args)
    {
        string str = (string)args;
        string path = string.Empty;
        string title = string.Empty;
        FileInfo fi;

        switch (str)
        {
            case "fileUpdate":
                title = "Select SVN file to run update";
                break;

            case "fileCommit":
                title = "Select SVN file to run commit";
                break;

            case "fileRemove":
                title = "Select SVN file to remove";
                break;

            case "fileRename":
                title = "Select SVN file to rename";
                break;

            case "fileRevert":
                title = "Select SVN file to revert";
                break;

            case "fileLock":
                title = "Select SVN file to lock";
                break;

            case "fileUnLock":
                title = "Select SVN file to unlock";
                break;

            case "fileAdd":
                title = "Select SVN file to add";
                break;

            case "fileCheckout":
                title = "Select file to checkout";
                break;
        }

        path = EditorUtility.OpenFilePanel(title, s_lastExternalPath, "");

        if (!string.IsNullOrEmpty(path))
        {
            fi = new FileInfo(path);
            switch (str)
            {
                case "fileUpdate":
                    GoodSVN.SVNUpdate(fi.FullName, (int)s_fileOptions);
                    break;

                case "fileCommit":
                    GoodSVN.SVNCommitFile(fi.FullName, (int)s_fileOptions);
                    break;

                case "fileRemove":
                    GoodSVN.SVNRemove(fi.FullName, (int)s_fileOptions);
                    break;

                case "fileRename":
                    GoodSVN.SVNRename(fi.FullName, (int)s_fileOptions);
                    break;

                case "fileRevert":
                    GoodSVN.ShowRevert(fi.FullName, (int)s_fileOptions);
                    break;

                case "fileAdd":
                    GoodSVN.SVNAdd(fi.FullName, (int)s_fileOptions);
                    break;
                case "fileLock":
                    GoodSVN.SVNLock(fi.FullName, (int)s_fileOptions);
                    break;
                case "fileUnLock":
                    GoodSVN.SVNUnLock(fi.FullName, (int)s_fileOptions);
                    break;
            }
            s_lastExternalPath = fi.Directory.FullName;
            SavePrefs();
        }
    }

    private void ExternalFolderCommand(object args)
    {
        string str = (string)args;
        string path = string.Empty;
        string title = string.Empty;
        FileInfo fi;

        switch (str)
        {
            case "folderUpdate":
                title = "Select SVN folder to run update";
                break;

            case "folderCommit":
                title = "Select SVN folder to run commit";
                break;

            case "folderRemove":
                title = "Select SVN folder to remove";
                break;

            case "folderRevert":
                title = "Select SVN folder to revert";
                break;

            case "folderAdd":
                title = "Select SVN folder to add";
                break;

            case "folderCheckout":
                title = "Select folder to checkout";
                break;

            case "folderCleanup":
                title = "Select folder to cleanup";
                break;

            case "folderLock":
                title = "Select folder to lock";
                break;

            case "folderUnLock":
                title = "Select folder to unlock";
                break;
        }

        path = EditorUtility.OpenFolderPanel(title, s_lastExternalPath, "");

        if (!string.IsNullOrEmpty(path))
        {
            fi = new FileInfo(path);
            switch (str)
            {
                case "folderUpdate":
                    GoodSVN.SVNUpdate(fi.FullName, (int)s_fileOptions);
                    break;

                case "folderCommit":
                    GoodSVN.SVNCommitFile(fi.FullName, (int)s_fileOptions);
                    break;

                case "folderRemove":
                    GoodSVN.SVNRemove(fi.FullName, (int)s_fileOptions);
                    break;

                case "folderRevert":
                    GoodSVN.ShowRevert(fi.FullName, (int)s_fileOptions);
                    break;

                case "folderAdd":
                    GoodSVN.SVNAdd(fi.FullName, (int)s_fileOptions);
                    break;

                case "folderCheckout":
                    GoodSVN.ShowCheckout(fi.FullName);
                    break;

                case "folderCleanup":
                    GoodSVN.ShowCleanUp(fi.FullName);
                    break; 

                case "folderLock":
                    GoodSVN.SVNLock(fi.FullName, (int)s_fileOptions);
                    break;

                case "folderUnLock":
                    GoodSVN.SVNUnLock(fi.FullName, (int)s_fileOptions);
                    break;
            }
            s_lastExternalPath = fi.FullName;
            SavePrefs();
        }
    }

    private void DoDropCheckForSVNPath(string[] droppedPaths)
    {
        if (droppedPaths != null && droppedPaths.Length > 0)
        {
            foreach (var path in droppedPaths)
            {
                FileInfo fi = new FileInfo(path);
                if (!fi.Exists) continue;

                if (fi.Extension.ToLower() == ".exe" || fi.Extension.ToLower() == ".app")
                {
                    Debug.Log("Found and setting SVN Path to: " + fi.FullName);
                    GoodSVN.PATH_TORTOISE_SVN = fi.FullName;
                    SavePrefs();
                    break;
                }
                else
                {
                    Debug.Log("Dropped path: " + path);
                }
            }
        }
    }
    #endregion

    #region DRAG/DROP
    public static string[] GetDroppedPaths()
    {
        EventType eventType = Event.current.type;
        bool isAccepted = false;

        if (eventType == EventType.DragUpdated || eventType == EventType.DragPerform)
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            if (eventType == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();
                isAccepted = true;
            }
            Event.current.Use();
        }
        return isAccepted ? DragAndDrop.paths : null;
    }
    #endregion

    #region PATHS
    public static void BrowseForSVN()
    {
        var newPath = EditorUtility.OpenFilePanel("Select your SVN program", GoodSVN.DEFAULT_PATH_TORTOISE_SVN,
                                                  "*");
        if (!string.IsNullOrEmpty(newPath))
        {
            GoodSVN.PATH_TORTOISE_SVN = newPath;
            SavePrefs();
            var notification = "Setting SVN path to " + newPath;
            Debug.Log(notification);
            if (window != null)
                window.ShowNotification(new GUIContent(notification));
        }
    }


    // LAST
    static private UnityEngine.Object[] s_lastObjectsSelected = new UnityEngine.Object[0];
    static private UnityEngine.Object s_lastActiveObject = null;

    // CURRENT PARSED LIST
    static private string[] s_lastParsedList = new string[0];
    static private string s_lastParsedPath = string.Empty;

    private string[] GetSelectedList()
    {
        if (Selection.activeObject != s_lastActiveObject || s_lastObjectsSelected.Length != Selection.objects.Length)
        {
            CachePaths();
        }

        return s_lastParsedList;
    }

    public static string GetFileOrAssetPath()
    {
        try
        {
            if (null == Selection.objects || Selection.objects.Length < 1)
            {
                return new FileInfo(Application.dataPath).FullName;
            }
            else
            {
                if (s_lastActiveObject != Selection.activeObject || Selection.objects.Length != s_lastObjectsSelected.Length)
                {
                    CachePaths();
                }

                return s_lastParsedPath;
            }
        }
        catch (Exception error)
        {
            Debug.LogWarning(error);
        }

        return string.Empty;
    }

    public static string GetFileOrBasePath()
    {
        try
        {
            if (null == Selection.objects || Selection.objects.Length < 1)
            {
                return new DirectoryInfo(Directory.GetCurrentDirectory()).FullName;
            }
            else
            {
                if (s_lastActiveObject != Selection.activeObject || Selection.objects.Length != s_lastObjectsSelected.Length)
                {
                    CachePaths();
                }

                return s_lastParsedPath;
            }
        }
        catch (Exception error)
        {
            Debug.LogWarning(error);
        }

        return string.Empty;
    }

    private static void CachePaths()
    {
        s_lastActiveObject = Selection.activeObject;
        s_lastObjectsSelected = Selection.objects;

        // GRAB FILES TO INCLUDE IN SVN COMMAND
        List<string> paths = new List<string>();
        for (var i = 0; i < Selection.objects.Length; i++)
        {
            var path = AssetDatabase.GetAssetPath(Selection.objects[i]);
            if (File.Exists(path) || Directory.Exists(path))
            {
                var fileInfo = new FileInfo(path);
                paths.Add(fileInfo.FullName);
            }
        }
        s_lastParsedPath = string.Join("*", paths.ToArray());

        // GRAB NAMES FOR DISPLAY
        string[] objects = new string[Selection.objects.Length];
        for (var i = 0; i < objects.Length; i++)
        {
            FileInfo info = null;
            var path = AssetDatabase.GetAssetPath(Selection.objects[i]);
            if (File.Exists(path))
            {
                info = new FileInfo(path);
                objects[i] = info.Name;
            }
            else if (Directory.Exists(path))
            {
                objects[i] = "\\" + new FileInfo(path).Name;
            }
        }
        Array.Sort(objects);
        s_lastParsedList = objects;
    }
    #endregion

    #region SVN-UPDATE
    [MenuItem("Assets/Good SVN/Assets/Update", true)]
    public static bool ValidateUpdateAssets() { return !GoodSVN.CantSync; }
    [MenuItem("Assets/Good SVN/Assets/Update", priority = 5000)]
    public static void UpdateAssets()
    {
#if EVAL_JUSTGOOD
        if (!GoodSVNPanel.CheckLicense())
        {
            return;
        }
#endif
        try
        {
            var fileInfo = new FileInfo(Application.dataPath);
            GoodSVN.SVNUpdate(fileInfo.FullName, (int)s_assetsOptions);
        }
        catch (Exception error)
        {
            Debug.LogWarning(error);
        }
    }

    [MenuItem("Assets/Good SVN/ProjectSettings/Update", true)]
    public static bool ValidateUpdateProjectSettings() { return !GoodSVN.CantSync; }
    [MenuItem("Assets/Good SVN/ProjectSettings/Update", priority = 5000)]
    public static void UpdateProjectSettings()
    {
#if EVAL_JUSTGOOD
        if (!GoodSVNPanel.CheckLicense())
        {
            return;
        }
#endif
        try
        {
            var dirInfo = new DirectoryInfo("ProjectSettings");
            GoodSVN.SVNUpdate(dirInfo.FullName, (int)s_projOptions);
        }
        catch (Exception error)
        {
            Debug.LogWarning(error);
        }
    }

    [MenuItem("Assets/Good SVN/Files/Update", true)]
    public static bool ValidateUpdateFile() { return !GoodSVN.CantSync && Selection.activeObject != null; }
    [MenuItem("Assets/Good SVN/Files/Update", priority = 5000)]
    public static void UpdateFile()
    {
#if EVAL_JUSTGOOD
        if (!GoodSVNPanel.CheckLicense())
        {
            return;
        }
#endif
        if (Selection.activeObject == null)
        {
            Debug.Log("Can't update null object.");
        }
        else
        {
            GoodSVN.SVNUpdate(GetFileOrAssetPath(), (int)s_fileOptions);
        }
    }
    #endregion

    #region SVN - COMMIT
    [MenuItem("Assets/Good SVN/Files/Commit", true)]
    public static bool ValidateCommitFile() { return !GoodSVN.CantSync && Selection.activeObject != null; }
    [MenuItem("Assets/Good SVN/Files/Commit", priority = 5001)]
    public static void CommitFile()
    {
#if EVAL_JUSTGOOD
        if (!GoodSVNPanel.CheckLicense())
        {
            return;
        }
#endif
        if (GoodSVN.CantSync)
        {
            Debug.LogWarning("TortoiseSVN path is invalid");
            return;
        }

        if (Selection.activeObject == null)
        {
            Debug.Log("Can't commit null object.");
        }
        else
        {
            GoodSVN.SVNCommitFile(GetFileOrAssetPath(), (int)s_fileOptions);
        }
    }

    [MenuItem("Assets/Good SVN/Assets/Commit", true)]
    public static bool ValidateCommitAssets() { return !GoodSVN.CantSync; }
    [MenuItem("Assets/Good SVN/Assets/Commit", priority = 5001)]
    public static void CommitAssets()
    {
#if EVAL_JUSTGOOD
        if (!GoodSVNPanel.CheckLicense())
        {
            return;
        }
#endif
        if (GoodSVN.CantSync)
        {
            Debug.LogWarning("TortoiseSVN path is invalid");
            return;
        }

        try
        {
            var fileInfo = new FileInfo(Application.dataPath);
            GoodSVN.SVNCommitFile(fileInfo.FullName, (int)s_assetsOptions);
        }
        catch (Exception error)
        {
            Debug.LogWarning(error);
        }
    }

    [MenuItem("Assets/Good SVN/Assets/Revert", true)]
    public static bool ValidateRevertAssets() { return !GoodSVN.CantSync; }
    [MenuItem("Assets/Good SVN/Assets/Revert", priority = 5001)]

    public static void RevertAssets()
    {
#if EVAL_JUSTGOOD
        if (!GoodSVNPanel.CheckLicense())
        {
            return;
        }
#endif
        if (GoodSVN.CantSync)
        {
            Debug.LogWarning("TortoiseSVN path is invalid");
            return;
        }

        try
        {
            var fileInfo = new FileInfo(Application.dataPath);
            GoodSVN.ShowRevert(fileInfo.FullName, (int)s_assetsOptions);
        }
        catch (Exception error)
        {
            Debug.LogWarning(error);
        }
    }

    [MenuItem("Assets/Good SVN/ProjectSettings/Commit", true)]
    public static bool ValidateCommitProjectSettings() { return !GoodSVN.CantSync; }
    [MenuItem("Assets/Good SVN/ProjectSettings/Commit", priority = 5001)]
    public static void CommitProjectSettings()
    {
#if EVAL_JUSTGOOD
        if (!GoodSVNPanel.CheckLicense())
        {
            return;
        }
#endif
        if (GoodSVN.CantSync)
        {
            Debug.LogWarning("TortoiseSVN path is invalid");
            return;
        }

        try
        {
            var dirInfo = new DirectoryInfo("ProjectSettings");
            GoodSVN.SVNCommitFile(dirInfo.FullName, (int)s_projOptions);
        }
        catch (Exception error)
        {
            Debug.LogWarning(error);
        }
    }
    #endregion

    [MenuItem("Assets/Good SVN/Files/Dif", true)]
    public static bool ValidateDiffFile() { return !GoodSVN.CantSync && Selection.activeObject != null && Selection.objects.Length == 1; }
    [MenuItem("Assets/Good SVN/Files/Dif", priority = 5000)]
    public static void DiffFile()
    {
#if EVAL_JUSTGOOD
        if (!GoodSVNPanel.CheckLicense())
        {
            return;
        }
#endif
        CachePaths();
        GoodSVN.SVNDiffFile(s_lastParsedPath, (int)s_fileOptions);
    }

    [MenuItem("Assets/Good SVN/Files/Edit Conflicts", true)]
    public static bool ValidateConflictEditorForFiles() { return !GoodSVN.CantSync && Selection.activeObject != null && Selection.objects.Length == 1; }
    [MenuItem("Assets/Good SVN/Files/Edit Conflicts", priority = 5000)]
    public static void ConflictEditorForFiles()
    {
#if EVAL_JUSTGOOD
        if (!GoodSVNPanel.CheckLicense())
        {
            return;
        }
#endif
        CachePaths();
        GoodSVN.SVNConflictEditorForFiles(s_lastParsedPath, (int)s_fileOptions);
    }

    [MenuItem("Assets/Good SVN/Files/Rename", true)]
    public static bool ValidateRenameFile() { return !GoodSVN.CantSync && Selection.activeObject != null && Selection.objects.Length == 1; }
    [MenuItem("Assets/Good SVN/Files/Rename", priority = 5000)]
    public static void RenameFile()
    {
#if EVAL_JUSTGOOD
        if (!GoodSVNPanel.CheckLicense())
        {
            return;
        }
#endif
        CachePaths();
        GoodSVN.SVNRename(s_lastParsedPath, (int)s_fileOptions);
    }


    [MenuItem("Assets/Good SVN/Files/UnLock", true)]
    public static bool ValidateUnLockFile() { return !GoodSVN.CantSync && Selection.activeObject != null; }
    [MenuItem("Assets/Good SVN/Files/UnLock", priority = 5000)]
    public static void UnLockFile()
    {
#if EVAL_JUSTGOOD
        if (!GoodSVNPanel.CheckLicense())
        {
            return;
        }
#endif
        CachePaths();
        GoodSVN.SVNUnLock(s_lastParsedPath, (int)s_fileOptions);
    }

    [MenuItem("Assets/Good SVN/Files/Lock", true)]
    public static bool ValidateLockFile() { return !GoodSVN.CantSync && Selection.activeObject != null; }
    [MenuItem("Assets/Good SVN/Files/Lock", priority = 5000)]
    public static void LockFile()
    {
#if EVAL_JUSTGOOD
        if (!GoodSVNPanel.CheckLicense())
        {
            return;
        }
#endif
        CachePaths();
        GoodSVN.SVNLock(s_lastParsedPath, (int)s_fileOptions);
    }

    #region SVN-ADD
    [MenuItem("Assets/Good SVN/Assets/Add", true)]
    public static bool ValidateAddAssets() { return !GoodSVN.CantSync; }
    [MenuItem("Assets/Good SVN/Assets/Add", priority = 5000)]
    public static void AddAssets()
    {
#if EVAL_JUSTGOOD
        if (!GoodSVNPanel.CheckLicense())
        {
            return;
        }
#endif
        try
        {
            var fileInfo = new FileInfo(Application.dataPath);
            GoodSVN.SVNAdd(fileInfo.FullName, (int)s_assetsOptions);
        }
        catch (Exception error)
        {
            Debug.LogWarning(error);
        }
    }

    [MenuItem("Assets/Good SVN/ProjectSettings/Revert", true)]
    public static bool ValidateRevertProjectSettings() { return !GoodSVN.CantSync; }
    [MenuItem("Assets/Good SVN/ProjectSettings/Revert", priority = 5000)]
    public static void RevertProjectSettings()
    {
#if EVAL_JUSTGOOD
        if (!GoodSVNPanel.CheckLicense())
        {
            return;
        }
#endif
        try
        {
            var dirInfo = new DirectoryInfo("ProjectSettings");
            GoodSVN.ShowRevert(dirInfo.FullName, (int)s_projOptions);
        }
        catch (Exception error)
        {
            Debug.LogWarning(error);
        }
    }

    [MenuItem("Assets/Good SVN/ProjectSettings/Add", true)]
    public static bool ValidateAddProjectSettings() { return !GoodSVN.CantSync; }
    [MenuItem("Assets/Good SVN/ProjectSettings/Add", priority = 5000)]
    public static void AddProjectSettings()
    {
#if EVAL_JUSTGOOD
        if (!GoodSVNPanel.CheckLicense())
        {
            return;
        }
#endif
        try
        {
            var dirInfo = new DirectoryInfo("ProjectSettings");
            GoodSVN.SVNAdd(dirInfo.FullName, (int)s_projOptions);
        }
        catch (Exception error)
        {
            Debug.LogWarning(error);
        }
    }

    [MenuItem("Assets/Good SVN/Files/Add", true)]
    public static bool ValidateAddFile() { return !GoodSVN.CantSync && Selection.activeObject != null; }
    [MenuItem("Assets/Good SVN/Files/Add", priority = 5000)]
    public static void AddFile()
    {
#if EVAL_JUSTGOOD
        if (!GoodSVNPanel.CheckLicense())
        {
            return;
        }
#endif
        if (Selection.activeObject == null)
        {
            Debug.Log("Can't Add null object.");
        }
        else
        {
            GoodSVN.SVNAdd(GetFileOrAssetPath(), (int)s_fileOptions);
        }
    }

    [MenuItem("Assets/Good SVN/Files/Remove", true)]
    public static bool ValidateRemoveFile() { return !GoodSVN.CantSync && Selection.activeObject != null; }
    [MenuItem("Assets/Good SVN/Files/Remove", priority = 5000)]
    public static void RemoveFile()
    {
#if EVAL_JUSTGOOD
        if (!GoodSVNPanel.CheckLicense())
        {
            return;
        }
#endif
        if (Selection.activeObject == null)
        {
            Debug.Log("Can't remove null object.");
        }
        else
        {
            GoodSVN.SVNRemove(GetFileOrAssetPath(), (int)s_fileOptions);
        }
    }
    #endregion

    #region SVN COMMANDS
    [MenuItem("Assets/Good SVN/Repo Browser", true)]
    public static bool ValidateShowBrowser() { return !GoodSVN.CantSync; }
    [MenuItem("Assets/Good SVN/Repo Browser", priority = 5000)]
    public static void ShowBrowser()
    {
#if EVAL_JUSTGOOD
        if (!GoodSVNPanel.CheckLicense())
        {
            return;
        }
#endif
        if (GoodSVN.CantSync)
        {
            Debug.LogWarning("TortoiseSVN path is invalid");
            return;
        }

        GoodSVN.ShowBrowser(GetFileOrBasePath(), (int)s_commandOptions);
    }

    [MenuItem("Assets/Good SVN/Checkout", true)]
    public static bool ValidateShowCheckout() { return !GoodSVN.CantSync; }
    [MenuItem("Assets/Good SVN/Checkout", priority = 5000)]
    public static void ShowCheckout()
    {
#if EVAL_JUSTGOOD
        if (!GoodSVNPanel.CheckLicense())
        {
            return;
        }
#endif
        if (GoodSVN.CantSync)
        {
            Debug.LogWarning("TortoiseSVN path is invalid");
            return;
        }

        GoodSVN.ShowCheckout(GetFileOrBasePath());
    }

    [MenuItem("Assets/Good SVN/Cleanup", true)]
    public static bool ValidateShowCleanUp() { return !GoodSVN.CantSync; }
    [MenuItem("Assets/Good SVN/Cleanup", priority = 5000)]
    public static void ShowCleanUp()
    {
#if EVAL_JUSTGOOD
        if (!GoodSVNPanel.CheckLicense())
        {
            return;
        }
#endif
        if (GoodSVN.CantSync)
        {
            Debug.LogWarning("TortoiseSVN path is invalid");
            return;
        }

        GoodSVN.ShowCleanUp(Directory.GetCurrentDirectory());
    }

    [MenuItem("Assets/Good SVN/Log", true)]
    public static bool ValidateShowLog() { return !GoodSVN.CantSync; }
    [MenuItem("Assets/Good SVN/Log", priority = 5000)]
    public static void ShowLog()
    {
#if EVAL_JUSTGOOD
        if (!GoodSVNPanel.CheckLicense())
        {
            return;
        }
#endif
        if (GoodSVN.CantSync)
        {
            Debug.LogWarning("TortoiseSVN path is invalid");
            return;
        }

        GoodSVN.ShowLog(GetFileOrBasePath(), (int)s_commandOptions);
    }

    [MenuItem("Assets/Good SVN/Revert", true)]
    public static bool ValidateShowRevert() { return !GoodSVN.CantSync; }
    [MenuItem("Assets/Good SVN/Revert", priority = 5000)]
    public static void ShowRevert()
    {
#if EVAL_JUSTGOOD
        if (!GoodSVNPanel.CheckLicense())
        {
            return;
        }
#endif
        if (GoodSVN.CantSync)
        {
            Debug.LogWarning("TortoiseSVN path is invalid");
            return;
        }

        GoodSVN.ShowRevert(GetFileOrBasePath(), (int)s_commandOptions);
    }

    [MenuItem("Assets/Good SVN/Check Status", true)]
    public static bool ValidateShowStatus() { return !GoodSVN.CantSync; }
    [MenuItem("Assets/Good SVN/Check Status", priority = 5000)]
    public static void ShowStatus()
    {
#if EVAL_JUSTGOOD
        if (!GoodSVNPanel.CheckLicense())
        {
            return;
        }
#endif
        if (GoodSVN.CantSync)
        {
            Debug.LogWarning("TortoiseSVN path is invalid");
            return;
        }

        GoodSVN.ShowStatus(GetFileOrBasePath(), (int)s_commandOptions);
    }
    #endregion
}
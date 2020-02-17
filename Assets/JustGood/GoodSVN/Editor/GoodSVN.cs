/*
 * Authors:  Jesse Graupmann & Tim Graupmann
 * Just Good Design, @copyright 2012-2013  All rights reserved.
 *
*/

using UnityEditor;
using UnityEngine;
using System.Collections;
using System.IO;
using System;

/// <summary>
/// See for more Commands: http://tortoisesvn.net/docs/nightly/TortoiseSVN_en/tsvn-automation.html
/// </summary>
public static class GoodSVN
{
    // SVN Types
    public enum SVNTypes
    {
        TortoiseSVN,
        NativeMac
    }

    public enum DialogCloseSettings
    {
        DontCloseAutomatically,
        AutoCloseNoErrors,
        AutoCloseNoErrorsConflicts,
        AutoCloseNoErrorsConflictsMerges,
    }

    #region VARIABLES
    public const string COMPANY = "Just Good Design";
    public const string PRODUCT = "Good SVN";
    public const string VERSION = "1.3";
    public static string URL { get { return string.Format("http://www.justgooddesign.com/products/?product={0}&version={1}", PRODUCT, VERSION); } }

    const string quote = "\"";

    const string EXCLUDE_SVN_FOLDER = ".SVN";

    // paths
    public const string DEFAULT_PATH_TORTOISE_SVN = @"C:\Program Files\TortoiseSVN\bin\TortoiseProc.exe";
    public const string DEFAULT_PATH_CROSSOVER_MAC = @"/Applications/CrossOver.app";
    public static string PATH_CROSSOVER_MAC = @"/Applications/CrossOver.app";
    public static string PATH_TORTOISE_SVN = @"C:\Program Files\TortoiseSVN\bin\TortoiseProc.exe";

    // commands
    public const string COMMAND_TORTOISE_ADD = @"/command:add /path:{0} /closeonend:{1}";
    public const string COMMAND_TORTOISE_REMOVE = @"/command:remove /path:{0} /closeonend:{1}";
    public const string COMMAND_TORTOISE_UPDATE = @"/command:update /path:{0} /closeonend:{1}";
    public const string COMMAND_TORTOISE_CHECKOUT = @"/command:checkout /path:{0} /findtype:0";
    public const string COMMAND_TORTOISE_CLEANUP = @"/command:cleanup /path:{0} /findtype:0";
    public const string COMMAND_TORTOISE_COMMIT = @"/command:commit /path:{0} /closeonend:{1}";
    public const string COMMAND_TORTOISE_LOG = @"/command:log /path:{0} /findtype:0 /closeonend:0";
    public const string COMMAND_TORTOISE_REPO = @"/command:repobrowser /path:{0} /findtype:0 /closeonend:0";
    public const string COMMAND_TORTOISE_STATUS = @"/command:repostatus /path:{0} /remote /closeonend:0";
    public const string COMMAND_TORTOISE_REVERT = @"/command:revert /path:{0} /closeonend:0";
    public const string COMMAND_TORTOISE_RENAME = @"/command:rename /path:{0} /closeonend:0";
    public const string COMMAND_TORTOISE_LOCK = @"/command:lock /path:{0} /closeonend:0";
    public const string COMMAND_TORTOISE_UNLOCK = @"/command:unlock /path:{0} /closeonend:0";

    public const string COMMAND_TORTOISE_DIF = @"/command:diff /path:{0} /closeonend:0";
    public const string COMMAND_TORTOISE_CONFLICT_EDITOR = @"/command:conflicteditor /path:{0} /closeonend:0";

    public static GoodSVN.SVNTypes SVNType = GoodSVN.SVNTypes.TortoiseSVN;
    public static bool UseCrossover = false;

    private const string KEY_CROSSOVER_BOTTLE = "GoodSVN_BottleName";

    /// <summary>
    /// 0 don't close the dialog automatically
    /// 1 auto close if no errors
    /// 2 auto close if no errors and conflicts
    /// 3 auto close if no errors, conflicts and merges
    /// </summary>
    public static int DialogueCloseSetting = 0;

#if false
    // MAC COMMANDS 
    // http://developer.apple.com/library/mac/#documentation/Darwin/Reference/ManPages/man1/svn.1.html
    // http://svnbook.red-bean.com/
    // http://svnbook.red-bean.com/en/1.7/svn-book.html#svn.ref.svn.c.update
    // $ svn update [PATH]
    public const string PATH_XCODE_SVN = @"/Applications/Xcode.app/Contents/Developer/usr/bin/svn";
    public const string COMMAND_XCODE_UPDATE = @"$ svn update {0}";
    public const string COMMAND_XCODE_COMMIT = @"$ svn commit {0}";
#endif

    public static bool CantSync
    {
        get
        {
#if EVAL_JUSTGOOD
            if (!GoodSVNPanel.CheckLicense())
            {
                return true;
            }
#endif

            switch (Application.platform)
            {
                case RuntimePlatform.OSXEditor:
                    return !Directory.Exists(PATH_CROSSOVER_MAC);
                default:
                    return !File.Exists(PATH_TORTOISE_SVN);
            }
        }
    }
    #endregion

    #region PREFERENCES

    public static string GetCrossOverBottle()
    {
        string bottleName = "TortoiseSVN";
        if (EditorPrefs.HasKey(KEY_CROSSOVER_BOTTLE))
        {
            bottleName = EditorPrefs.GetString(KEY_CROSSOVER_BOTTLE);
        }

        return bottleName;
    }

    public static void SetCrossOverBottle(string bottleName)
    {
        EditorPrefs.SetString(KEY_CROSSOVER_BOTTLE, bottleName);
    }

    #endregion

    #region COMMANDS
    public static void SVNConflictEditorForFiles(string path, int closeSetting)
    {
        var args = quote + path + quote;
        args = string.Format(COMMAND_TORTOISE_CONFLICT_EDITOR, args, closeSetting);

        WrapRunSVNThread(args);
    }

    public static void SVNDiffFile(string path, int closeSetting)
    {
        var args = quote + path + quote;
        args = string.Format(COMMAND_TORTOISE_DIF, args, closeSetting);

        WrapRunSVNThread(args);
    }

    public static void SVNRename(string path, int closeSetting)
    {
        var args = quote + path + quote;
        args = string.Format(COMMAND_TORTOISE_RENAME, args, closeSetting);

        WrapRunSVNThread(args);
    }

    public static void SVNUnLock(string path, int closeSetting)
    {
        var args = quote + path + quote;
        args = string.Format(COMMAND_TORTOISE_UNLOCK, args, closeSetting);

        WrapRunSVNThread(args);
    }

    public static void SVNLock(string path, int closeSetting)
    {
        var args = quote + path + quote;
        args = string.Format(COMMAND_TORTOISE_LOCK, args, closeSetting);

        WrapRunSVNThread(args);
    }

    public static void SVNAdd(string path, int closeSetting)
    {
        var args = quote + path + quote;
        args = string.Format(COMMAND_TORTOISE_ADD, args, closeSetting);

        WrapRunSVNThread(args);
    }

    public static void SVNRemove(string path, int closeSetting)
    {
        var args = quote + path + quote;
        args = string.Format(COMMAND_TORTOISE_REMOVE, args, closeSetting);

        WrapRunSVNThread(args);
    }

    public static void SVNUpdate(string path, int closeSetting)
    {
        var args = quote + path + quote;
        args = string.Format(COMMAND_TORTOISE_UPDATE, args, closeSetting);

        WrapRunSVNThread(args);
    }

    public static void SVNCommitFile(string path, int closeSetting)
    {
        var args = quote + path + quote;
        args = string.Format(COMMAND_TORTOISE_COMMIT, args, closeSetting); // keep window open to avoid errors

        WrapRunSVNThread(args);
    }
    public static void ShowLog(string path, int closeSetting)
    {
        var args = quote + path + quote;

        args = string.Format(COMMAND_TORTOISE_LOG, args, closeSetting); // keep window open to avoid errors


        WrapRunSVNThread(args);
    }
    public static void ShowBrowser(string path, int closeSetting)
    {
        var args = quote + path + quote;

        args = string.Format(COMMAND_TORTOISE_REPO, args, closeSetting); // keep window open to avoid errors

        WrapRunSVNThread(args);
    }

    public static void ShowCheckout(string path)
    {
        var args = quote + path + quote;

        args = string.Format(COMMAND_TORTOISE_CHECKOUT, args); // keep window open to avoid errors

        WrapRunSVNThread(args);
    }

    public static void ShowCleanUp(string path)
    {
        var args = quote + path + quote;

        args = string.Format(COMMAND_TORTOISE_CLEANUP, args); // keep window open to avoid errors

        WrapRunSVNThread(args);
    }

    public static void ShowStatus(string path, int closeSetting)
    {
        var args = quote + path + quote;
        args = string.Format(COMMAND_TORTOISE_STATUS, args, closeSetting); // keep window open to avoid errors

        WrapRunSVNThread(args);
    }

    public static void ShowRevert(string path, int closeSetting)
    {
        var args = quote + path + quote;
        args = string.Format(COMMAND_TORTOISE_REVERT, args, closeSetting); // keep window open to avoid errors

        WrapRunSVNThread(args);
    }
    #endregion

    #region RUN SVN
    private static void WrapRunSVNThread(string args)
    {
        switch (Application.platform)
        {
            case RuntimePlatform.OSXEditor:
                System.Collections.Specialized.StringDictionary env = new System.Collections.Specialized.StringDictionary();
                string goodcmd = args.Replace("\"", string.Empty);
                env.Add("goodcmd", goodcmd);

                //Debug.Log(string.Format("export goodcmd={0}ls", goodcmd));

                string bottleName = GoodSVN.GetCrossOverBottle();
                env.Add("bottle", bottleName);

                //Debug.Log(string.Format("export bottle={0}", bottleName));

                //Debug.Log(args);
                string crossOverScript = GoodEditorPanel.FindAssetPath("GoodSVNCrossOver.sh", EXCLUDE_SVN_FOLDER);
                if (string.IsNullOrEmpty(crossOverScript))
                {
                    Debug.LogError(string.Format("Failed to find script: {0}", crossOverScript));
                    break;
                }

                string command = "/bin/sh";
                string argument = crossOverScript.Replace(@"\", "/");
                //Debug.Log(string.Format("{0} {1}", command, argument));
                GoodEditorProcess.RunProcessThread(command, argument, null, env);
                break;
            default:
                RunSVNThread(args);
                break;
        }
    }

    public static void RunSVNThread(string args)
    {
        try
        {
            GoodEditorProcess.RunProcessThread(PATH_TORTOISE_SVN, args);
        }
        catch (Exception error)
        {
            Debug.LogWarning(error);
        }
    }
    #endregion
}
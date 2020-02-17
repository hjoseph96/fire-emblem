/*
 * Authors:  Jesse Graupmann & Tim Graupmann
 * Just Good Design, @copyright 2012-2013  All rights reserved.
 *
*/
 
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEditor;
using UnityEngine;

public static class GoodEditorProcess
{
    public const string COMPANY = "Just Good Design";
    public const string PRODUCT = "Good Editor Process";
    public const string VERSION = "1.2.0";
    public static string URL { get { return string.Format("http://www.justgooddesign.com/products/?product={0}&version={1}", PRODUCT, VERSION); } }
    public static bool DebugProcesses = false;

    #region OPEN DIRECTORY
    public static System.Diagnostics.Process OpenDirectory(string path)
    {
        FileInfo fi = new FileInfo(path);
        if (!string.IsNullOrEmpty(fi.DirectoryName))
        {
            return System.Diagnostics.Process.Start(fi.DirectoryName);
        }
        else
        {
            Debug.LogWarning("Directory doesn't exist for " + path);
        }
        return null;
    }
    #endregion

    #region RUN PROCESS
    public static System.Diagnostics.Process CreateProcess(string path, string arguments)
    {
        return CreateProcess(path, arguments, null);
    }

    public static System.Diagnostics.Process CreateProcess(string path, string arguments, System.Collections.Specialized.StringDictionary env)
    {
        System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo(path, arguments)
        {
            WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
            UseShellExecute = false,
            ErrorDialog = false,
            CreateNoWindow = true,
            RedirectStandardError = true,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            LoadUserProfile = true
        };

        if (null != env)
        {
            foreach (System.Collections.DictionaryEntry kvp in env)
            {
                string key = (string)kvp.Key;
                string val = (string)kvp.Value;

                //Debug.Log(string.Format("key={0} val={1}", key, val));
                startInfo.EnvironmentVariables[key] = val;
            }
        }

        return new System.Diagnostics.Process() { StartInfo = startInfo };
    }

    public static System.Diagnostics.Process RunProcess(string path, string arguments)
    {
        return RunProcess(path, arguments, null);
    }

    public static System.Diagnostics.Process RunProcess(string path, string arguments, System.Collections.Specialized.StringDictionary env)
    {
        System.Diagnostics.Process process = CreateProcess(path, arguments, env);

        if (DebugProcesses) Debug.Log(string.Format("[Run Process] filename={0} arguments={1}", process.StartInfo.FileName, process.StartInfo.Arguments));

        DateTime startTime = DateTime.Now;
        process.Start();

        if (DebugProcesses) Debug.Log(string.Format("[Results] PID:{0} elapsed:{1}", process.Id, (DateTime.Now - startTime).TotalSeconds));

        return process;
    }

    public static System.Diagnostics.Process RunProcess(string path, string arguments, ref string output, ref string error)
    {
        try
        {
            if (string.IsNullOrEmpty(path))
            {
                if (DebugProcesses) Debug.LogWarning(string.Format("Can't run process on a file that doesn't exist: path={0} args={1}", path, arguments));
                return null;
            }

            System.Diagnostics.Process process = CreateProcess(path, arguments);
            string errorData = string.Empty;


            process.ErrorDataReceived += (sender, line) =>
            {
                if (!string.IsNullOrEmpty(line.Data))
                {
                    errorData += line.Data + Environment.NewLine;
                }
            };

            if (DebugProcesses) Debug.Log(string.Format("[Run Process] filename={0} arguments={1}", process.StartInfo.FileName, process.StartInfo.Arguments));
            DateTime startTime = DateTime.Now;
            process.Start();

            process.BeginErrorReadLine();
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            while (!process.HasExited)
            {
                sb.Append(process.StandardOutput.ReadToEnd());
            }
            output = sb.ToString();
            process.WaitForExit(2000);
            error = errorData;


            if (DebugProcesses) Debug.Log(string.Format("[Results] PID:{0} elapsed:{1} {2}{3}{4}{5}{6}", process.Id, (DateTime.Now - startTime).TotalSeconds,
                string.IsNullOrEmpty(error) ? "no errors " : " ", // 2
                string.IsNullOrEmpty(output) ? "no output " : " ", // 3
                string.IsNullOrEmpty(error) ? "" : "errors: " + error,//4
                string.IsNullOrEmpty(error) ? " " : "\n",
                string.IsNullOrEmpty(output) ? "" : "output: " + output));

            //if (output.Length > 0 ) Debug.Log("Output: " + output);
            //if (error.Length > 0 ) Debug.Log("Error: " + error); 

            return process;
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning(string.Format("Unable to run process: path={0} arguments={1} exception={2}", path,
                                           arguments, ex));
        }
        return null;
    }

    #endregion

    #region THREADING
    public static System.Threading.ThreadPriority ThreadPriority = System.Threading.ThreadPriority.BelowNormal;
    public static GoodProcessThread RunProcessThread(string path, string arguments)
    {
        return RunProcessThread(path, arguments, null);
    }
    public static GoodProcessThread RunProcessThread(string path, string arguments, Action<GoodProcessThread> completeCallback)
    {
        GoodProcessThread worker = new GoodProcessThread(path, arguments, completeCallback);
        return worker.RunThread();
    }

    public static GoodProcessThread RunProcessThread(string path, string arguments, Action<GoodProcessThread> completeCallback, System.Collections.Specialized.StringDictionary env)
    {
        GoodProcessThread worker = new GoodProcessThread(path, arguments, completeCallback, env);
        worker.Thread = new Thread(worker.Work)
        {
            IsBackground = true,
            Priority = ThreadPriority
        };
        worker.Thread.Start();

        return worker;
    }
    #endregion
}

#region GOOD THREADS
public class GoodActionThread
{
    public bool IsRunning { get; private set; }
    public object[] Arguments = null;
    public Action ThreadedAction = null;
    public Action<GoodActionThread> CompleteCallback = null;


    public GoodActionThread(Action actionToThread, Action<GoodActionThread> completeCallback, params object[] arguments)
    {
        ThreadedAction = actionToThread;
        Arguments = arguments;
        CompleteCallback = completeCallback;
    }

    public void Work()
    {
        IsRunning = true;
        ThreadedAction.Invoke();
        IsRunning = false;

        if (null != CompleteCallback)
        {
            CompleteCallback.Invoke(this);
        }
    }
}

public class GoodProcessThread
{
    public bool HasError { get { return !string.IsNullOrEmpty(Error); } }
    public bool IsRunning { get; private set; }
    public string Path = string.Empty;
    public string Arguments = string.Empty;
    public string Error = string.Empty;
    public string Output = string.Empty;
    public System.Diagnostics.Process Process;
    public Action<GoodProcessThread> CompleteCallback = null;
    public System.Collections.Specialized.StringDictionary Env = null;
    public System.Threading.ThreadPriority Priority = System.Threading.ThreadPriority.Normal;
    public Thread Thread;

    public GoodProcessThread(string path, string arguments)
    {
        Path = path;
        Arguments = arguments;
    }

    public GoodProcessThread(string path, string arguments, Action<GoodProcessThread> completeCallback)
    {
        Path = path;
        Arguments = arguments;
        CompleteCallback = completeCallback;
    }

    public GoodProcessThread(string path, string arguments, Action<GoodProcessThread> completeCallback, System.Collections.Specialized.StringDictionary env)
    {
        Path = path;
        Arguments = arguments;
        CompleteCallback = completeCallback;
        Env = env;
    }

    public void Work()
    {
        Error = string.Empty;
        Output = string.Empty;

        IsRunning = true;
        if (Env == null)
        {
            Process = GoodEditorProcess.RunProcess(Path, Arguments, ref Output, ref Error);
        }
        else
        {
            Process = GoodEditorProcess.RunProcess(Path, Arguments, Env);
        }
        IsRunning = false;

        if (null != CompleteCallback)
        {
            CompleteCallback.Invoke(this);
        }
    }

    public GoodProcessThread RunThread()
    {
        Thread = new Thread(Work)
        {
            Priority = Priority,
            IsBackground = true
        };
        Thread.Start();

        return this;
    }

    public GoodProcessThread RunThread(string path, string arguments, Action<GoodProcessThread> completeCallback)
    {
        Path = path;
        Arguments = arguments;
        CompleteCallback = completeCallback;

        RunThread();

        return this;
    }
}
#endregion
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class AppHelper
{
    Process proc;
    public bool isRunning { get { return !proc.HasExited; } }
    public bool hasExited { get { return proc.HasExited; } }
    public int getExitCode { get { return proc.ExitCode; } }

    GameData data;
    public GameData getData { get { return data; } }

    public AppHelper(GameData gameData)
    {
        data = gameData;
        proc = Process.Start(data.exePath);
    }

    /// <summary>
    /// Kills the application and returns its exit code
    /// </summary>
    /// <returns></returns>
    public int KillApp()
    {
        proc.Kill();
        return proc.ExitCode;
    }
}

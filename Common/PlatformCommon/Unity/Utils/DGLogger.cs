using System.Diagnostics;
using UnityEngine;

public static class DGLogger
{

    [Conditional("ENABLE_LOGS")]
    public static void Log(string logMsg)
    {
        UnityEngine.Debug.Log(logMsg);
    }

}
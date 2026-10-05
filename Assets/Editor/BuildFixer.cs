#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

public static class BuildFixer
{
    [PostProcessBuild(0)]
    public static void CleanUpExistingUsdPluginsBeforeCopy(BuildTarget target, string pathToBuiltProject)
    {
        string destination = "";

        if (target == BuildTarget.StandaloneLinux64)
        {
            destination = pathToBuiltProject.Replace(".x86_64", "_Data/Plugins");
        }
        else if (target == BuildTarget.StandaloneOSX)
        {
            destination = pathToBuiltProject + "/Contents/Plugins";
        }
        else if (target == BuildTarget.StandaloneWindows64)
        {
            destination = pathToBuiltProject.Replace(".exe", "_Data/Plugins");
        }
        else
        {
            return;
        }

        if (!Directory.Exists(destination)) return;

        string usdDir = Path.Combine(destination, "usd");
        string plugInfoFile = Path.Combine(destination, "plugInfo.json");

        if (Directory.Exists(usdDir))
        {
            FileUtil.DeleteFileOrDirectory(usdDir);
            Debug.Log("[BuildFixer] Cleaned existing '_Data/Plugins/usd' folder to prevent rebuild IOException.");
        }

        if (File.Exists(plugInfoFile))
        {
            FileUtil.DeleteFileOrDirectory(plugInfoFile);
            Debug.Log("[BuildFixer] Cleaned existing '_Data/Plugins/plugInfo.json' file to prevent rebuild IOException.");
        }
    }
}
#endif

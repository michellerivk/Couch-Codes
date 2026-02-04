using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class CopyServerToBuild : IPostprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPostprocessBuild(BuildReport report)
    {
        // report.summary.outputPath is the full path to the built EXE
        var buildFolder = Path.GetDirectoryName(report.summary.outputPath);

        var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        var srcServer = Path.Combine(projectRoot, "server");
        var dstServer = Path.Combine(buildFolder, "server");

        if (!Directory.Exists(srcServer))
        {
            Debug.LogError($"Server folder not found: {srcServer}");
            return;
        }

        // Clean destination
        if (Directory.Exists(dstServer))
            FileUtil.DeleteFileOrDirectory(dstServer);

        FileUtil.CopyFileOrDirectoryFollowSymlinks(srcServer, dstServer);
        Debug.Log($"Copied server folder to build: {dstServer}");
    }
}

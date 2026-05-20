using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// One-command demo builder for Text-Quest-Reader. Builds MainScene to a
/// predictable folder under Builds/TextQuestReaderDemo/ on the host
/// platform. Invokable from the CLI:
///
///   /path/to/Unity -projectPath . -batchmode -quit -nographics \
///     -executeMethod BuildTextQuestDemo.BuildCurrentPlatform \
///     -logFile Logs/build-demo.log
///
/// Use BuildWindows / BuildMacOS / BuildLinux for explicit cross-platform
/// builds (the host machine must have that target installed via Unity Hub).
/// </summary>
public static class BuildTextQuestDemo
{
    private const string MainScenePath = "Assets/_Scenes/MainScene.unity";
    private const string BuildRootName = "Builds/TextQuestReaderDemo";

    public static void BuildCurrentPlatform()
    {
        BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
        BuildTargetGroup group = BuildPipeline.GetBuildTargetGroup(target);
        Build(target, group, "current");
    }

    public static void BuildWindows()
    {
        Build(BuildTarget.StandaloneWindows64, BuildTargetGroup.Standalone, "win64");
    }

    public static void BuildMacOS()
    {
        Build(BuildTarget.StandaloneOSX, BuildTargetGroup.Standalone, "macos");
    }

    public static void BuildLinux()
    {
        Build(BuildTarget.StandaloneLinux64, BuildTargetGroup.Standalone, "linux64");
    }

    private static void Build(BuildTarget target, BuildTargetGroup group, string folderSuffix)
    {
        string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        string outDir = Path.Combine(projectRoot, BuildRootName, folderSuffix);
        Directory.CreateDirectory(outDir);

        string executableName = ResolveExecutableName(target);
        string outPath = Path.Combine(outDir, executableName);

        // Make sure MainScene is in the build, even if EditorBuildSettings
        // hasn't been touched. We don't mutate global build settings; we
        // pass the scene path directly to BuildPlayer.
        if (!File.Exists(MainScenePath))
        {
            Debug.LogError($"[BuildTextQuestDemo] MainScene not found at {MainScenePath}. Aborting.");
            EditorApplication.Exit(2);
            return;
        }

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = new[] { MainScenePath },
            locationPathName = outPath,
            target = target,
            targetGroup = group,
            options = BuildOptions.None
        };

        Debug.Log($"[BuildTextQuestDemo] Starting build target={target} -> {outPath}");
        BuildReport report = BuildPipeline.BuildPlayer(options);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"[BuildTextQuestDemo] BUILD OK target={target} size={summary.totalSize} duration={summary.totalTime} output={outPath}");
        }
        else
        {
            Debug.LogError($"[BuildTextQuestDemo] BUILD FAILED target={target} result={summary.result} errors={summary.totalErrors}");
            EditorApplication.Exit(1);
        }
    }

    private static string ResolveExecutableName(BuildTarget target)
    {
        switch (target)
        {
            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64:
                return "TextQuestReaderDemo.exe";
            case BuildTarget.StandaloneOSX:
                return "TextQuestReaderDemo.app";
            case BuildTarget.StandaloneLinux64:
                return "TextQuestReaderDemo";
            default:
                return "TextQuestReaderDemo";
        }
    }
}

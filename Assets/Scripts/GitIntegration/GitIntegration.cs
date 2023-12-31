#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace GitIntegration
{
    [InitializeOnLoad]
    public class SmartMergeRegistrator
    {
        private const string SmartMergeRegistratorEditorPrefsKey = "smart_merge_installed";
        private const int Version = 1;
        private static readonly string VersionKey = $"{Version}_{Application.unityVersion}";

        public static void ExecuteGitWithParams(string param)
        {
            var processInfo = new System.Diagnostics.ProcessStartInfo("git")
            {
                UseShellExecute = false,
                WorkingDirectory = Environment.CurrentDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            var process = new System.Diagnostics.Process();
            process.StartInfo = processInfo;
            process.StartInfo.FileName = "git";
            process.StartInfo.Arguments = param;
            process.Start();
            process.WaitForExit();

            if (process.ExitCode != 0)
                throw new Exception(process.StandardError.ReadLine());

            process.StandardOutput.ReadLine();
        }

        [MenuItem("Tools/Git/SmartMerge registration")]
        private static void SmartMergeRegister()
        {
            try
            {
                var unityYamlMergePath = EditorApplication.applicationContentsPath + "/Tools" + "/UnityYAMLMerge.exe";
                ExecuteGitWithParams("config merge.unityyamlmerge.name \"Unity SmartMerge (UnityYamlMerge)\"");
                ExecuteGitWithParams(
                    $"config merge.unityyamlmerge.driver \"\\\"{unityYamlMergePath}\\\" merge -h -p --force --fallback none %O %B %A %A\"");
                ExecuteGitWithParams("config merge.unityyamlmerge.recursive binary");
                EditorPrefs.SetString(SmartMergeRegistratorEditorPrefsKey, VersionKey);
                Debug.Log($"Succesfuly registered UnityYAMLMerge with path {unityYamlMergePath}");
            }
            catch (Exception e)
            {
                Debug.Log($"Fail to register UnityYAMLMerge with error: {e}");
            }
        }

        //Unity calls the static constructor when the engine opens
        static SmartMergeRegistrator()
        {
            var installedVersionKey = EditorPrefs.GetString(SmartMergeRegistratorEditorPrefsKey);
            if (installedVersionKey != VersionKey)
                SmartMergeRegister();
        }
    }
}
#endif
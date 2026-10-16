using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using PerfMonitor.Recording;
using UnityEngine;

namespace PerfMonitor.Editor
{
    // GPU and render thread timings need Frame Timing Stats in Player Settings, so turn it on for every build
    sealed class EnableFrameTimingOnBuild : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;
        public void OnPreprocessBuild(BuildReport report) => PlayerSettings.enableFrameTimingStats = true;
    }

    static class PerfMonitorMenu
    {
        [MenuItem("Tools/Perf Monitor/Enable Frame Timing Stats")]
        static void EnableFrameTiming()
        {
            PlayerSettings.enableFrameTimingStats = true;
            Debug.Log("PerfMonitor: Frame Timing Stats enabled (GPU and render-thread percentages).");
        }

        [MenuItem("Tools/Perf Monitor/Open Recordings Folder")]
        static void OpenFolder()
        {
            Directory.CreateDirectory(PerfRecorder.Folder);
            EditorUtility.RevealInFinder(PerfRecorder.Folder);
        }
    }
}

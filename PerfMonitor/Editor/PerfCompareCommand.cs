using System;
using PerfMonitor.Analysis;
using UnityEditor;
using UnityEngine;

namespace PerfMonitor.Editor
{
    // Command line entry point for CI. Exits with code 1 when the new build is slower.
    // Example: Unity -batchmode -quit -projectPath . -executeMethod PerfMonitor.Editor.PerfCompareCommand.Run -baseline a.csv -candidate b.csv -threshold 5
    public static class PerfCompareCommand
    {
        public static void Run()
        {
            string baselinePath = GetArg("-baseline");
            string candidatePath = GetArg("-candidate");
            if (baselinePath == null || candidatePath == null)
            {
                Debug.LogError("PerfCompare: pass -baseline <csv> and -candidate <csv>");
                EditorApplication.Exit(2);
                return;
            }

            float threshold = 5f;
            string thresholdArg = GetArg("-threshold");
            if (thresholdArg != null)
            {
                float.TryParse(thresholdArg, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out threshold);
            }

            try
            {
                RecordingStats baseline = RecordingLoader.Load(baselinePath);
                RecordingStats candidate = RecordingLoader.Load(candidatePath);
                var rows = new RecordingComparer { ThresholdPct = threshold }.Compare(baseline, candidate);

                Debug.Log(ComparisonReport.Build(baseline, candidate, rows));
                EditorApplication.Exit(RecordingComparer.HasRegression(rows) ? 1 : 0);
            }
            catch (Exception exception)
            {
                Debug.LogError("PerfCompare failed: " + exception.Message);
                EditorApplication.Exit(2);
            }
        }

        static string GetArg(string name)
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == name)
                {
                    return args[i + 1];
                }
            }
            return null;
        }
    }
}

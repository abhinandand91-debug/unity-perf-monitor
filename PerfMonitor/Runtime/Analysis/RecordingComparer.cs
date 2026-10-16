using System;
using System.Collections.Generic;
using PerfMonitor.Core;

namespace PerfMonitor.Analysis
{
    public enum Change
    {
        Better,
        Same,
        Worse,
        NotComparable,
    }

    public struct ComparisonRow
    {
        public string Name;
        public string Unit;
        public float Baseline;
        public float Candidate;
        public float DeltaPct;
        public Change Change;
    }

    // Compares two recordings (for example last build vs new build) metric by metric
    public sealed class RecordingComparer
    {
        // Changes smaller than this many percent count as noise
        public float ThresholdPct = 5f;

        public List<ComparisonRow> Compare(RecordingStats baseline, RecordingStats candidate)
        {
            var rows = new List<ComparisonRow>();
            Add(rows, "Average FPS", "fps", baseline.AvgFps, candidate.AvgFps, true);
            Add(rows, "1% low FPS", "fps", baseline.Low1PercentFps, candidate.Low1PercentFps, true);
            Add(rows, "Frame time p95", "ms", baseline.FrameP95Ms, candidate.FrameP95Ms, false);
            Add(rows, "Frame time p99", "ms", baseline.FrameP99Ms, candidate.FrameP99Ms, false);
            Add(rows, "Frames over budget", "%", baseline.OverBudgetPct, candidate.OverBudgetPct, false);
            Add(rows, "Main thread time", "ms", baseline.AvgMainThreadMs, candidate.AvgMainThreadMs, false);
            Add(rows, "Render thread time", "ms", baseline.AvgRenderThreadMs, candidate.AvgRenderThreadMs, false);
            Add(rows, "GPU time", "ms", baseline.AvgGpuMs, candidate.AvgGpuMs, false);
            Add(rows, "CPU (process)", "%", baseline.AvgProcessCpuPct, candidate.AvgProcessCpuPct, false);
            Add(rows, "Peak app memory", "MB", baseline.PeakAppMemoryMB, candidate.PeakAppMemoryMB, false);
            Add(rows, "GC allocation", "KB/frame", baseline.AvgGcKbPerFrame, candidate.AvgGcKbPerFrame, false);
            return rows;
        }

        public static bool HasRegression(List<ComparisonRow> rows)
        {
            foreach (ComparisonRow row in rows)
            {
                if (row.Change == Change.Worse)
                {
                    return true;
                }
            }
            return false;
        }

        void Add(List<ComparisonRow> rows, string name, string unit, float baseline, float candidate, bool higherIsBetter)
        {
            var row = new ComparisonRow { Name = name, Unit = unit, Baseline = baseline, Candidate = candidate };

            if (baseline < 0f || candidate < 0f)
            {
                row.Change = Change.NotComparable;
                rows.Add(row);
                return;
            }

            row.DeltaPct = CalculateDeltaPct(baseline, candidate);
            bool worse = higherIsBetter ? row.DeltaPct < -ThresholdPct : row.DeltaPct > ThresholdPct;
            bool better = higherIsBetter ? row.DeltaPct > ThresholdPct : row.DeltaPct < -ThresholdPct;
            row.Change = worse ? Change.Worse : better ? Change.Better : Change.Same;
            rows.Add(row);
        }

        static float CalculateDeltaPct(float baseline, float candidate)
        {
            if (Math.Abs(baseline) < 0.0001f)
            {
                return Math.Abs(candidate) < 0.0001f ? 0f : 100f;
            }
            return (candidate - baseline) / Math.Abs(baseline) * 100f;
        }
    }
}

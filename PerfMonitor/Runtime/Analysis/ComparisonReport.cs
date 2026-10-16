using System.Collections.Generic;
using System.Text;

namespace PerfMonitor.Analysis
{
    // Turns comparison rows into plain text for the console, the window and CI logs
    public static class ComparisonReport
    {
        public static string Build(RecordingStats baseline, RecordingStats candidate, List<ComparisonRow> rows)
        {
            var text = new StringBuilder();
            text.AppendLine("Perf comparison");
            text.AppendLine($"Baseline:  {baseline.Name} ({baseline.Frames} frames, {baseline.DurationSeconds:0.0}s)");
            text.AppendLine($"Candidate: {candidate.Name} ({candidate.Frames} frames, {candidate.DurationSeconds:0.0}s)");
            text.AppendLine();
            text.AppendLine($"{"Metric",-22}{"Baseline",14}{"Candidate",14}{"Change",10}  Result");

            int worseCount = 0;
            foreach (ComparisonRow row in rows)
            {
                if (row.Change == Change.NotComparable)
                {
                    text.AppendLine($"{row.Name,-22}{"n/a",14}{"n/a",14}{"",10}  -");
                    continue;
                }
                if (row.Change == Change.Worse)
                {
                    worseCount++;
                }
                text.AppendLine($"{row.Name,-22}{Format(row.Baseline, row.Unit),14}{Format(row.Candidate, row.Unit),14}{row.DeltaPct,9:+0.0;-0.0;0.0}%  {row.Change}");
            }

            text.AppendLine();
            text.AppendLine(worseCount == 0 ? "RESULT: no regression" : $"RESULT: REGRESSION, {worseCount} metric(s) got worse");
            return text.ToString();
        }

        static string Format(float value, string unit)
        {
            return $"{value:0.#} {unit}";
        }
    }
}

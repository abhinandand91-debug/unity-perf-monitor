using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using PerfMonitor.Core;

namespace PerfMonitor.Analysis
{
    // Reads a CSV written by PerfRecorder and works out the statistics
    public static class RecordingLoader
    {
        const float DefaultBudgetMs = 1000f / 60f;

        public static RecordingStats Load(string path)
        {
            string[] lines = File.ReadAllLines(path);
            if (lines.Length < 2)
            {
                throw new InvalidDataException("Recording has no rows: " + path);
            }

            Dictionary<string, int> columns = ReadHeader(lines[0]);
            var frameMs = new List<float>();
            var fps = new List<float>();
            var mainMs = new List<float>();
            var renderMs = new List<float>();
            var gpuMs = new List<float>();
            var cpuPct = new List<float>();
            var gcBytes = new List<float>();
            float peakMemory = 0f;
            float firstTime = 0f;
            float lastTime = 0f;
            int overBudget = 0;

            for (int i = 1; i < lines.Length; i++)
            {
                if (lines[i].Length == 0)
                {
                    continue;
                }

                string[] cells = lines[i].Split(',');
                float time = Read(cells, columns, "time");
                float frame = Read(cells, columns, "frame_ms");
                float budget = Read(cells, columns, "budget_ms");
                if (budget <= 0f)
                {
                    budget = DefaultBudgetMs;
                }

                if (frameMs.Count == 0)
                {
                    firstTime = time;
                }
                lastTime = time;

                frameMs.Add(frame);
                fps.Add(Read(cells, columns, "fps"));
                mainMs.Add(Read(cells, columns, "main_thread_ms"));
                renderMs.Add(Read(cells, columns, "render_thread_ms"));
                gpuMs.Add(Read(cells, columns, "gpu_ms"));
                cpuPct.Add(Read(cells, columns, "cpu_process_pct"));
                gcBytes.Add(Read(cells, columns, "gc_alloc_bytes"));
                peakMemory = Math.Max(peakMemory, Read(cells, columns, "app_mem_mb"));

                if (FrameBudget.IsOverBudget(frame, budget))
                {
                    overBudget++;
                }
            }

            var sorted = new List<float>(frameMs);
            sorted.Sort();
            float p99 = Percentile(sorted, 0.99f);

            return new RecordingStats
            {
                Name = Path.GetFileNameWithoutExtension(path),
                Frames = frameMs.Count,
                DurationSeconds = lastTime - firstTime,
                AvgFps = AverageOf(fps),
                Low1PercentFps = p99 > 0f ? 1000f / p99 : 0f,
                FrameP95Ms = Percentile(sorted, 0.95f),
                FrameP99Ms = p99,
                OverBudgetPct = 100f * overBudget / frameMs.Count,
                AvgMainThreadMs = AverageOf(mainMs),
                AvgRenderThreadMs = AverageOf(renderMs),
                AvgGpuMs = AverageOf(gpuMs),
                AvgProcessCpuPct = AverageOf(cpuPct),
                PeakAppMemoryMB = peakMemory,
                AvgGcKbPerFrame = AverageOf(gcBytes) / 1024f,
            };
        }

        static Dictionary<string, int> ReadHeader(string headerLine)
        {
            var columns = new Dictionary<string, int>();
            string[] names = headerLine.Split(',');
            for (int i = 0; i < names.Length; i++)
            {
                columns[names[i].Trim()] = i;
            }
            return columns;
        }

        // Missing columns give -1, so old recordings still load
        static float Read(string[] cells, Dictionary<string, int> columns, string name)
        {
            if (!columns.TryGetValue(name, out int index) || index >= cells.Length)
            {
                return PerfSample.NotAvailable;
            }
            return float.TryParse(cells[index], NumberStyles.Float, CultureInfo.InvariantCulture, out float value)
                ? value
                : PerfSample.NotAvailable;
        }

        // Average that skips unavailable (negative) values
        static float AverageOf(List<float> values)
        {
            double sum = 0;
            int count = 0;
            foreach (float value in values)
            {
                if (value < 0f)
                {
                    continue;
                }
                sum += value;
                count++;
            }
            return count > 0 ? (float)(sum / count) : PerfSample.NotAvailable;
        }

        // Expects a sorted list
        static float Percentile(List<float> sorted, float fraction)
        {
            int index = Math.Max(0, (int)Math.Ceiling(sorted.Count * fraction) - 1);
            return sorted[Math.Min(index, sorted.Count - 1)];
        }
    }
}

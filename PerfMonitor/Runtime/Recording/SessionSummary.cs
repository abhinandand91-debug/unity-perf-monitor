using System.Globalization;
using System.Text;
using PerfMonitor.Core;
using UnityEngine;

namespace PerfMonitor.Recording
{
    // Collects totals while recording and turns them into a short text report
    public sealed class SessionSummary
    {
        int _frames;
        float _startTime;
        float _minFps;
        float _worstFrameMs;
        float _peakAppMemoryMB;
        float _peakManagedMB;
        long _gcBytes;
        double _sumFps;
        double _sumAppMemoryMB;
        readonly Average _cpu = new Average();
        readonly Average _mainThread = new Average();
        readonly Average _gpu = new Average();

        public void Reset()
        {
            _frames = 0;
            _startTime = Time.unscaledTime;
            _minFps = float.MaxValue;
            _worstFrameMs = 0f;
            _peakAppMemoryMB = 0f;
            _peakManagedMB = 0f;
            _gcBytes = 0;
            _sumFps = 0;
            _sumAppMemoryMB = 0;
            _cpu.Reset();
            _mainThread.Reset();
            _gpu.Reset();
        }

        public void Add(in PerfSample sample)
        {
            _frames++;
            _sumFps += sample.Fps;
            _sumAppMemoryMB += sample.AppMemoryMB;
            _gcBytes += sample.GcAllocBytes;

            if (sample.Fps > 0f)
            {
                _minFps = Mathf.Min(_minFps, sample.Fps);
            }
            _worstFrameMs = Mathf.Max(_worstFrameMs, sample.FrameMs);
            _peakAppMemoryMB = Mathf.Max(_peakAppMemoryMB, sample.AppMemoryMB);
            _peakManagedMB = Mathf.Max(_peakManagedMB, sample.ManagedUsedMB);

            _cpu.Add(sample.ProcessCpuPct);
            _mainThread.Add(sample.MainThreadPct);
            _gpu.Add(sample.GpuPct);
        }

        public string Build()
        {
            var culture = CultureInfo.InvariantCulture;
            var text = new StringBuilder();
            text.AppendLine("Perf Monitor session summary");
            text.AppendLine($"Device: {SystemInfo.deviceModel} | {SystemInfo.processorType} x{SystemInfo.processorCount} | {SystemInfo.graphicsDeviceName}");
            text.AppendLine($"Unity {Application.unityVersion} | {Application.platform} | RAM {SystemInfo.systemMemorySize} MB | VRAM {SystemInfo.graphicsMemorySize} MB");
            text.AppendLine($"Duration: {(Time.unscaledTime - _startTime).ToString("F1", culture)} s, frames: {_frames}");

            if (_frames == 0)
            {
                return text.ToString();
            }

            text.AppendLine($"FPS avg {(_sumFps / _frames).ToString("F1", culture)}, min {_minFps.ToString("F1", culture)}, worst frame {_worstFrameMs.ToString("F1", culture)} ms");
            text.AppendLine($"CPU process avg {_cpu.Text()} | main thread avg {_mainThread.Text()} | GPU avg {_gpu.Text()}");
            text.AppendLine($"App memory avg {(_sumAppMemoryMB / _frames).ToString("F0", culture)} MB, peak {_peakAppMemoryMB.ToString("F0", culture)} MB | managed peak {_peakManagedMB.ToString("F0", culture)} MB");
            text.AppendLine($"GC allocated total {(_gcBytes / 1024f).ToString("F0", culture)} KB");
            return text.ToString();
        }

        // Average that skips frames where the metric was not available
        sealed class Average
        {
            double _sum;
            int _count;

            public void Reset()
            {
                _sum = 0;
                _count = 0;
            }

            public void Add(float percent)
            {
                if (percent < 0f)
                {
                    return;
                }
                _sum += percent;
                _count++;
            }

            public string Text()
            {
                return _count > 0 ? (_sum / _count).ToString("F1", CultureInfo.InvariantCulture) + "%" : "n/a";
            }
        }
    }
}

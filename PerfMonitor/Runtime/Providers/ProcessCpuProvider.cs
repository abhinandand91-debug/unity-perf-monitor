using System;
using System.Diagnostics;
using PerfMonitor.Core;
using UnityEngine;

namespace PerfMonitor.Providers
{
    // Share of all CPU cores used by this process, measured over a short window
    public sealed class ProcessCpuProvider : IMetricProvider
    {
        const float WindowSeconds = 0.5f;

        readonly int _coreCount = Math.Max(1, SystemInfo.processorCount);

        float _windowStart;
        double _windowCpuSeconds;
        float _lastPercent = PerfSample.NotAvailable;
        bool _unsupported;

        public ProcessCpuProvider()
        {
            _windowStart = Time.realtimeSinceStartup;
            _windowCpuSeconds = ReadCpuSeconds();
            _unsupported = _windowCpuSeconds < 0;
        }

        public void Fill(ref PerfSample sample)
        {
            if (_unsupported)
            {
                return;
            }

            float now = Time.realtimeSinceStartup;
            if (now - _windowStart >= WindowSeconds)
            {
                double cpuSeconds = ReadCpuSeconds();
                double busySeconds = cpuSeconds - _windowCpuSeconds;
                double wallSeconds = (now - _windowStart) * _coreCount;
                _lastPercent = Mathf.Clamp((float)(busySeconds / wallSeconds * 100.0), 0f, 100f);
                _windowCpuSeconds = cpuSeconds;
                _windowStart = now;
            }

            sample.ProcessCpuPct = _lastPercent;
        }

        // Returns -1 when the platform does not allow reading process info
        static double ReadCpuSeconds()
        {
            try
            {
                using (Process process = Process.GetCurrentProcess())
                {
                    return process.TotalProcessorTime.TotalSeconds;
                }
            }
            catch (Exception)
            {
                return -1.0;
            }
        }

        public void Dispose()
        {
        }
    }
}

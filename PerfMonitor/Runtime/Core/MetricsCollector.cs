using System;
using System.Collections.Generic;
using PerfMonitor.Providers;

namespace PerfMonitor.Core
{
    // Runs every provider once per frame and returns the combined sample
    public sealed class MetricsCollector : IDisposable
    {
        readonly List<IMetricProvider> _providers;

        public MetricsCollector()
        {
            _providers = new List<IMetricProvider>
            {
                new FrameTimingProvider(),
                new ProcessCpuProvider(),
                new MemoryProvider(),
                new RenderStatsProvider(),
            };
        }

        public PerfSample Sample()
        {
            var sample = PerfSample.Create();
            for (int i = 0; i < _providers.Count; i++)
            {
                _providers[i].Fill(ref sample);
            }
            return sample;
        }

        public void Dispose()
        {
            for (int i = 0; i < _providers.Count; i++)
            {
                _providers[i].Dispose();
            }
        }
    }
}

using System;

namespace PerfMonitor.Core
{
    // Each provider measures one area (CPU, memory...) and writes it into the sample
    public interface IMetricProvider : IDisposable
    {
        void Fill(ref PerfSample sample);
    }
}

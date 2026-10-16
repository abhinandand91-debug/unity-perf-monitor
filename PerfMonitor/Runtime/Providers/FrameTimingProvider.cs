using PerfMonitor.Core;
using Unity.Profiling;
using UnityEngine;

namespace PerfMonitor.Providers
{
    // Main thread, render thread and GPU load from the FrameTimingManager
    public sealed class FrameTimingProvider : IMetricProvider
    {
        readonly FrameTiming[] _timings = new FrameTiming[1];

        // Used when FrameTimingManager gives no main thread time
        ProfilerRecorder _mainThreadRecorder;

        public FrameTimingProvider()
        {
            _mainThreadRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread", 15);
        }

        public void Fill(ref PerfSample sample)
        {
            FrameTimingManager.CaptureFrameTimings();
            uint count = FrameTimingManager.GetLatestTimings(1, _timings);
            bool hasTimings = count > 0 && _timings[0].cpuFrameTime > 0;

            // Percent is busy time compared to how long the frame really took
            float frameMs = Mathf.Max(sample.FrameMs, 0.01f);

            if (hasTimings)
            {
                FrameTiming timing = _timings[0];
#if UNITY_2022_2_OR_NEWER
                if (timing.cpuMainThreadFrameTime > 0)
                {
                    sample.MainThreadPct = PerfSample.ToPercent((float)timing.cpuMainThreadFrameTime, frameMs);
                }
                if (timing.cpuRenderThreadFrameTime > 0)
                {
                    sample.RenderThreadPct = PerfSample.ToPercent((float)timing.cpuRenderThreadFrameTime, frameMs);
                }
#endif
                if (timing.gpuFrameTime > 0)
                {
                    sample.GpuPct = PerfSample.ToPercent((float)timing.gpuFrameTime, frameMs);
                }
            }

            if (sample.MainThreadPct < 0f && _mainThreadRecorder.Valid && _mainThreadRecorder.LastValue > 0)
            {
                // Recorder value is in nanoseconds
                float mainMs = _mainThreadRecorder.LastValue / 1000000f;
                sample.MainThreadPct = PerfSample.ToPercent(mainMs, frameMs);
            }
        }

        public void Dispose()
        {
            _mainThreadRecorder.Dispose();
        }
    }
}

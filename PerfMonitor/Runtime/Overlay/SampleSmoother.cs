using PerfMonitor.Core;
using UnityEngine;

namespace PerfMonitor.Overlay
{
    // Smooths the fast changing values so the panel text does not flicker
    public sealed class SampleSmoother
    {
        const float SmoothTime = 0.25f;

        public PerfSample Current;

        public SampleSmoother()
        {
            Current = PerfSample.Create();
        }

        public void Update(in PerfSample latest)
        {
            float blend = 1f - Mathf.Exp(-Time.unscaledDeltaTime / SmoothTime);

            // Start from the latest values, then smooth the ones that jump around
            PerfSample previous = Current;
            Current = latest;
            Current.Fps = Mathf.Lerp(previous.Fps, latest.Fps, blend);
            Current.FrameMs = Mathf.Lerp(previous.FrameMs, latest.FrameMs, blend);
            Current.MainThreadMs = Smooth(previous.MainThreadMs, latest.MainThreadMs, blend);
            Current.RenderThreadMs = Smooth(previous.RenderThreadMs, latest.RenderThreadMs, blend);
            Current.GpuMs = Smooth(previous.GpuMs, latest.GpuMs, blend);
            Current.MainThreadPct = Smooth(previous.MainThreadPct, latest.MainThreadPct, blend);
            Current.RenderThreadPct = Smooth(previous.RenderThreadPct, latest.RenderThreadPct, blend);
            Current.GpuPct = Smooth(previous.GpuPct, latest.GpuPct, blend);
        }

        static float Smooth(float previous, float target, float blend)
        {
            if (target < 0f)
            {
                return PerfSample.NotAvailable;
            }
            return previous < 0f ? target : Mathf.Lerp(previous, target, blend);
        }
    }
}

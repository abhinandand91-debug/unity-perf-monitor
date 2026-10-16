using UnityEngine;

namespace PerfMonitor.Core
{
    // One frame of measurements
    public struct PerfSample
    {
        // Value used for any metric the platform cannot give us
        public const float NotAvailable = -1f;

        public float Time;
        public float FrameMs;
        public float Fps;

        // Time budget for this frame, from the target FPS
        public float BudgetMs;

        // Busy time in milliseconds, NotAvailable when unsupported
        public float MainThreadMs;
        public float RenderThreadMs;
        public float GpuMs;

        // Percent values, NotAvailable when unsupported
        public float ProcessCpuPct;
        public float MainThreadPct;
        public float RenderThreadPct;
        public float GpuPct;
        public float AppMemoryPct;
        public float ManagedHeapPct;
        public float GfxMemoryPct;

        // Memory in MB
        public float AppMemoryMB;
        public float ManagedUsedMB;
        public float GfxMemoryMB;

        // Per frame counters
        public long GcAllocBytes;
        public long DrawCalls;
        public long SetPassCalls;
        public long Triangles;
        public long Vertices;

        // Makes a sample with the timing filled in and every optional metric marked unavailable
        public static PerfSample Create()
        {
            var sample = new PerfSample
            {
                Time = UnityEngine.Time.unscaledTime,
                FrameMs = UnityEngine.Time.unscaledDeltaTime * 1000f,
                BudgetMs = FrameBudget.Ms,
                MainThreadMs = NotAvailable,
                RenderThreadMs = NotAvailable,
                GpuMs = NotAvailable,
                ProcessCpuPct = NotAvailable,
                MainThreadPct = NotAvailable,
                RenderThreadPct = NotAvailable,
                GpuPct = NotAvailable,
                AppMemoryPct = NotAvailable,
                ManagedHeapPct = NotAvailable,
                GfxMemoryPct = NotAvailable,
            };
            sample.Fps = sample.FrameMs > 0f ? 1000f / sample.FrameMs : 0f;
            return sample;
        }

        public static float ToPercent(float part, float whole)
        {
            return whole <= 0f ? NotAvailable : Mathf.Clamp(part / whole * 100f, 0f, 100f);
        }
    }
}

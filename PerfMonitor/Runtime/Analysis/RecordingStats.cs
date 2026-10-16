namespace PerfMonitor.Analysis
{
    // Numbers worked out from one recorded CSV. A value of -1 means the recording had no data for it
    public sealed class RecordingStats
    {
        public string Name;
        public int Frames;
        public float DurationSeconds;

        public float AvgFps;
        public float Low1PercentFps;
        public float FrameP95Ms;
        public float FrameP99Ms;
        public float OverBudgetPct;

        public float AvgMainThreadMs;
        public float AvgRenderThreadMs;
        public float AvgGpuMs;
        public float AvgProcessCpuPct;

        public float PeakAppMemoryMB;
        public float AvgGcKbPerFrame;
    }
}

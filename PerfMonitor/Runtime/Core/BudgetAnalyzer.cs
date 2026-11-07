namespace PerfMonitor.Core
{
    public enum BudgetStatus
    {
        WithinBudget,
        MainThreadBound,
        RenderThreadBound,
        GpuBound,
        MemoryPressure,
        Stalled,
        Unknown,
    }

    // Result of comparing the frame against its time budget
    public struct BudgetReport
    {
        public BudgetStatus Status;
        public string Text;

        // Busy time as a percent of the budget, can go above 100. NotAvailable when unsupported
        public float MainBudgetPct;
        public float RenderBudgetPct;
        public float GpuBudgetPct;

        public bool IsHealthy => Status == BudgetStatus.WithinBudget;
    }

    // Works out what is holding the frame back
    public static class BudgetAnalyzer
    {
        // A unit counts as the bottleneck when it uses at least this much of the budget
        const float BottleneckShare = 0.9f;
        const float MemoryPressurePct = 90f;

        public static BudgetReport Analyze(in PerfSample sample)
        {
            var report = new BudgetReport
            {
                MainBudgetPct = ToBudgetPercent(sample.MainThreadMs, sample.BudgetMs),
                RenderBudgetPct = ToBudgetPercent(sample.RenderThreadMs, sample.BudgetMs),
                GpuBudgetPct = ToBudgetPercent(sample.GpuMs, sample.BudgetMs),
            };

            if (sample.AppMemoryPct >= MemoryPressurePct)
            {
                return SetStatus(report, BudgetStatus.MemoryPressure, "Memory pressure");
            }

            if (!FrameBudget.IsOverBudget(sample.FrameMs, sample.BudgetMs))
            {
                return SetStatus(report, BudgetStatus.WithinBudget, "Within budget");
            }

            // Frame is too slow, find the slowest unit
            float worstMs = System.Math.Max(sample.MainThreadMs, System.Math.Max(sample.RenderThreadMs, sample.GpuMs));
            if (worstMs < 0f)
            {
                return SetStatus(report, BudgetStatus.Unknown, "Over budget (no timing data)");
            }
            if (worstMs < sample.BudgetMs * BottleneckShare)
            {
                return SetStatus(report, BudgetStatus.Stalled, "Over budget, waiting (vsync or stall)");
            }

            if (worstMs == sample.GpuMs)
            {
                return SetStatus(report, BudgetStatus.GpuBound, "GPU bound");
            }
            if (worstMs == sample.RenderThreadMs)
            {
                return SetStatus(report, BudgetStatus.RenderThreadBound, "Render thread bound");
            }
            return SetStatus(report, BudgetStatus.MainThreadBound, "Main thread bound (CPU)");
        }

        static BudgetReport SetStatus(BudgetReport report, BudgetStatus status, string text)
        {
            report.Status = status;
            report.Text = text;
            return report;
        }

        static float ToBudgetPercent(float busyMs, float budgetMs)
        {
            if (busyMs < 0f || budgetMs <= 0f)
            {
                return PerfSample.NotAvailable;
            }
            return busyMs / budgetMs * 100f;
        }
    }
}

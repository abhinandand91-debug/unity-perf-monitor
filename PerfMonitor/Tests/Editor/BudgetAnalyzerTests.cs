using NUnit.Framework;
using PerfMonitor.Core;

namespace PerfMonitor.Tests
{
    public class BudgetAnalyzerTests
    {
        const float Budget60 = 1000f / 60f;

        static PerfSample Frame(float frameMs, float mainMs, float renderMs, float gpuMs, float memoryPct = 40f)
        {
            return new PerfSample
            {
                FrameMs = frameMs,
                BudgetMs = Budget60,
                MainThreadMs = mainMs,
                RenderThreadMs = renderMs,
                GpuMs = gpuMs,
                AppMemoryPct = memoryPct,
            };
        }

        [Test]
        public void FastFrame_IsWithinBudget()
        {
            Assert.AreEqual(BudgetStatus.WithinBudget, BudgetAnalyzer.Analyze(Frame(16f, 9f, 3f, 8f)).Status);
        }

        [Test]
        public void SlowGpu_IsGpuBound()
        {
            Assert.AreEqual(BudgetStatus.GpuBound, BudgetAnalyzer.Analyze(Frame(25f, 8f, 3f, 24f)).Status);
        }

        [Test]
        public void SlowMainThread_IsMainThreadBound()
        {
            Assert.AreEqual(BudgetStatus.MainThreadBound, BudgetAnalyzer.Analyze(Frame(25f, 24f, 3f, 8f)).Status);
        }

        [Test]
        public void SlowRenderThread_IsRenderThreadBound()
        {
            Assert.AreEqual(BudgetStatus.RenderThreadBound, BudgetAnalyzer.Analyze(Frame(25f, 8f, 24f, 8f)).Status);
        }

        [Test]
        public void SlowFrameWithIdleUnits_IsStalled()
        {
            Assert.AreEqual(BudgetStatus.Stalled, BudgetAnalyzer.Analyze(Frame(33f, 5f, 2f, 5f)).Status);
        }

        [Test]
        public void SlowFrameWithoutTimings_IsUnknown()
        {
            var na = PerfSample.NotAvailable;
            Assert.AreEqual(BudgetStatus.Unknown, BudgetAnalyzer.Analyze(Frame(33f, na, na, na)).Status);
        }

        [Test]
        public void HighMemory_WinsOverEverything()
        {
            Assert.AreEqual(BudgetStatus.MemoryPressure, BudgetAnalyzer.Analyze(Frame(25f, 8f, 3f, 24f, 95f)).Status);
        }

        [Test]
        public void BudgetPercent_CanGoAbove100()
        {
            var report = BudgetAnalyzer.Analyze(Frame(25f, 8f, 3f, 25f));
            Assert.AreEqual(150f, report.GpuBudgetPct, 0.1f);
        }

        [Test]
        public void SlightlyOver_IsStillWithinTolerance()
        {
            Assert.IsFalse(FrameBudget.IsOverBudget(Budget60 * 1.05f, Budget60));
            Assert.IsTrue(FrameBudget.IsOverBudget(Budget60 * 1.2f, Budget60));
        }
    }
}

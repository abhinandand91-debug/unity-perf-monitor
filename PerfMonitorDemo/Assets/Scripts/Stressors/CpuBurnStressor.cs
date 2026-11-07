using System.Diagnostics;

namespace PerfMonitorDemo.Stressors
{
    // Keeps the main thread busy for a set time every frame
    public sealed class CpuBurnStressor : IStressor
    {
        public string Name => "CPU burn";

        // Milliseconds to burn per frame, set from the slider
        public float BurnMs;

        public void Tick()
        {
            if (BurnMs <= 0f)
            {
                return;
            }

            var timer = Stopwatch.StartNew();
            double value = 0;
            while (timer.Elapsed.TotalMilliseconds < BurnMs)
            {
                value += System.Math.Sqrt(value + 1.0);
            }

            // Using the result stops the loop from being optimised away
            if (value < 0)
            {
                UnityEngine.Debug.Log(value);
            }
        }

        public void Reset()
        {
            BurnMs = 0f;
        }
    }
}

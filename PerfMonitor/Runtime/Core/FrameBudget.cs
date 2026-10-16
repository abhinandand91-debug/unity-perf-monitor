using UnityEngine;

namespace PerfMonitor.Core
{
    // The time one frame is allowed to take, based on the target FPS
    public static class FrameBudget
    {
        // A frame counts as over budget only when it is more than 10% too slow
        public const float OverBudgetTolerance = 1.1f;

        const float FallbackFps = 60f;

        static readonly int[] Presets = { 0, 30, 60, 90, 120 };
        static int _presetIndex;

        // 0 means use the game's own target (Application.targetFrameRate or the screen refresh rate)
        public static int TargetFpsOverride { get; private set; }

        public static float TargetFps
        {
            get
            {
                if (TargetFpsOverride > 0)
                {
                    return TargetFpsOverride;
                }
                if (Application.targetFrameRate > 0)
                {
                    return Application.targetFrameRate;
                }
                float refresh = GetRefreshRate();
                return refresh > 0f ? refresh : FallbackFps;
            }
        }

        public static float Ms => 1000f / TargetFps;

        public static bool IsAuto => TargetFpsOverride == 0;

        // Goes auto, 30, 60, 90, 120 and back to auto
        public static void CyclePreset()
        {
            _presetIndex = (_presetIndex + 1) % Presets.Length;
            TargetFpsOverride = Presets[_presetIndex];
        }

        public static bool IsOverBudget(float frameMs, float budgetMs)
        {
            return frameMs > budgetMs * OverBudgetTolerance;
        }

        static float GetRefreshRate()
        {
#if UNITY_2022_2_OR_NEWER
            return (float)Screen.currentResolution.refreshRateRatio.value;
#else
            return Screen.currentResolution.refreshRate;
#endif
        }
    }
}

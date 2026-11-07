using System.Collections.Generic;
using PerfMonitorDemo.Stressors;
using UnityEngine;

namespace PerfMonitorDemo
{
    // Draws the buttons and sliders on the right side and runs the stressors
    public sealed class StressTestPanel : MonoBehaviour
    {
        const float PanelWidth = 230f;
        const float PanelHeight = 330f;
        const float RowHeight = 28f;

        readonly CubeStressor _cubes = new CubeStressor();
        readonly SphereStressor _spheres = new SphereStressor();
        readonly MemoryStressor _memory = new MemoryStressor();
        readonly CpuBurnStressor _cpuBurn = new CpuBurnStressor();
        readonly GarbageStressor _garbage = new GarbageStressor();

        OverdrawStressor _overdraw;
        List<IStressor> _all;

        void Start()
        {
            _overdraw = new OverdrawStressor();
            _all = new List<IStressor> { _cubes, _spheres, _overdraw, _memory, _cpuBurn, _garbage };
        }

        void Update()
        {
            foreach (IStressor stressor in _all)
            {
                stressor.Tick();
            }
        }

        void OnGUI()
        {
            float scale = Mathf.Max(1f, Mathf.Min(Screen.width, Screen.height) / 540f);
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));

            float panelX = Screen.width / scale - PanelWidth - 8f;
            GUI.Box(new Rect(panelX, 8f, PanelWidth, PanelHeight), "Stress test");

            float x = panelX + 8f;
            float width = PanelWidth - 16f;
            float y = 32f;

            if (GUI.Button(new Rect(x, y, width, 24f), $"Drop 100 physics cubes ({_cubes.Count})"))
            {
                _cubes.Spawn();
            }
            y += RowHeight;

            if (GUI.Button(new Rect(x, y, width, 24f), $"Add 100 spheres - GPU/draws ({_spheres.Count})"))
            {
                _spheres.Spawn();
            }
            y += RowHeight;

            if (GUI.Button(new Rect(x, y, width, 24f), $"Add 10 overdraw layers ({_overdraw.Count})"))
            {
                _overdraw.Spawn();
            }
            y += RowHeight;

            if (GUI.Button(new Rect(x, y, width, 24f), $"Allocate 100 MB ({_memory.AllocatedMB} MB)"))
            {
                _memory.Allocate();
            }
            y += RowHeight + 4f;

            _cpuBurn.BurnMs = DrawSlider(x, y, width, $"CPU burn: {_cpuBurn.BurnMs:0} ms/frame", _cpuBurn.BurnMs, 30f);
            y += 42f;

            _garbage.KilobytesPerFrame = DrawSlider(x, y, width, $"GC garbage: {_garbage.KilobytesPerFrame:0} KB/frame", _garbage.KilobytesPerFrame, 2000f);
            y += 46f;

            if (GUI.Button(new Rect(x, y, width, 26f), "Reset everything"))
            {
                ResetAll();
            }
            y += 32f;

            GUI.Label(new Rect(x, y, width, 36f), "F1 overlay, F2 record. Recordings: Tools > Perf Monitor > Open Recordings Folder.");
        }

        static float DrawSlider(float x, float y, float width, string label, float value, float max)
        {
            GUI.Label(new Rect(x, y, width, 18f), label);
            return GUI.HorizontalSlider(new Rect(x, y + 20f, width, 16f), value, 0f, max);
        }

        void ResetAll()
        {
            foreach (IStressor stressor in _all)
            {
                stressor.Reset();
            }
        }
    }
}

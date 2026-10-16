using PerfMonitor.Core;
using PerfMonitor.Recording;
using UnityEngine;

namespace PerfMonitor.Overlay
{
    // The component that ties everything together: samples each frame, records, and draws the panel.
    // F1 shows or hides the panel, F2 starts or stops recording.
    public sealed class PerfOverlay : MonoBehaviour
    {
        const int HistoryLength = 240;
        const float StatsInterval = 0.5f;
        const float Margin = 8f;
        const float PanelWidth = 250f;
        const float PanelHeight = 292f;

        static PerfOverlay _instance;

        public bool Expanded = true;

        readonly PerfRecorder _recorder = new PerfRecorder();
        readonly FrameHistory _history = new FrameHistory(HistoryLength);
        readonly SampleSmoother _smoother = new SampleSmoother();

        MetricsCollector _collector;
        OverlayDrawer _drawer;
        PerfSample _latest;
        float _low1PercentFps;
        float _nextStatsTime;

        // Starts itself in the Editor and Development builds
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AutoStart()
        {
            if (Application.isEditor || Debug.isDebugBuild)
            {
                Create();
            }
        }

        // Call this yourself if you want the overlay in a release build
        public static PerfOverlay Create()
        {
            if (_instance != null)
            {
                return _instance;
            }

            var go = new GameObject("PerfMonitor") { hideFlags = HideFlags.DontSave };
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<PerfOverlay>();
            return _instance;
        }

        void Awake()
        {
            _collector = new MetricsCollector();
            _drawer = new OverlayDrawer();
            _latest = PerfSample.Create();
        }

        void OnDestroy()
        {
            _recorder.Stop();
            _collector.Dispose();
            _drawer.Destroy();
        }

        void OnApplicationQuit()
        {
            _recorder.Stop();
        }

        void Update()
        {
            HandleHotkeys();

            _latest = _collector.Sample();
            _recorder.Add(_latest);
            _history.Add(_latest.FrameMs);
            _smoother.Update(_latest);

            if (Time.unscaledTime >= _nextStatsTime)
            {
                _nextStatsTime = Time.unscaledTime + StatsInterval;
                _low1PercentFps = _history.GetLow1PercentFps();
            }
        }

        void HandleHotkeys()
        {
            // Only works with the old input system, the on screen buttons work with both
#if ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetKeyDown(KeyCode.F1))
            {
                Expanded = !Expanded;
            }
            if (Input.GetKeyDown(KeyCode.F2))
            {
                ToggleRecording();
            }
#endif
        }

        public void ToggleRecording()
        {
            if (_recorder.IsRecording)
            {
                _recorder.Stop();
            }
            else
            {
                _recorder.Start();
            }
        }

        void OnGUI()
        {
            _drawer.BeginFrame();

            if (!Expanded)
            {
                if (_drawer.Button(new Rect(Margin, Margin, 56f, 24f), "PERF"))
                {
                    Expanded = true;
                }
                return;
            }

            DrawPanel();
        }

        void DrawPanel()
        {
            PerfSample shown = _smoother.Current;
            float left = Margin + 8f;
            float width = PanelWidth - 16f;
            float y = Margin + 6f;

            _drawer.FillRect(new Rect(Margin, Margin, PanelWidth, PanelHeight), new Color(0f, 0f, 0f, 0.72f));

            // Header
            _drawer.BigLabel(new Rect(left, y, 120f, 30f), $"{shown.Fps:0} FPS");
            _drawer.Label(new Rect(left + 100f, y + 3f, width - 100f, 20f), $"{shown.FrameMs:0.0} ms   1% low {_low1PercentFps:0}");
            if (_drawer.Button(new Rect(Margin + PanelWidth - 30f, y, 22f, 20f), "x"))
            {
                Expanded = false;
            }
            y += 30f;

            // Load bars
            y = _drawer.DrawBar(left, y, width, "CPU (process)", shown.ProcessCpuPct, "");
            y = _drawer.DrawBar(left, y, width, "Main thread", shown.MainThreadPct, "");
            y = _drawer.DrawBar(left, y, width, "Render thread", shown.RenderThreadPct, "");
            y = _drawer.DrawBar(left, y, width, "GPU", shown.GpuPct, "");
            y = _drawer.DrawBar(left, y, width, "RAM (app / device)", shown.AppMemoryPct, $"{shown.AppMemoryMB:0} MB");
            y = _drawer.DrawBar(left, y, width, "Managed heap", shown.ManagedHeapPct, $"{shown.ManagedUsedMB:0} MB");
            y = _drawer.DrawBar(left, y, width, "GPU memory", shown.GfxMemoryPct, $"{shown.GfxMemoryMB:0} MB");

            // Counters and graph
            _drawer.Label(new Rect(left, y, width, 18f), BuildCounterText(shown));
            y += 20f;
            _drawer.DrawFrameGraph(new Rect(left, y, width, 36f), _history);
            y += 42f;

            DrawRecordRow(left, y, width);
        }

        static string BuildCounterText(in PerfSample shown)
        {
            if (shown.DrawCalls > 0)
            {
                return $"Draw {shown.DrawCalls}  SetPass {shown.SetPassCalls}  Tris {shown.Triangles / 1000f:0.#}k  GC {shown.GcAllocBytes / 1024f:0.#} KB/f";
            }
            return $"GC alloc {shown.GcAllocBytes / 1024f:0.#} KB/frame";
        }

        void DrawRecordRow(float x, float y, float width)
        {
            string buttonText = _recorder.IsRecording ? "STOP REC" : "REC";
            if (_drawer.Button(new Rect(x, y, 80f, 22f), buttonText))
            {
                ToggleRecording();
            }

            var statusRect = new Rect(x + 88f, y + 2f, width - 88f, 18f);
            if (_recorder.IsRecording)
            {
                _drawer.Label(statusRect, "recording...");
            }
            else if (_recorder.LastFileName != null)
            {
                _drawer.Label(statusRect, "saved: " + _recorder.LastFileName);
            }
        }
    }
}

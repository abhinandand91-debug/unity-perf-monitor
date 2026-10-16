# Unity Perf Monitor

A small in-game performance overlay for Unity, in the spirit of GameBench. It shows FPS, CPU, GPU and memory load as percentages while the game runs, and can record a session to CSV.

- `PerfMonitor/` - the package (Unity 2021.3+ and Unity 6, no dependencies)
- `PerfMonitorDemo/` - a Unity 6 demo project with a stress-test panel

## Try the demo
1. Open `PerfMonitorDemo` in Unity 6000.3 (or newer).
2. Open `Assets/Scenes/Demo.unity` and press Play.
3. Use the buttons on the right to load the CPU, GPU, memory and GC, and watch the overlay react.

## Use it in your own project
Add this to `Packages/manifest.json` (adjust the path):

```
"com.local.perfmonitor": "file:../path/to/PerfMonitor"
```

The overlay starts itself in the Editor and Development builds. Release builds: call `PerfMonitor.Overlay.PerfOverlay.Create()`.

Controls: **F1** show/hide, **F2** start/stop recording (or use the on-screen buttons). For GPU numbers turn on **Tools > Perf Monitor > Enable Frame Timing Stats** (builds do this automatically).

## Frame budget verdict
The overlay compares the main thread, render thread and GPU against the frame budget of your target FPS (the **Target** button cycles Auto / 30 / 60 / 90 / 120) and names the bottleneck: `GPU bound`, `Main thread bound (CPU)`, `Render thread bound`, `Memory pressure`, `Over budget, waiting (vsync or stall)` or `Within budget`. The line under it shows each unit as a percent of the budget, which can go above 100%.

## Compare two builds
Record a session in each build (REC button), then:
- **Editor:** Tools > Perf Monitor > Compare Recordings, pick the two CSV files. Metrics are marked Better / Same / Worse using a noise threshold.
- **CI:** exits with code 1 when the new build regressed.

```
Unity -batchmode -quit -projectPath . -executeMethod PerfMonitor.Editor.PerfCompareCommand.Run -baseline old.csv -candidate new.csv -threshold 5
```
`SampleRecordings/` has two made-up CSVs to try this with. They are synthetic, not real measurements.

## What is measured
| Metric | Source |
|---|---|
| CPU (process) | OS process CPU time / (wall time x cores) |
| Main / render thread, GPU | FrameTimingManager busy time / frame time (render thread needs 2022.2+) |
| RAM | ProfilerRecorder "System Used Memory" / device RAM |
| Managed heap | GC used / GC reserved |
| GPU memory | "Gfx Used Memory" / graphics memory size |
| Draw calls, SetPass, triangles | ProfilerRecorder render counters |

A metric shows `n/a` when the platform does not provide it (for example GPU time on some devices, or process CPU on iOS and WebGL).

## Code layout
```
PerfMonitor/Runtime
  Core/        PerfSample, IMetricProvider, MetricsCollector, FrameBudget, BudgetAnalyzer
  Providers/   FrameTiming, ProcessCpu, Memory, RenderStats (one IMetricProvider each)
  Recording/   PerfRecorder, CsvWriter, SessionSummary
  Analysis/    RecordingLoader, RecordingStats, RecordingComparer, ComparisonReport
  Overlay/     PerfOverlay (entry point), OverlayDrawer, FrameHistory, SampleSmoother
PerfMonitor/Editor   menu items, build hook, compare window, CI command
```
To add a metric, write a new `IMetricProvider` and add it to `MetricsCollector`.

## Limits
It measures from inside the game. It cannot see other apps, true hardware GPU utilisation, thermals or power draw; use Xcode Instruments or Android Perfetto for those.

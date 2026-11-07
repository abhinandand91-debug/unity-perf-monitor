using PerfMonitor.Core;
using Unity.Profiling;

namespace PerfMonitor.Providers
{
    // Draw calls, set pass calls, triangles and vertices
    public sealed class RenderStatsProvider : IMetricProvider
    {
        ProfilerRecorder _drawCalls;
        ProfilerRecorder _setPassCalls;
        ProfilerRecorder _triangles;
        ProfilerRecorder _vertices;

        public RenderStatsProvider()
        {
            _drawCalls = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Draw Calls Count");
            _setPassCalls = ProfilerRecorder.StartNew(ProfilerCategory.Render, "SetPass Calls Count");
            _triangles = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Triangles Count");
            _vertices = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Vertices Count");
        }

        public void Fill(ref PerfSample sample)
        {
            sample.DrawCalls = ReadValue(_drawCalls);
            sample.SetPassCalls = ReadValue(_setPassCalls);
            sample.Triangles = ReadValue(_triangles);
            sample.Vertices = ReadValue(_vertices);
        }

        static long ReadValue(ProfilerRecorder recorder)
        {
            return recorder.Valid ? recorder.LastValue : 0;
        }

        public void Dispose()
        {
            _drawCalls.Dispose();
            _setPassCalls.Dispose();
            _triangles.Dispose();
            _vertices.Dispose();
        }
    }
}

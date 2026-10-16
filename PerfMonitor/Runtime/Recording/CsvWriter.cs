using System;
using System.Globalization;
using System.IO;
using System.Text;
using PerfMonitor.Core;

namespace PerfMonitor.Recording
{
    // Writes one CSV row per frame
    public sealed class CsvWriter : IDisposable
    {
        const string Header = "time,fps,frame_ms,cpu_process_pct,main_thread_pct,render_thread_pct,gpu_pct," +
                              "app_mem_mb,app_mem_pct,managed_mb,managed_heap_pct,gfx_mem_mb,gfx_mem_pct," +
                              "gc_alloc_bytes,draw_calls,setpass_calls,triangles,vertices," +
                              "main_thread_ms,render_thread_ms,gpu_ms,budget_ms";

        readonly StreamWriter _writer;
        readonly StringBuilder _line = new StringBuilder(256);

        public CsvWriter(string path)
        {
            _writer = new StreamWriter(path, false, Encoding.UTF8);
            _writer.WriteLine(Header);
        }

        public void WriteRow(in PerfSample s)
        {
            _line.Clear();
            Append(s.Time, "F3");
            Append(s.Fps, "F1");
            Append(s.FrameMs, "F2");
            Append(s.ProcessCpuPct, "F1");
            Append(s.MainThreadPct, "F1");
            Append(s.RenderThreadPct, "F1");
            Append(s.GpuPct, "F1");
            Append(s.AppMemoryMB, "F1");
            Append(s.AppMemoryPct, "F1");
            Append(s.ManagedUsedMB, "F1");
            Append(s.ManagedHeapPct, "F1");
            Append(s.GfxMemoryMB, "F1");
            Append(s.GfxMemoryPct, "F1");
            Append(s.GcAllocBytes);
            Append(s.DrawCalls);
            Append(s.SetPassCalls);
            Append(s.Triangles);
            Append(s.Vertices);
            Append(s.MainThreadMs, "F2");
            Append(s.RenderThreadMs, "F2");
            Append(s.GpuMs, "F2");
            Append(s.BudgetMs, "F2", false);
            _writer.WriteLine(_line.ToString());
        }

        void Append(float value, string format, bool comma = true)
        {
            _line.Append(value.ToString(format, CultureInfo.InvariantCulture));
            if (comma)
            {
                _line.Append(',');
            }
        }

        void Append(long value, bool comma = true)
        {
            _line.Append(value.ToString(CultureInfo.InvariantCulture));
            if (comma)
            {
                _line.Append(',');
            }
        }

        public void Dispose()
        {
            _writer.Dispose();
        }
    }
}

using System;
using System.Diagnostics;
using PerfMonitor.Core;
using Unity.Profiling;
using UnityEngine;

namespace PerfMonitor.Providers
{
    // App RAM, managed heap, graphics memory and GC allocations
    public sealed class MemoryProvider : IMetricProvider
    {
        const float BytesPerMB = 1024f * 1024f;

        ProfilerRecorder _systemUsed;
        ProfilerRecorder _gfxUsed;
        ProfilerRecorder _gcAllocated;
        ProfilerRecorder _gcUsed;
        ProfilerRecorder _gcReserved;

        public MemoryProvider()
        {
            _systemUsed = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "System Used Memory");
            _gfxUsed = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Gfx Used Memory");
            _gcAllocated = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame");
            _gcUsed = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Used Memory");
            _gcReserved = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Reserved Memory");
        }

        public void Fill(ref PerfSample sample)
        {
            FillAppMemory(ref sample);
            FillManagedHeap(ref sample);
            FillGraphicsMemory(ref sample);

            if (_gcAllocated.Valid)
            {
                sample.GcAllocBytes = _gcAllocated.LastValue;
            }
        }

        void FillAppMemory(ref PerfSample sample)
        {
            long bytes = _systemUsed.Valid ? _systemUsed.LastValue : 0;
            if (bytes <= 0)
            {
                bytes = ReadWorkingSetBytes();
            }
            if (bytes <= 0)
            {
                return;
            }

            sample.AppMemoryMB = bytes / BytesPerMB;
            // systemMemorySize is already in MB
            sample.AppMemoryPct = PerfSample.ToPercent(sample.AppMemoryMB, SystemInfo.systemMemorySize);
        }

        void FillManagedHeap(ref PerfSample sample)
        {
            long usedBytes = _gcUsed.Valid ? _gcUsed.LastValue : GC.GetTotalMemory(false);
            sample.ManagedUsedMB = usedBytes / BytesPerMB;

            if (_gcReserved.Valid)
            {
                sample.ManagedHeapPct = PerfSample.ToPercent(usedBytes, _gcReserved.LastValue);
            }
        }

        void FillGraphicsMemory(ref PerfSample sample)
        {
            if (!_gfxUsed.Valid)
            {
                return;
            }

            sample.GfxMemoryMB = _gfxUsed.LastValue / BytesPerMB;
            sample.GfxMemoryPct = PerfSample.ToPercent(sample.GfxMemoryMB, SystemInfo.graphicsMemorySize);
        }

        static long ReadWorkingSetBytes()
        {
            try
            {
                using (Process process = Process.GetCurrentProcess())
                {
                    return process.WorkingSet64;
                }
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public void Dispose()
        {
            _systemUsed.Dispose();
            _gfxUsed.Dispose();
            _gcAllocated.Dispose();
            _gcUsed.Dispose();
            _gcReserved.Dispose();
        }
    }
}

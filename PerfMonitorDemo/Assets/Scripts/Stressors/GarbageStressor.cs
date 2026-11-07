using System.Collections.Generic;
using UnityEngine;

namespace PerfMonitorDemo.Stressors
{
    // Allocates new arrays every frame so the garbage collector has work to do
    public sealed class GarbageStressor : IStressor
    {
        readonly List<byte[]> _sink = new List<byte[]>();

        public string Name => "GC garbage";

        // Kilobytes to allocate per frame, set from the slider
        public float KilobytesPerFrame;

        public void Tick()
        {
            _sink.Clear();
            int count = Mathf.CeilToInt(KilobytesPerFrame);
            for (int i = 0; i < count; i++)
            {
                _sink.Add(new byte[1024]);
            }
        }

        public void Reset()
        {
            KilobytesPerFrame = 0f;
            _sink.Clear();
        }
    }
}

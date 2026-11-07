using System;
using UnityEngine;

namespace PerfMonitor.Overlay
{
    // Ring buffer of the latest frame times, used for the graph and the 1% low FPS
    public sealed class FrameHistory
    {
        readonly float[] _frameMs;
        readonly float[] _sortBuffer;
        int _count;
        int _head;

        public FrameHistory(int capacity)
        {
            _frameMs = new float[capacity];
            _sortBuffer = new float[capacity];
        }

        public int Capacity => _frameMs.Length;
        public int Count => _count;

        public void Add(float frameMs)
        {
            _frameMs[_head] = frameMs;
            _head = (_head + 1) % _frameMs.Length;
            _count = Mathf.Min(_count + 1, _frameMs.Length);
        }

        // Index 0 is the oldest frame still stored
        public float GetFromOldest(int index)
        {
            int slot = (_head - _count + index + _frameMs.Length) % _frameMs.Length;
            return _frameMs[slot];
        }

        // FPS of the slowest 1% of stored frames
        public float GetLow1PercentFps()
        {
            if (_count < 10)
            {
                return 0f;
            }

            Array.Copy(_frameMs, _sortBuffer, _count);
            Array.Sort(_sortBuffer, 0, _count);
            int worstIndex = Mathf.Clamp(Mathf.CeilToInt(_count * 0.99f) - 1, 0, _count - 1);
            float worstMs = _sortBuffer[worstIndex];
            return worstMs > 0f ? 1000f / worstMs : 0f;
        }
    }
}

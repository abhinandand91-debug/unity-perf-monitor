using System.Collections.Generic;

namespace PerfMonitorDemo.Stressors
{
    // Holds on to big byte arrays so app memory goes up
    public sealed class MemoryStressor : IStressor
    {
        const int BlockSizeMB = 100;
        const int PageSize = 4096;

        readonly List<byte[]> _blocks = new List<byte[]>();

        public string Name => "Memory";
        public int AllocatedMB => _blocks.Count * BlockSizeMB;

        public void Allocate()
        {
            var block = new byte[BlockSizeMB * 1024 * 1024];

            // Write to every page, otherwise the OS may not count the memory as used
            for (int i = 0; i < block.Length; i += PageSize)
            {
                block[i] = 1;
            }
            _blocks.Add(block);
        }

        public void Tick()
        {
        }

        public void Reset()
        {
            _blocks.Clear();
            System.GC.Collect();
        }
    }
}

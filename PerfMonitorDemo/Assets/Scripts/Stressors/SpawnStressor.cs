using System.Collections.Generic;
using UnityEngine;

namespace PerfMonitorDemo.Stressors
{
    // Base class for stressors that create GameObjects, it keeps track of them so Reset can remove them
    public abstract class SpawnStressor : IStressor
    {
        protected readonly List<GameObject> Spawned = new List<GameObject>();

        public abstract string Name { get; }
        public int Count => Spawned.Count;

        public abstract void Spawn();

        public void Tick()
        {
        }

        public void Reset()
        {
            foreach (GameObject spawned in Spawned)
            {
                if (spawned != null)
                {
                    Object.Destroy(spawned);
                }
            }
            Spawned.Clear();
        }
    }
}

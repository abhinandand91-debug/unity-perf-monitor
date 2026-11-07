using UnityEngine;

namespace PerfMonitorDemo.Stressors
{
    // Drops physics cubes to load the CPU
    public sealed class CubeStressor : SpawnStressor
    {
        const int CubesPerClick = 100;

        public override string Name => "Physics cubes";

        public override void Spawn()
        {
            for (int i = 0; i < CubesPerClick; i++)
            {
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.position = new Vector3(Random.Range(-4f, 4f), Random.Range(6f, 14f), Random.Range(-1f, 3f));
                cube.transform.rotation = Random.rotation;
                cube.AddComponent<Rigidbody>();
                Spawned.Add(cube);
            }
        }
    }
}

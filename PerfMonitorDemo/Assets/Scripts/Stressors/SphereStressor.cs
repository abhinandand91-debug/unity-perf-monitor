using UnityEngine;

namespace PerfMonitorDemo.Stressors
{
    // Adds spheres with their own material each, which raises draw calls and GPU work
    public sealed class SphereStressor : SpawnStressor
    {
        const int SpheresPerClick = 100;

        public override string Name => "Spheres";

        public override void Spawn()
        {
            for (int i = 0; i < SpheresPerClick; i++)
            {
                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                Object.Destroy(sphere.GetComponent<Collider>());
                sphere.transform.position = new Vector3(Random.Range(-6f, 6f), Random.Range(0.5f, 6f), Random.Range(-1f, 8f));
                sphere.GetComponent<Renderer>().material.color = Random.ColorHSV(0f, 1f, 0.6f, 1f, 0.7f, 1f);
                Spawned.Add(sphere);
            }
        }
    }
}

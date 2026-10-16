using UnityEngine;

namespace PerfMonitorDemo.Stressors
{
    // Stacks transparent full screen quads in front of the camera to load the GPU
    public sealed class OverdrawStressor : SpawnStressor
    {
        const int LayersPerClick = 10;
        const float LayerGap = 0.01f;

        readonly Material _material;

        public OverdrawStressor()
        {
            _material = new Material(Shader.Find("Sprites/Default"))
            {
                color = new Color(1f, 0.2f, 0.2f, 0.08f)
            };
        }

        public override string Name => "Overdraw layers";

        public override void Spawn()
        {
            Transform cameraTransform = Camera.main.transform;

            for (int i = 0; i < LayersPerClick; i++)
            {
                GameObject layer = GameObject.CreatePrimitive(PrimitiveType.Quad);
                Object.Destroy(layer.GetComponent<Collider>());
                layer.GetComponent<Renderer>().sharedMaterial = _material;
                layer.transform.SetParent(cameraTransform, false);
                layer.transform.localPosition = new Vector3(0f, 0f, 1f + Spawned.Count * LayerGap);
                layer.transform.localScale = new Vector3(4f, 3f, 1f);
                Spawned.Add(layer);
            }
        }
    }
}

using PerfMonitorDemo;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Menu command that builds the demo scene from code
public static class BuildDemoScene
{
    [MenuItem("Tools/Perf Monitor Demo/Build Demo Scene")]
    public static void Build()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var camGo = new GameObject("Main Camera") { tag = "MainCamera" };
        var cam = camGo.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.12f, 0.14f, 0.2f);
        camGo.AddComponent<AudioListener>();
        camGo.transform.position = new Vector3(0f, 7f, -14f);
        camGo.transform.rotation = Quaternion.Euler(18f, 0f, 0f);

        var lightGo = new GameObject("Directional Light");
        var light = lightGo.AddComponent<Light>();
        light.type = LightType.Directional;
        light.shadows = LightShadows.Soft;
        lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "Floor";
        floor.transform.localScale = new Vector3(14f, 1f, 16f);
        floor.transform.position = new Vector3(0f, -0.5f, 3f);
        foreach (var side in new[] { -1f, 1f })
        {
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = "Wall";
            wall.transform.localScale = new Vector3(1f, 8f, 16f);
            wall.transform.position = new Vector3(side * 7.5f, 3.5f, 3f);
            wall.GetComponent<Renderer>().enabled = false;
        }

        new GameObject("StressTestPanel").AddComponent<StressTestPanel>();

        const string path = "Assets/Scenes/Demo.unity";
        EditorSceneManager.SaveScene(scene, path);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(path, true) };
        UnityEngine.Debug.Log("Demo scene saved to " + path);
    }
}

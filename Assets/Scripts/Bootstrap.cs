using UnityEngine;
using UnityEngine.Rendering.Universal;

public static class Bootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Init()
    {
        if (Object.FindAnyObjectByType<GameManager>() != null) return;

        var cam = Camera.main;
        if (cam == null)
        {
            var camGo = new GameObject("Main Camera") { tag = "MainCamera" };
            cam = camGo.AddComponent<Camera>();
            camGo.AddComponent<AudioListener>();
        }
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        cam.transform.position = new Vector3(0f, 0f, -10f);
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.42f, 0.68f, 0.90f);
        GameManager.Cam = cam;

        if (Object.FindAnyObjectByType<Light2D>() == null)
        {
            var light = new GameObject("Global Light 2D").AddComponent<Light2D>();
            light.lightType = Light2D.LightType.Global;
            light.intensity = 1f;
        }

        BuildWorld();

        var ui = new GameObject("UI").AddComponent<UIManager>();
        var gm = new GameObject("Game").AddComponent<GameManager>();
        gm.ui = ui;
        gm.shooter = new GameObject("Shooter").AddComponent<PlayerShooter>();
        gm.spawner = new GameObject("Spawner").AddComponent<DuckSpawner>();
    }

    static void BuildWorld()
    {
        Band("Grass", new Color(0.14f, 0.40f, 0.17f), -5.2f, 2.6f, -12);
        Band("GrassMid", new Color(0.19f, 0.52f, 0.21f), -4.2f, 1.1f, -11);
        Band("GrassTop", new Color(0.26f, 0.62f, 0.26f), -3.85f, 0.45f, -10);
        Band("Hill", new Color(0.30f, 0.60f, 0.42f, 0.55f), -2.9f, 2.2f, -20);
    }

    static void Band(string name, Color color, float y, float height, int order)
    {
        var go = new GameObject(name);
        go.transform.position = new Vector3(0f, y, 0f);
        go.transform.localScale = new Vector3(40f, height, 1f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteFactory.Square;
        sr.color = color;
        sr.sortingOrder = order;
    }
}

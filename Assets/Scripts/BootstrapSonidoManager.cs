using UnityEngine;

public static class AudioBootstrapRuntime
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        if (SonidoManager.Instance != null) return;

        const string path = "Prefabs/SonidoManager"; // ruta relativa dentro de Assets/Resources
        GameObject prefab = Resources.Load<GameObject>(path);
        if (prefab == null)
        {
            Debug.LogWarning($"[AudioBootstrapRuntime] Prefab no encontrado en Assets/Resources/{path}.prefab");
            return;
        }

        Object.Instantiate(prefab);
        Debug.Log("[AudioBootstrapRuntime] Instanciado SonidoManager automáticamente desde Resources/" + path);
    }
}


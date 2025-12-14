using UnityEngine;

public static class AudioBootstrapRuntime
{
    // Método que se ejecuta antes de cargar la primera escena
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        if (SonidoManager.Instance != null) return;

        // Carga e instancia el SonidoManager desde Resources si no existe
        const string path = "Prefabs/SonidoManager";
        GameObject prefab = Resources.Load<GameObject>(path);
        if (prefab == null)
        {
            return;
        }

        Object.Instantiate(prefab);
    }
}
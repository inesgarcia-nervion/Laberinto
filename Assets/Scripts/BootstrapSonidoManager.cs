using UnityEngine;

public class Instanciar : MonoBehaviour
{
    [Tooltip("Ruta dentro de Resources (ej: Prefabs/SonidoManager)")]
    [SerializeField] private string prefabPath = "External Files/Resources/Prefabs/SonidoManager";

    void Awake()
    {
        // Si ya hay instancia activa, nada que hacer
        if (SonidoManager.Instance != null) {
            Debug.Log("[AudioBootstrap] Ya existe SonidoManager en la escena.");
            return; 
        }

        GameObject prefab = Resources.Load<GameObject>(prefabPath);
        if (prefab == null)
        {
            Debug.LogWarning($"[AudioBootstrap] Prefab no encontrado en Resources/{prefabPath}");
            return;
        }

        Instantiate(prefab);
        Debug.Log("[AudioBootstrap] Instanciado SonidoManager desde Resources/" + prefabPath);

        if (SonidoManager.Instance != null)
        {
            SonidoManager.Instance.PlayMusic();
            SonidoManager.Instance.PlayAmbient();
            Debug.Log("[AudioBootstrap] PlayMusic() y PlayAmbient() llamados.");
        }
        else
        {
            Debug.LogWarning("[AudioBootstrap] Instanciado pero SonidoManager.Instance sigue siendo null.");
        }
    }
}


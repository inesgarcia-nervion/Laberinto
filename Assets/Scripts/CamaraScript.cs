using UnityEngine;

public class CamaraScript : MonoBehaviour
{
    public Transform player;        // Objetivo a seguir
    public float smoothSpeed = 0.125f; // Suavidad del movimiento de cámara
    public Vector3 offset;          // Desplazamiento respecto al objetivo

    void Awake()
    {
        // Añade AudioListener si no existe ninguno en la escena (Método actualizado)
        if (Object.FindFirstObjectByType<AudioListener>() == null)
        {
            gameObject.AddComponent<AudioListener>();
        }
    }

    void LateUpdate()
    {
        // Calcula la posición deseada siguiendo al jugador
        Vector3 desiredPosition = new Vector3(player.position.x, player.position.y, transform.position.z) + offset;

        // Interpola suavemente hacia la posición deseada
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        transform.position = smoothedPosition;
    }
}
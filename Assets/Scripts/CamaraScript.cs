using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    public Transform player;        // Referencia al jugador
    public float smoothSpeed = 0.125f; // Suavizado
    public Vector3 offset;          // Distancia respecto al jugador, normalmente (0,0,-10)

    void LateUpdate()
    {
        // Posición deseada de la cámara (solo X e Y siguen al jugador)
        Vector3 desiredPosition = new Vector3(player.position.x, player.position.y, transform.position.z) + offset;

        // Suavizado de la posición
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // Actualizar la posición de la cámara
        transform.position = smoothedPosition;
    }
}

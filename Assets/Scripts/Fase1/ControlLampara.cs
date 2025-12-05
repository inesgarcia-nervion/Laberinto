using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ControlLampara : MonoBehaviour
{
    // --- Referencias que debes arrastrar desde el Inspector de Unity ---
    
    [Header("Referencias de Componentes")]
    // Componente Light 2D/3D (la luz cuyo color cambiará)
    public Light2D luzDeEscena; 
    
    [Header("Configuración de la Acción")]
    // Nuevo color que la luz adoptará al activar
    public Color nuevoColor = Color.red; 
    
    // Define la tecla de interacción (E)
    public KeyCode teclaDeActivacion = KeyCode.E; 
    
    // ---------------------------------------------------------------------
    // VARIABLES DE CONTROL DE ESTADO
    // ---------------------------------------------------------------------
    
    // Bandera para asegurar que la acción solo se ejecute una vez
    private bool yaActivado = false; 
    
    // Bandera para saber si el jugador está dentro del área de interacción
    private bool jugadorEstaCerca = false; 
    
    // ---------------------------------------------------------------------
    
    void Update()
    {
        // 1. Verificación de doble condición:
        //    a) El jugador debe estar dentro del rango (jugadorEstaCerca es true)
        //    b) La acción no se ha ejecutado ya (no yaActivado)
        //    c) La tecla 'E' acaba de ser presionada (Input.GetKeyDown)
        
        if (!yaActivado && jugadorEstaCerca && Input.GetKeyDown(teclaDeActivacion))
        {
            ActivarEvento();
        }
    }

    // Se activa cuando otro Collider entra en el área de trigger (rango de interacción)
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Asume que tu jugador tiene la etiqueta "Player"
        if (other.gameObject.CompareTag("Player")) 
        {
            jugadorEstaCerca = true;
            Debug.Log("Jugador cerca. Pulsa E para interactuar.");
            // Opcional: Muestra un mensaje en pantalla al jugador
        }
    }

    // Se activa cuando otro Collider sale del área de trigger
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            jugadorEstaCerca = false;
            Debug.Log("Jugador se ha alejado.");
            // Opcional: Oculta el mensaje en pantalla
        }
    }

    void ActivarEvento()
    {
        // 1. Desaparecer el Sprite de la lámpara
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.enabled = false;
        }
        
        // 2. Cambiar el color de la iluminación
        if (luzDeEscena != null)
        {
            luzDeEscena.color = nuevoColor;
        }
        
        // 3. Marcar la acción como completada y deshabilitar el collider
        yaActivado = true;
        
        // Desactivar el Collider de Trigger para evitar interacciones futuras
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D col in colliders)
        {
            col.enabled = false;
        }
        
        Debug.Log("¡Lámpara ACTIVADA y DESACTIVADA!");
    }
}
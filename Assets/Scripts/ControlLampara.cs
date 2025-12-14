using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ControlLampara : MonoBehaviour
{
    // --- Referencias ---
    [Header("Referencias de Componentes")]
    public Light2D luzDeEscena; // Luz global a modificar

    [Header("Configuración de la Acción")]
    public Color nuevoColor = Color.red; // Color al activar
    public KeyCode teclaDeActivacion = KeyCode.E; // Tecla para interactuar

    // --- Variables Privadas ---
    private bool yaActivado = false; // Evita doble activación
    private bool jugadorEstaCerca = false;

    private GameManager gameManager;
    private Timer timer;

    void Start()
    {
        // Busca el GameManager usando métodos nuevos
        if (GameManager.Instance != null)
            gameManager = GameManager.Instance;
        else
            gameManager = Object.FindFirstObjectByType<GameManager>();

        timer = Object.FindFirstObjectByType<Timer>();

        if (gameManager == null)
        {
            Debug.LogError("¡No se ha encontrado el GameManager en la escena!");
        }
    }

    void Update()
    {
        if (!yaActivado && jugadorEstaCerca && Input.GetKeyDown(teclaDeActivacion))
        {
            ActivarEvento();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            jugadorEstaCerca = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            jugadorEstaCerca = false;
        }
    }

    void ActivarEvento()
    {
        // Oculta la lámpara y cambia el color de la luz
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = false;

        if (luzDeEscena != null) luzDeEscena.color = nuevoColor;

        yaActivado = true;

        // Si es el final, guarda el tiempo
        if (gameManager != null && gameManager.EsUltimaFase())
        {
            if (timer == null) timer = Object.FindFirstObjectByType<Timer>();
            if (timer != null)
            {
                timer.FinalizarNivel();
            }
        }

        // Avanza de fase o escena
        if (gameManager != null)
        {
            Debug.Log("Llamando a PasarDeFase...");
            gameManager.PasarDeFase();
        }

        // Desactiva colisiones
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D col in colliders)
        {
            col.enabled = false;
        }
    }
}
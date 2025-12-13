using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ControlLampara : MonoBehaviour
{
    // --- Referencias ---
    [Header("Referencias de Componentes")]
    public Light2D luzDeEscena;

    [Header("Configuración de la Acción")]
    public Color nuevoColor = Color.red;
    public KeyCode teclaDeActivacion = KeyCode.E;

    // --- Variables Privadas ---
    private bool yaActivado = false;
    private bool jugadorEstaCerca = false;

    // Referencia al script que controla las fases y el tiempo
    private GameManager gameManager;
    private Timer timer;

    void Start()
    {
        // Preferimos la instancia singleton si existe
        if (GameManager.Instance != null)
            gameManager = GameManager.Instance;
        else
            gameManager = FindObjectOfType<GameManager>();

        timer = FindObjectOfType<Timer>();

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
            // Opcional: Mostrar UI de "Pulsa E"
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
        // 1. Efectos visuales de la lámpara (opcional si la fase va a desaparecer inmediatamente)
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = false;

        if (luzDeEscena != null) luzDeEscena.color = nuevoColor;

        // 2. Marcar como activado para que no se pulse dos veces
        yaActivado = true;

        // 3. Si es la fase final, asegurar que el Timer guarde el tiempo antes de avanzar
        if (gameManager != null && gameManager.IsLastPhase())
        {
            if (timer == null) timer = FindObjectOfType<Timer>();
            if (timer != null)
            {
                timer.FinalizarNivel(); // guarda el tiempo en PlayerPrefs
            }
        }

        // 4. LLAMAR AL GAMEMANAGER PARA CAMBIAR DE FASE
        if (gameManager != null)
        {
            Debug.Log("Llamando a PasarDeFase...");
            gameManager.PasarDeFase();
        }

        // 5. Desactivar colliders
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D col in colliders)
        {
            col.enabled = false;
        }
    }
}
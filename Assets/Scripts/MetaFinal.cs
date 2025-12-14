using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MetaFinal : MonoBehaviour
{
    [Header("Configuración UI")]
    public GameObject objetoFinJuego; // Referencia al panel de Fin de Juego
    public float tiempoDeEspera = 3.0f; // Tiempo antes de volver al menú

    [Header("Configuración Escena")]
    public string nombreEscenaMenu = "Menú";

    private bool playerInRange = false; // Indica si el jugador está en el trigger

    private bool juegoTerminado = false;

    void Update()
    {
        if (!juegoTerminado && playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            juegoTerminado = true;

            // Busca el Timer usando el nuevo método optimizado de Unity
            Timer timer = Object.FindFirstObjectByType<Timer>();
            if (timer != null)
            {
                // Guarda el tiempo final
                timer.FinalizarNivel();
            }

            StartCoroutine(SecuenciaVictoria());
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }

    IEnumerator SecuenciaVictoria()
    {
        Time.timeScale = 0f;

        // Detiene el contador de tiempo si existe
        if (GameManager.Instance != null)
        {
            Timer timer = Object.FindFirstObjectByType<Timer>();
            if (timer != null) timer.FinalizarNivel();
        }

        // Muestra el mensaje de victoria
        if (objetoFinJuego != null)
        {
            objetoFinJuego.SetActive(true);
        }

        // Espera unos segundos
        yield return new WaitForSecondsRealtime(tiempoDeEspera);

        // Destruye el GameManager para reiniciar datos en la próxima partida
        if (GameManager.Instance != null)
        {
            Destroy(GameManager.Instance.gameObject);
        }

        // Volver a poner el tiempo a 1 antes de cambiar de escena, para que el menú no se congele
        Time.timeScale = 1f;

        // Vuelve al menú principal (Índice 0)
        SceneManager.LoadScene(0);
    }
}
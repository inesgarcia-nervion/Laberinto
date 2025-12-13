using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MetaFinal : MonoBehaviour
{
    [Header("Configuración UI")]
    public GameObject objetoFinJuego; // Arrastra aquí el objeto 'finJuego' del Canvas
    public float tiempoDeEspera = 3.0f;

    [Header("Configuración Escena")]
    public string nombreEscenaMenu = "Menú"; // O el índice 0

    private bool metaAlcanzada = false;
    private bool playerInRange = false;

    // Update is called once per frame
    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            Timer timer = FindObjectOfType<Timer>();
            if (timer != null)
            {
                // Esto ejecuta tu función GuardarEnLista() del script Timer
                timer.FinalizarNivel();
                Debug.Log("Tiempo guardado correctamente.");
            }

            StartCoroutine(SecuenciaVictoria());
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            metaAlcanzada = true;
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            metaAlcanzada = false;
            playerInRange = false;
        }
    }

    IEnumerator SecuenciaVictoria()
    {
        // 1. Detener el Cronómetro (opcional, para que deje de contar mientras celebras)
        if (GameManager.Instance != null)
        {
            // Podrías añadir un método en GameManager para pausar, 
            // o buscar el script del Timer y pararlo:
            Timer timer = FindObjectOfType<Timer>();
            if (timer != null) timer.FinalizarNivel();
        }

        // 2. Mostrar el mensaje
        if (objetoFinJuego != null)
        {
            objetoFinJuego.SetActive(true);
        }

        // 3. Esperar 3 segundos
        yield return new WaitForSeconds(tiempoDeEspera);

        // 4. Limpiar el GameManager (IMPORTANTE)
        // Como el juego acabó, destruimos el GameManager para que si vuelves a jugar
        // empieces con vidas y tiempo de 0, no con los datos viejos.
        if (GameManager.Instance != null)
        {
            Destroy(GameManager.Instance.gameObject);
        }

        // 5. Cargar el Menú Principal (Asumiendo que es la escena 0)
        SceneManager.LoadScene(0);
    }
}
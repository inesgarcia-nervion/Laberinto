using System.Linq;
using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Singleton del GameManager
    public static GameManager Instance { get; private set; }

    [Header("UI")]
    public TMP_Text textoTiempo; // Referencia al texto del temporizador
    public string timerTag = "Timer"; // Tag para buscar el temporizador

    [Header("Estado persistente")]
    public int playerLives = 2; // Vidas guardadas entre escenas

    [Header("Sistema de Fases (opcional, en la misma escena)")]
    public GameObject[] fases;
    private int faseActual = 0;

    private float tiempoInicio;
    private bool inicializado = false;

    void Awake()
    {
        // Configuración Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (!inicializado)
        {
            tiempoInicio = Time.time;
            inicializado = true;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        if (textoTiempo == null)
            ReasignarTimer();

        // Activa solo la fase actual si se usa el sistema de fases
        if (fases != null && fases.Length > 0)
        {
            for (int i = 0; i < fases.Length; i++)
                fases[i].SetActive(i == faseActual);
        }
    }

    void Update()
    {
        // Actualiza el texto del tiempo en pantalla
        if (textoTiempo != null)
        {
            float tiempoTranscurrido = Time.time - tiempoInicio;
            string t = string.Format("{0:0}:{1:00}", Mathf.Floor(tiempoTranscurrido / 60), Mathf.Floor(tiempoTranscurrido % 60));
            textoTiempo.text = "Tiempo: " + t;
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Busca referencias y reinicia estado al cargar escena
        if (textoTiempo == null)
            ReasignarTimer();

        fases = null;
        faseActual = 0;

       
        // Busca al jugador con el método nuevo
        var player = Object.FindFirstObjectByType<Player>();
        if (player != null)
        {
            player.salud = playerLives;
            player.ActualizaHud();
        }
    }

    void ReasignarTimer()
    {
        textoTiempo = null;
        // Intenta encontrar el texto del temporizador por Tag
        if (!string.IsNullOrEmpty(timerTag))
        {
            var go = GameObject.FindWithTag(timerTag);
            if (go != null)
            {
                var tmp = go.GetComponent<TMP_Text>();
                if (tmp != null)
                {
                    textoTiempo = tmp;
                }
            }
        }
    }

    // Devuelve el tiempo total jugado desde el inicio
    public float ObtenerTiempoTranscurrido()
    {
        if (!inicializado) return 0f;
        return Time.time - tiempoInicio;
    }

    public void EstablecerVidasJugador(int lives)
    {
        playerLives = lives;
    }

    public void ReiniciarTemporizador()
    {
        tiempoInicio = Time.time;
    }

    public void PasarDeFase()
    {
        // Avanza fase en la misma escena si existen fases configuradas
        if (fases != null && fases.Length > 0)
        {
            if (faseActual >= 0 && faseActual < fases.Length)
                fases[faseActual].SetActive(false);

            faseActual++;

            if (faseActual < fases.Length)
            {
                fases[faseActual].SetActive(true);
                return;
            }

        }

        // Si no hay más fases, carga la siguiente escena
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextIndex = currentSceneIndex + 1;

        if (nextIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextIndex);
        }

    }

    public bool EsUltimaFase()
    {
        // Comprueba si es la última fase (local o escena)
        if (fases != null && fases.Length > 0)
        {
            return faseActual >= fases.Length - 1;
        }

        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        return currentSceneIndex + 1 >= SceneManager.sceneCountInBuildSettings;
    }

}
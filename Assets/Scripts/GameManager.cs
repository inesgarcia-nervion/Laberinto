using System.Linq;
using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI")]
    public TMP_Text textoTiempo;
    public string timerTag = "Timer";

    [Header("End Game UI")]
    public TMP_Text endGameText;           // Asignar en inspector o poner tag "EndGameText" en la escena
    public string endGameTag = "EndGameText";
    public float endGameDelay = 3f;       // segundos antes de volver al menú

    [Header("Estado persistente")]
    public int playerLives = 2;

    [Header("Sistema de Fases (opcional, en la misma escena)")]
    public GameObject[] fases;
    private int faseActual = 0;

    private float tiempoInicio;
    private bool inicializado = false;

    void Awake()
    {
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

        if (fases != null && fases.Length > 0)
        {
            for (int i = 0; i < fases.Length; i++)
                fases[i].SetActive(i == faseActual);
        }
    }

    void Update()
    {
        if (textoTiempo != null)
        {
            float tiempoTranscurrido = Time.time - tiempoInicio;
            string t = string.Format("{0:0}:{1:00}", Mathf.Floor(tiempoTranscurrido / 60), Mathf.Floor(tiempoTranscurrido % 60));
            textoTiempo.text = "Tiempo: " + t;
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (textoTiempo == null)
            ReasignarTimer();

        // Intentar reasignar el texto final si existe en la escena
        if (endGameText == null && !string.IsNullOrEmpty(endGameTag))
        {
            var go = GameObject.FindWithTag(endGameTag);
            if (go != null) endGameText = go.GetComponent<TMP_Text>();
        }

        var scenePhases = GameObject.FindGameObjectsWithTag("PhaseRoot");
        if (scenePhases != null && scenePhases.Length > 0)
        {
            fases = scenePhases.OrderBy(g => g.name).ToArray();

            if (faseActual < 0) faseActual = 0;
            if (faseActual >= fases.Length) faseActual = fases.Length - 1;

            for (int i = 0; i < fases.Length; i++)
                fases[i].SetActive(i == faseActual);
        }

        var player = FindObjectOfType<Player>();
        if (player != null)
        {
            player.salud = playerLives;
            player.ActualizaHud();
        }
    }

    void ReasignarTimer()
    {
        if (!string.IsNullOrEmpty(timerTag))
        {
            var go = GameObject.FindWithTag(timerTag);
            if (go != null)
            {
                var tmp = go.GetComponent<TMP_Text>();
                if (tmp != null)
                {
                    textoTiempo = tmp;
                    return;
                }
            }
        }

        foreach (var tmp in FindObjectsOfType<TMP_Text>())
        {
            if ((tmp.name != null && tmp.name.ToLower().Contains("tiemp")) ||
                (tmp.text != null && tmp.text.ToLower().Contains("tiemp")))
            {
                textoTiempo = tmp;
                return;
            }
        }
    }

    // Fuente de verdad del tiempo transcurrido
    public float GetElapsedTime()
    {
        if (!inicializado) return 0f;
        return Time.time - tiempoInicio;
    }

    public void SetPlayerLives(int lives)
    {
        playerLives = lives;
    }

    public void ResetTimer()
    {
        tiempoInicio = Time.time;
    }

    public void PasarDeFase()
    {
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
            else
            {
                // Se completaron las fases definidas en la escena -> fin del juego
                StartCoroutine(ShowEndAndReturn());
                return;
            }
        }

        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextIndex = currentSceneIndex + 1;

        if (nextIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextIndex);
        }
        else
        {
            // No hay más escenas en el build -> fin del juego
            StartCoroutine(ShowEndAndReturn());
        }
    }

    IEnumerator ShowEndAndReturn()
    {
        // Asegurarse de que el Timer ha guardado el tiempo (Player.RecolectarLampara llama a Timer.FinalizarNivel antes)
        // Recuperar mejor tiempo desde PlayerPrefs
        string best = GetBestTimeFormatted();

        if (endGameText != null)
        {
            endGameText.gameObject.SetActive(true);
            endGameText.text = "¡Enhorabuena! Has completado el juego!\nRécord: " + best;
        }
        else
        {
            Debug.Log("¡Enhorabuena! Has completado el juego! Récord: " + best);
        }

        yield return new WaitForSecondsRealtime(endGameDelay);

        // Volver al menú (escena índice 0)
        SceneManager.LoadScene(0);
    }

    string GetBestTimeFormatted()
    {
        string data = PlayerPrefs.GetString("TablaTiempos", "");
        if (string.IsNullOrEmpty(data)) return "00:00";

        string[] parts = data.Split(',');
        float best = float.MaxValue;
        foreach (var p in parts)
        {
            if (float.TryParse(p, out float v))
            {
                if (v < best) best = v;
            }
        }

        if (best == float.MaxValue) return "00:00";

        // Usar mismo formato que MenuInicial (añade +1 como allí para consistencia)
        best += 1f;
        int min = Mathf.FloorToInt(best / 60f);
        int seg = Mathf.FloorToInt(best % 60f);
        return string.Format("{0:00}:{1:00}", min, seg);
    }


    public bool IsLastPhase()
    {
        // Si gestionas fases por GameObjects en la misma escena
        if (fases != null && fases.Length > 0)
        {
            return faseActual >= fases.Length - 1;
        }

        // Si avanzas por escenas en el build index
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        return currentSceneIndex + 1 >= SceneManager.sceneCountInBuildSettings;
    }

}
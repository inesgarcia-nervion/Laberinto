using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI timerText; // Texto en pantalla del cronómetro

    private float tiempoTranscurrido; // Tiempo en segundos
    private bool cronometroActivo = true; // Controla si el tiempo corre o para

    void Start()
    {
        // Sincroniza el tiempo con el GameManager si existe, o inicia en 0
        if (GameManager.Instance != null)
        {
            tiempoTranscurrido = GameManager.Instance.ObtenerTiempoTranscurrido();
            cronometroActivo = true;
        }
        else
        {
            tiempoTranscurrido = 0f;
            cronometroActivo = true;
        }
    }

    void Update()
    {
        if (!cronometroActivo) return;

        // Usa el GameManager como fuente principal de tiempo si está disponible
        if (GameManager.Instance != null)
        {
            tiempoTranscurrido = GameManager.Instance.ObtenerTiempoTranscurrido();
        }
        else
        {
            tiempoTranscurrido += Time.deltaTime;
        }

        ActualizarTextoTimer(tiempoTranscurrido);
    }

    void ActualizarTextoTimer(float tiempo)
    {
        // Convierte el tiempo a formato MM:SS
        float minutos = Mathf.FloorToInt(tiempo / 60);
        float segundos = Mathf.FloorToInt(tiempo % 60);

        if (timerText != null)
        {
            timerText.text = string.Format("{0:00}:{1:00}", minutos, segundos);
        }
    }

    public void FinalizarNivel()
    {
        if (cronometroActivo)
        {
            cronometroActivo = false;
            GuardarEnLista();
        }
    }

    void GuardarEnLista()
    {
        float tiempoAGuardar = tiempoTranscurrido;

        // Asegura obtener el tiempo correcto del GameManager
        if (GameManager.Instance != null)
            tiempoAGuardar = GameManager.Instance.ObtenerTiempoTranscurrido();

        // Recupera tiempos guardados, añade el nuevo y guarda
        string tiemposPrevios = PlayerPrefs.GetString("TablaTiempos", "");

        if (!string.IsNullOrEmpty(tiemposPrevios))
        {
            tiemposPrevios += ",";
        }

        tiemposPrevios += tiempoAGuardar.ToString();

        PlayerPrefs.SetString("TablaTiempos", tiemposPrevios);
        PlayerPrefs.Save();
    }
}
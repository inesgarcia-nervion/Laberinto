using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI timerText;

    private float tiempoTranscurrido;
    private bool cronometroActivo = true;

    void Start()
    {
        // Si existe GameManager persistente, sincronizamos el tiempo inicial
        if (GameManager.Instance != null)
        {
            tiempoTranscurrido = GameManager.Instance.GetElapsedTime();
            cronometroActivo = true;
        }
        else
        {
            // Solo si no hay GM (pruebas) empieza de 0
            tiempoTranscurrido = 0f;
            cronometroActivo = true;
        }
    }

    void Update()
    {
        if (!cronometroActivo) return;

        if (GameManager.Instance != null)
        {
            // Fuente de verdad: GameManager
            tiempoTranscurrido = GameManager.Instance.GetElapsedTime();
        }
        else
        {
            // Fallback local si no hay GameManager
            tiempoTranscurrido += Time.deltaTime;
        }

        ActualizarTextoTimer(tiempoTranscurrido);
    }

    void ActualizarTextoTimer(float tiempo)
    {
        // Eliminado el +1: mostrará el tiempo real
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

        // Si existe GameManager usamos su tiempo por seguridad
        if (GameManager.Instance != null)
            tiempoAGuardar = GameManager.Instance.GetElapsedTime();

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
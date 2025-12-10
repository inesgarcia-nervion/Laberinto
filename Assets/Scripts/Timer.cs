using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI timerText;

    private float tiempoTranscurrido;
    private bool cronometroActivo;

    void Start()
    {
        cronometroActivo = true;
        tiempoTranscurrido = 0f;
    }

    void Update()
    {
        if (cronometroActivo)
        {
            tiempoTranscurrido += Time.deltaTime;
            ActualizarTextoTimer(tiempoTranscurrido);
        }
    }

    void ActualizarTextoTimer(float tiempo)
    {
        tiempo += 1;
        float minutos = Mathf.FloorToInt(tiempo / 60);
        float segundos = Mathf.FloorToInt(tiempo % 60);

        if (timerText != null)
        {
            timerText.text = string.Format("{0:00}:{1:00}", minutos, segundos);
        }
    }

    // Llamado cuando el jugador recolecta la lámpara
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
        // Recuperamos la lista actual
        string tiemposPrevios = PlayerPrefs.GetString("TablaTiempos", "");

        if (!string.IsNullOrEmpty(tiemposPrevios))
        {
            tiemposPrevios += ",";
        }

        // Añadimos el nuevo tiempo al final
        tiemposPrevios += tiempoTranscurrido.ToString();

        // Guardamos la nueva lista completa
        PlayerPrefs.SetString("TablaTiempos", tiemposPrevios);
        PlayerPrefs.Save();

    }
}